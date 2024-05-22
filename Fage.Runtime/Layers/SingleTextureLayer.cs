using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Layering;

public class SingleTextureLayer(string name) : ILayer
{
	public string Name { get; } = name;

	public ILayer? Parent { get; set; }

	public Texture2D? Texture { get; set; }

	public Rectangle DestinationBounds;
	public Rectangle TextureSourceBounds;
	public Color TintColor { get; set; } = Color.White;

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		if (Texture != null)
			spriteBatch.Draw(Texture, DestinationBounds, TextureSourceBounds, TintColor);
	}

	public void Update(GameTime gameTime)
	{
		
	}
}
