using Fage.Runtime.Scenes.Main.Branch;
using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;

namespace Fage.Runtime.Script.Instructions;

file static class BranchActions
{
	internal static Action<ScriptingEnvironment> MakeCode(string code)
	{
		return env =>
		{
			Console.WriteLine(code);
			throw new NotImplementedException();
		};
	}

	internal static Action<ScriptingEnvironment> MakeJump(string location, string? path)
	{
		return env =>
		{
			path ??= env.Script.ScriptName;

			env.JumpToAnchor(location, path);
		};
	}
}

public class BranchInstruction : ScriptInstruction
{
	internal BranchOption[] InternalOptions { get; private set; }

	public IReadOnlyList<BranchOption> Options => InternalOptions;

	public BranchInstruction(Branch instantization) : base(ScriptInstructionOpcode.Branch)
	{
		BlocksExecution = true;
		var serializedOptions = instantization.Options;

		InternalOptions = serializedOptions.Length != 0
			? new BranchOption[serializedOptions.Length] :
			[];


		for (int i = 0; i < InternalOptions.Length; i++)
		{
			var serializedOption = serializedOptions[i];
			Action<ScriptingEnvironment> action;

			if (serializedOption.Code != null)
			{
				action = BranchActions.MakeCode(serializedOption.Code);
			}
			else if (serializedOption.AnchorExpression != null)
			{
				ReadOnlySpan<char> span = serializedOption.AnchorExpression;

				int spaceIndex = span.IndexOf(' ');
				string location;
				string? path;
				if (spaceIndex == -1)
				{
					location = serializedOption.AnchorExpression;
					path = null;
				}
				else
				{
					location = span[..spaceIndex].ToString();
					path = span[(spaceIndex + 1)..].ToString();
				}

				action = BranchActions.MakeJump(location, path);
			}
			else
			{
				action = static env => { };
			}

			InternalOptions[i] = new BranchOption
			{
				HintText = serializedOptions[i].HintText,
				SelectedAction = action
			};
		}
	}

	public override void Handle(ScriptingEnvironment env)
	{
		env.BranchOptionPanel.Options.Clear();
		env.BranchOptionPanel.Options.AddRange(InternalOptions);

		env.BranchOptionPanel.Show();
	}
}
