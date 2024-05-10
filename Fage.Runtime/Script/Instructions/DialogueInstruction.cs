using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class DialogueInstruction : ScriptInstruction
{
	public string Content { get; }

	public bool CompletesParagraph { get; }

	public DialogueInstruction(Dialogue instantizationInfo)
		: base(ScriptInstructionOpcode.Dialogue)
	{
		BlocksExecution = true;
		Content = instantizationInfo.Content;
		CompletesParagraph = instantizationInfo.CompletesParagraph;
	}
	public override void Handle(ScriptingEnvironment env)
	{
		env.TextWriter.PushNewLine(Content);
		if (CompletesParagraph)
			env.TextWriter.CompleteParagraph();
	}
}
