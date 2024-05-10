using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;
using System.Diagnostics;

namespace Fage.Runtime.Script.Instructions;

public class JumpToAnchorInstruction(JumpToAnchor instantizationInfo)
	: ScriptInstruction(ScriptInstructionOpcode.JumpToAnchor)
{
	public string? Script { get; } = instantizationInfo.Script;

	public string Anchor { get; } = instantizationInfo.Anchor;

	public bool IsThisScript { get; internal set; }

	public override void Handle(ScriptingEnvironment env)
	{
		if (IsThisScript)
		{
			Debug.Assert(Script == null || env.Script.Path == Script);
			env.JumpToAnchorThisScript(env.Script.Anchors[Anchor]); 
		}
		else
		{
			Debug.Assert(Script != null);
			env.JumpToAnchor(Anchor, Script);
		}
	}
}
