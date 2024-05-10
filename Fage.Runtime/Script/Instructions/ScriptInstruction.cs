using Fage.Script.Instruction;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Fage.Runtime.Script.Instructions;

[DebuggerDisplay("{Opcode}")]
public abstract class ScriptInstruction(ScriptInstructionOpcode opcode)
{
	public ScriptInstructionOpcode Opcode { get; } = opcode;

	public int InstructionIndex { get; internal set; }

	/// <summary>
	/// 指令是否可能自行持有资源。
	/// </summary>
	/// <remarks>
	/// 派生类中需要使用<see cref="LoadResource(ContentManager)"/>
	/// 等资源管理方法时，自行加载资源时，应该将这个属性设为<see langword="true"/>。
	/// 此属性的值为<see langword="false"/>时，外部管理器不会调用资源相关方法。
	/// </remarks>
	public bool WillObtainResource { get; protected init; } = false;

	public bool ResourceLoaded { get; private set; }

	internal bool ResourcesReadyToExecute => !WillObtainResource || ResourceLoaded;

	public bool BlocksExecution { get; protected init; } = false;


	public abstract void Handle(ScriptingEnvironment env);

	protected virtual void LoadResource(ContentManager contentManager) { }

	internal void InternalLoadResource(ContentManager contentManager)
	{
		LoadResource(contentManager);
		ResourceLoaded = true;
	}

	protected virtual void UnloadResource(ContentManager contentManager) { }

	internal void InternalUnloadResource(ContentManager contentManager)
	{
		UnloadResource(contentManager);
		ResourceLoaded = false;
	}
}
