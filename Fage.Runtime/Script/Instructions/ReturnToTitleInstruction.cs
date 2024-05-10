using Fage.Script.Instruction;

namespace Fage.Runtime.Script.Instructions;

public class ReturnToTitleInstruction : ScriptInstruction
{
	public ReturnToTitleInstruction() : base(ScriptInstructionOpcode.ReturnToTitle)
	{
		BlocksExecution = true;
	}

	public override void Handle(ScriptingEnvironment env)
	{
		env.Game.SwitchToTitleScene();
	}
}