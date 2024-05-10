using Neo.IronLua;

namespace Fage.Runtime.Script;

public class LuaEnvironment(Lua luaVM, LuaGlobal globalEnvironment)
{
	public Lua LuaVM { get; internal init; } = luaVM;

	public LuaGlobal GlobalEnvironment { get; internal init; } = globalEnvironment;

	public LuaCompileOptions CompileOptions { get; } = new()
	{
		ClrEnabled = true,
		DebugEngine = null,
	};

	private static string ChunkNameWithSourceInfo(string scriptPath, int lineNumber, (int, int) sourceSpan)
	{
		return $"{scriptPath}::Line = {lineNumber}, Span = {sourceSpan.Item1}-{sourceSpan.Item2}";
	}

	public LuaResult ExecuteCode(string code, string scriptPath, int? lineNumber, (int start, int end)? sourceSpan)
	{
		string chunkName;

		if (lineNumber is not null && sourceSpan is not null)
			chunkName = ChunkNameWithSourceInfo(scriptPath, lineNumber.Value, sourceSpan.Value);
		else
			chunkName = scriptPath;

		var chunk = LuaVM.CompileChunk(code, chunkName, CompileOptions);
		return chunk.Run(GlobalEnvironment);
	}

	protected virtual void ConfigureGlobalEnvironment(ScriptingEnvironment env)
	{
		GlobalEnvironment.DefineFunction("jump_to", env.JumpToAnchor);
		GlobalEnvironment.SetMemberValue("fage", env, true);

		GlobalEnvironment.SetMemberValue("audio", env.Audio);
		GlobalEnvironment.SetMemberValue("script", env.Script);
		GlobalEnvironment.SetMemberValue("scene", env.Scene);
		GlobalEnvironment.SetMemberValue("text_writer", env.TextWriter);
		GlobalEnvironment.SetMemberValue("characters_manager", env.Characters);
		GlobalEnvironment.SetMemberValue("branch", env.BranchOptionPanel);
		GlobalEnvironment.SetMemberValue("game_engine", env.Game);

		GlobalEnvironment.DefineFunction("block_script_execution", env.Scene.BlockScriptExecution);
		GlobalEnvironment.DefineFunction("unblock_script_execution", env.Scene.UnblockScriptExecution);

		// GlobalEnvironment.RegisterPackage() // 这东西干嘛的？
	}

	internal void ConfigureGlobalInternal(ScriptingEnvironment env) => ConfigureGlobalEnvironment(env);
}