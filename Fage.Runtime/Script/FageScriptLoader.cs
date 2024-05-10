using Fage.Runtime.Scenes.Main;
using Fage.Script.Serialization;
using Fage.Script.Serialization.DebugSerializer;
using Remotion.Linq.Clauses.ResultOperators;

namespace Fage.Runtime.Script;

public class FageScriptLoader(FageTemplateGame game, MainScene scene)
{
	private readonly MainScene _scene = scene;
	private readonly FageCommonResources _resources = game.CommonResources;
	private readonly string _scriptRoot = Path.Combine(game.Content.RootDirectory, "Scripts");

	public async ValueTask<FageScript> LoadAsync(string scriptName, CancellationToken ct)
	{
		string scriptStemName = Path.Combine(_scriptRoot, scriptName);

		// 先猜 json
		string guessingScriptPath = scriptStemName + ".fage.json";

		FageScriptInstantization instantization;

		if (File.Exists(guessingScriptPath))
		{
			using var scriptFile = File.OpenRead(guessingScriptPath);
			instantization = await DebugSerializer.DeserializeAsync(scriptFile, ct);
		}
		else
		{
			throw new FileNotFoundException($"找不到潜在的脚本文件，提供的文件名为{scriptName}");
		}

		var result = new FageScript(instantization, guessingScriptPath, scriptName);
		result.CheckVersionSupported();

		return result;
	}

	public Task LoadResourceAsync(FageScript script, CancellationToken ct = default)
	{
		return Task.Run(() =>
		{
			foreach (var instruction in script.InstructionSequence)
			{
				if (instruction.WillObtainResource)
				{
					if (ct.IsCancellationRequested)
					{
						int index = 0;
						var seq = script.InstructionSequence;
						while (!ReferenceEquals(instruction, seq[index]))
						{
							if (instruction.ResourceLoaded)
							{
								instruction.InternalUnloadResource(_scene.Content);
							}
						}
						ct.ThrowIfCancellationRequested();
					}

					instruction.InternalLoadResource(_scene.Content);
				}
			}
		});
	}

	public Task UnloadResourceAsync(FageScript script)
	{
		return Task.Run(() =>
		{
			foreach (var instruction in script.InstructionSequence)
			{
				if (instruction.WillObtainResource)
				{
					instruction.InternalUnloadResource(_scene.Content);
				}
			}
		});
	}
}
