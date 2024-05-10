using Fage.Script.Instruction;

namespace Fage.Runtime.Script.Instructions;

internal class ParagraphStartInstruction() : ScriptInstruction(ScriptInstructionOpcode.ParagraphStart)
{
	public override void Handle(ScriptingEnvironment env)
	{
		env.TextWriter.StartNewParagraph();
	}
}
