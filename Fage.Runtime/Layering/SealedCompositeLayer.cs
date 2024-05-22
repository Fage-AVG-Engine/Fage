using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Layering;

public sealed class SealedCompositeLayer(string name) : CompositeLayer(name)
{
	public sealed override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		base.Draw(gameTime, spriteBatch);
	}

	public sealed override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}

	public sealed override bool HandleInput(ILayer sender, LayeredMouseEventArgs e)
	{
		return base.HandleInput(sender, e);
	}

	public sealed override bool HandleInput(ILayer sender, LayeredKeyboardEventArgs e)
	{
		return base.HandleInput(sender, e);
	}
	
	public sealed override bool HandleInput(ILayer sender, LayeredTouchEventArgs e)
	{
		return base.HandleInput(sender, e);
	}
	
	public sealed override bool HandleInput(ILayer sender, LayeredGamepadEventArgs e)
	{
		return base.HandleInput(sender, e);
	}
}
