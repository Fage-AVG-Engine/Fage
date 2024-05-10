using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

public class CodeInstruction(Code instantization) : ScriptInstruction(ScriptInstructionOpcode.Code)
{
	public string Code { get; } = instantization.Content;
	public bool IsBlock { get; } = instantization.IsBlock;
	public int? LineNumber { get; } = instantization.LineNumber;
	public (int StartPosition, int EndPosition)? Span { get; } = instantization.Span;


	public override void Handle(ScriptingEnvironment env)
	{
		env.Lua.ExecuteCode(Code, env.Script.Path, LineNumber, Span);
	}
}
