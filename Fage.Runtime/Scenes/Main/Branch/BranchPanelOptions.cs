namespace Fage.Runtime.Scenes.Main.Branch;

public class BranchPanelOptions
{
	public string OptionBackground { get; set; } = "branch-option-background";
	public string OptionHoverBackground { get; set; } = "branch-option-hover-background";
	public string OptionPressedBackground { get; set; } = "branch-option-pressed-background";

	public Color OptionTextColor { get; set; } = Color.White;
	public Color OptionHoverTextColor { get; set; } = Color.White;
	public Color OptionPressedTextColor { get; set; } = Color.White;

	public float HintTextSize { get; set; } = 18f;
}