using Fage.Script.Instruction;
using Fage.Script.Instruction.Instantization;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Script.Instructions;

public class SetBackgroundInstruction : ScriptInstruction
{
	private Texture2D? _texture;

	public string Name { get; }

	public SetBackgroundInstruction(SetBackground instantizationInfo)
		: base(ScriptInstructionOpcode.SetBackground)
	{
		Name = instantizationInfo.Name;
		WillObtainResource = true;
	}

	public override void Handle(ScriptingEnvironment env)
	{
		env.Scene.Background = _texture;
		env.Scene.ApplyNewBackground();
	}

	protected override void LoadResource(ContentManager contentManager)
	{
		if (!string.IsNullOrEmpty(Name))
			_texture = contentManager.Load<Texture2D>(Name);
	}

	protected override void UnloadResource(ContentManager contentManager)
	{
		if (!string.IsNullOrEmpty(Name))
			contentManager.UnloadAsset(Name);
	}
}
