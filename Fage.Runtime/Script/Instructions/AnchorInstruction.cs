using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;
using System.Diagnostics.CodeAnalysis;

namespace Fage.Runtime.Script.Instructions;

public class AnchorInstruction(Anchor instantizationInfo)
	: NoOpInstruction(ScriptInstructionOpcode.Anchor)
{
	public string Name { get; } = instantizationInfo.Name;

	public string? HeadingText { get; } = instantizationInfo.HeadingText;

	[MemberNotNullWhen(true, nameof(HeadingText))]
	public bool IsHeading => HeadingText != null;
}
