using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class StartSfxInstruction(StartSfx instantizationInfo) : ScriptInstruction(ScriptInstructionOpcode.StartSfx)
{
	public string Name { get; } = instantizationInfo.Name;

	public override void Handle(ScriptingEnvironment env)
	{
		env.Audio.SfxChannel.PlaySfx(Name);
	}
}
