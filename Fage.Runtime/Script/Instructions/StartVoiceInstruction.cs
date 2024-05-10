using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class StartVoiceInstruction(StartVoice instantizationInfo) : ScriptInstruction(ScriptInstructionOpcode.StartVoice)
{

	public string Name { get; } = instantizationInfo.Name;

	public override void Handle(ScriptingEnvironment env)
	{
		env.Audio.VoiceChannel.NextVoice(Name);
	}
}
