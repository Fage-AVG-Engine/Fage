using Fage.Runtime.Script;

namespace Fage.Runtime.Scenes.Main.Branch;

public class BranchOption
{
	public required string HintText { get; set; }

	public required Action<ScriptingEnvironment> SelectedAction { get; set; }
}
