using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class AddCharacterInstruction(AddCharacter instantizationInfo)
	: ScriptInstruction(ScriptInstructionOpcode.AddCharacter)
{
	public string Identity { get; } = instantizationInfo.Identity;
	public string? Motion { get; } = instantizationInfo.Motion;
	public int? Position { get; } = instantizationInfo.Position;
	public override void Handle(ScriptingEnvironment env)
	{
		env.Characters.AddCharacter(Identity, Motion, Position);
	}
}
