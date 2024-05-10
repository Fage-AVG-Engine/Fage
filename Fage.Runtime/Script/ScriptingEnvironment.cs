using Fage.Runtime.Audio;
using Fage.Runtime.Scenes.Main;
using Fage.Runtime.Scenes.Main.Branch;
using Fage.Runtime.Scenes.Main.Characters;
using Fage.Runtime.Scenes.Main.Text;

namespace Fage.Runtime.Script;

public class ScriptingEnvironment
{
	public ScriptingEnvironment(
		FageScript script,
		AudioManager audio,
		MainScene scene,
		FageTemplateGame game,
		LuaEnvironment luaEnv)
	{
		Script = script;
		Audio = audio;
		Game = game;
		TextArea = scene.TextArea;
		TextWriter = TextArea.Writer;
		Characters = scene.Characters;
		Scene = scene;
		Lua = luaEnv;
	}

	public FageScript Script { get; }

	public AudioManager Audio { get; }

	public MainScene Scene { get; }

	public CurrentTextAreaComponent TextArea { get; }

	public TextAreaWriter TextWriter { get; }

	public CharactersManagerBase<Character> Characters { get; }

	public BranchOptionPanel BranchOptionPanel => Scene.BranchOptionPanel;

	public LuaEnvironment Lua { get; }

	public FageTemplateGame Game { get; }

	public void JumpToAnchor(string anchor, string script)
	{
		if (script == Script.ScriptName)
		{
			JumpToAnchorThisScript(Script.Anchors[anchor]);
		}
		else
		{
			JumpToAnchorAnotherScript(anchor, script);
		}
	}

	internal void JumpToAnchorThisScript(int nextThisScriptIndex)
	{
		Script.CurrentPosition = nextThisScriptIndex;
	}

	private void JumpToAnchorAnotherScript(string anchor, string script)
	{
		// 好孩子不要学
		Game.SwitchToMainScene(script, anchor);
	}
}
