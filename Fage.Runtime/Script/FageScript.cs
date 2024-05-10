using Fage.Runtime.Script.Instructions;
using Fage.Script.Instruction;
using Fage.Script.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Fage.Runtime.Script;

public partial class FageScript
{
	public static readonly Version CurrentInstructionSetVersion = new("0.1.0");

	public IReadOnlyList<ScriptInstruction> InstructionSequence { get; }
	public IReadOnlyDictionary<string, int> Anchors { get; }

	public string Path { get; }
	public string ScriptName { get; }

	public Version InstructionSetVersion { get; }

	private int _currentPosition = -1;

	public bool NotCompleted => CurrentPosition >= -1 && CurrentPosition < InstructionSequence.Count - 1;

	public int CurrentPosition
	{
		get => _currentPosition;
		set
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(value, -1);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(value, InstructionSequence.Count);

			_currentPosition = value;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// 先调用<see cref="MoveNext"/>
	/// </remarks>
	public ScriptInstruction? Current { get; private set; } = null;

	public static bool IsScriptVersionSupported(Version scriptInstructionSetVersion)
	{
		//return scriptInstructionSetVersion.Major == CurrentInstructionSetVersion.Major
		//	&& scriptInstructionSetVersion.Minor <= CurrentInstructionSetVersion.Minor;

		// 脚本子系统正在开发中，api不稳定，稳定版 SemVer 兼容性判断方式不适用。
		return scriptInstructionSetVersion == CurrentInstructionSetVersion;
	}

	public FageScript(FageScriptInstantization instantization, string path, string scriptName)
	{
		Path = path;
		ScriptName = scriptName;
		InstructionSetVersion = instantization.InstructionSetVersion;

		InstructionSequence = ParseInstructionSequence(instantization.InstructionSequence);
		Anchors = ScanForAnchors(InstructionSequence);
	}

	private static Dictionary<string, int> ScanForAnchors(IReadOnlyList<ScriptInstruction> instructionSequence)
	{
		Dictionary<string, int> anchors = new(6);

		foreach (var anchor in instructionSequence.Where(i => i.Opcode == ScriptInstructionOpcode.Anchor)
			.Cast<AnchorInstruction>())
		{
			anchors.Add(anchor.Name, anchor.InstructionIndex);
		}

		return anchors;
	}

	public void CheckVersionSupported()
	{
		if (!IsScriptVersionSupported(InstructionSetVersion))
		{
			throw new NotSupportedException($"版本{InstructionSetVersion}不兼容，当前版本{CurrentInstructionSetVersion}");
		}
	}

	[MemberNotNullWhen(true, nameof(Current))]
	public bool MoveNext()
	{
		if (NotCompleted)
		{
			CurrentPosition++;
			Current = InstructionSequence[CurrentPosition];
			return true;
		}
		else
		{
			return false;
		}
	}

	public void Reset()
	{
		CurrentPosition = -1;
	}
}
