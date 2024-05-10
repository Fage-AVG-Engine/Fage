using Fage.Runtime.Script.Instructions;
using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;
using Fage.Script.Serialization;
using System.Diagnostics;

namespace Fage.Runtime.Script;

public partial class FageScript
{
	private static class InstructionSequenceCache
	{
		private const int DefaultCapacity = 128;
		private const int MaxCapacity = 1024; // 看情况，不够可以调高
		private static List<ScriptInstruction>? _cachedInstance = new(DefaultCapacity);

		public static List<ScriptInstruction> Get()
		{
			List<ScriptInstruction>? cached = Interlocked.Exchange(ref _cachedInstance, null);
			cached ??= new(DefaultCapacity);
			return cached;
		}

		public static void Return(List<ScriptInstruction> list)
		{
			list.Clear();
			var oldValue = _cachedInstance;
			if (list.Count < MaxCapacity && list.Count > oldValue?.Count)
			{
				Interlocked.CompareExchange(ref _cachedInstance, list, oldValue);
			}
		}
	}

	private List<ScriptInstruction> ParseInstructionSequence(IReadOnlyList<ISerializedInstruction> instructionSequence)
	{
		List<ScriptInstruction> parsedSequence = InstructionSequenceCache.Get();
		InstructionInstantizer instantizer = new(this);

		for (int i = 0; i < instructionSequence.Count; i++)
		{
			var serialized = instructionSequence[i];
			ScriptInstruction parsedInstruction;

			switch (serialized.Opcode)
			{
				case ScriptInstructionOpcode.Dialogue:
				{
					ISerializedInstruction<Dialogue> serializedDialogue = (ISerializedInstruction<Dialogue>)serialized;
					var dialogueInstruction = new DialogueInstruction(serializedDialogue.InstantizationInfo);
					parsedInstruction = dialogueInstruction;
					break;
				}
				case ScriptInstructionOpcode.ParagraphStart:
				{
					parsedInstruction = new ParagraphStartInstruction();
					break;
				}
				case ScriptInstructionOpcode.StartVoice:
				{
					ISerializedInstruction<StartVoice> serializedStartVoice = (ISerializedInstruction<StartVoice>)serialized;
					parsedInstruction = new StartVoiceInstruction(serializedStartVoice.InstantizationInfo);
					break;
				}
				default:
				{
					parsedInstruction = instantizer.Instantize(serialized, i);
					break;
				}
			}

			parsedInstruction.InstructionIndex = i;
			parsedSequence.Add(parsedInstruction);
		}

		return parsedSequence;
	}
}

file class InstructionInstantizer
{
	internal static readonly ScriptInstructionOpcode[] IgnoredOpcodes
		= [
			ScriptInstructionOpcode.SetSpeakingCharacter,
		];

	private readonly Dictionary<ScriptInstructionOpcode,
		Func<ISerializedInstruction, ScriptInstruction>
	> _handlers;

	private readonly FageScript _partialScript;

	internal InstructionInstantizer(FageScript partialScript)
	{
		_handlers = RegisterHandlers();
		_partialScript = partialScript;
	}


	internal Dictionary<
		ScriptInstructionOpcode,
		Func<ISerializedInstruction, ScriptInstruction>
	> RegisterHandlers()
	{
		Dictionary<
			ScriptInstructionOpcode,
			Func<ISerializedInstruction, ScriptInstruction>
		> result = new(32)
		{
			[ScriptInstructionOpcode.Nop] = ParameterLess<NopInstruction>,

			// [ScriptInstructionOpcode.Dialogue] 高频指令，switch处理
			// [ScriptInstructionOpcode.ParagraphEnd] = 高频指令，switch处理

			// TODO 其它指令，可使用的指令不应出现在上面的IgnoreOpcodes中
			[ScriptInstructionOpcode.ReturnToTitle] = ParameterLess<ReturnToTitleInstruction>,
			[ScriptInstructionOpcode.Branch] = Branch,
			// [ScriptInstructionOpcode.BranchOption] = BranchOption 已移除,

			[ScriptInstructionOpcode.SetBackground] = SetBackground,

			[ScriptInstructionOpcode.AddCharacter] = AddCharacter,
			[ScriptInstructionOpcode.RemoveCharacter] = RemoveCharacter,

			[ScriptInstructionOpcode.Anchor] = Anchor,
			[ScriptInstructionOpcode.JumpToAnchor] = JumpToAnchor,

			[ScriptInstructionOpcode.SetBgm] = SetBgm,
			// [ScriptInstructionOpcode.StartVoice] switch处理
			[ScriptInstructionOpcode.StartSfx] = StartSfx,

			[ScriptInstructionOpcode.Code] = Code
		};

		return result;
	}

	internal ScriptInstruction Instantize(ISerializedInstruction serialized, int instructionIndex)
	{
		if (_handlers.TryGetValue(serialized.Opcode, out var instantizer))
		{
			var instruction = instantizer(serialized);
			instruction.InstructionIndex = instructionIndex;
			return instruction;
		}
		else
		{
			if (IgnoredOpcodes.Contains(serialized.Opcode))
				return ParameterLess<NopInstruction>(null);
			else
				throw new InvalidOperationException($"脚本文件{_partialScript.Path}中的第{instructionIndex}条指令不受支持。（操作码为{serialized.Opcode}）");
		}
	}

	internal TInstruction ParameterLess<TInstruction>(
		ISerializedInstruction? serialized
	) where TInstruction : ScriptInstruction
	{
		Debug.Assert(serialized == null 
			|| ReferenceEquals(serialized.InstantizationInfo, 
				Fage.Script.Instruction.Instantization.ParameterLess.Instance) 
			|| serialized.InstantizationInfo is ParameterLess, serialized != null
			? $"Opcode为{serialized.Opcode}的指令不接受参数" : "¿看一下自动参数");

		return (TInstruction?)Activator.CreateInstance<TInstruction>()
				?? throw new InvalidOperationException($"无法实例化指令{typeof(TInstruction).Name}");
	}

	internal static TInstantizationInfo AssertInstantizationType<TInstantizationInfo>(ISerializedInstruction serialized)
		where TInstantizationInfo : class, IInstructionInstantization
	{
		Type instantizationInfoType = serialized.InstantizationInfo.GetType();
		Debug.Assert(instantizationInfoType.IsAssignableTo(typeof(TInstantizationInfo)),
			$"指令 opcode 与实例化信息不符，实例化信息类型为{instantizationInfoType}");

		return (TInstantizationInfo)serialized.InstantizationInfo;
	}

	internal SetBackgroundInstruction SetBackground(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<SetBackground>(serialized);
		return new SetBackgroundInstruction(info);
	}

	internal AnchorInstruction Anchor(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<Anchor>(serialized);
		return new AnchorInstruction(info);
	}

	internal JumpToAnchorInstruction JumpToAnchor(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<JumpToAnchor>(serialized);

		return new JumpToAnchorInstruction(info)
		{
			IsThisScript = info.Script == null || info.Script == _partialScript.ScriptName
		};
	}

	internal SetBgmInstruction SetBgm(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<SetBgm>(serialized);

		return new SetBgmInstruction(info);
	}

	// internal StartVoiceInstruction StartVoice(ISerializedInstruction serialized)
	// 内联到ParseInstructionSequence

	internal StartSfxInstruction StartSfx(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<StartSfx>(serialized);

		return new StartSfxInstruction(info);
	}

	internal AddCharacterInstruction AddCharacter(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<AddCharacter>(serialized);

		return new AddCharacterInstruction(info);
	}

	internal RemoveCharacterInstruction RemoveCharacter(ISerializedInstruction serialized)
	{
		var info = AssertInstantizationType<RemoveCharacter>(serialized);

		return new RemoveCharacterInstruction(info);
	}

	internal BranchInstruction Branch(ISerializedInstruction serialized)
	{
		return new BranchInstruction(AssertInstantizationType<Branch>(serialized));
	}

	internal CodeInstruction Code(ISerializedInstruction serialized)
	{
		return new CodeInstruction(AssertInstantizationType<Code>(serialized));
	}
}
