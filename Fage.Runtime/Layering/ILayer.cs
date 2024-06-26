using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Layering;

public interface ILayer
{
	string Name { get; }

	ILayer? Parent { get; set; }

	void Update(GameTime gameTime);

	void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}