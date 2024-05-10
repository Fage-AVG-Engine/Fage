using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class SetBgmInstruction(SetBgm instantizationInfo) : ScriptInstruction(ScriptInstructionOpcode.SetBgm)
{

	public string Name { get; } = instantizationInfo.Name;

	public override void Handle(ScriptingEnvironment env)
	{
		env.Audio.BackgroundMusicChannel.SetNextBgm(Name);
	}
}
