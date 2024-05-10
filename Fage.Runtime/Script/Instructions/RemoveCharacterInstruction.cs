using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class RemoveCharacterInstruction(RemoveCharacter instantizationInfo)
	: ScriptInstruction(ScriptInstructionOpcode.RemoveCharacter)
{
	public string Identity { get; } = instantizationInfo.Identity;
	public bool TryUnloadResources { get; } = instantizationInfo.TryUnloadResources;

	public override void Handle(ScriptingEnvironment env)
	{
		env.Characters.RemoveCharacter(Identity, TryUnloadResources);
	}
}
