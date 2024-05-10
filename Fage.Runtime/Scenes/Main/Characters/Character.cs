using Fage.Runtime.Utility;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Scenes.Main.Characters;

public class Character(string identity)
{
	public const string DefaultTextureName = "Default";
	public const string DefaultMotionName = "Default";

	public string Identity { get; } = identity;
	public string DisplayName { get; set; } = identity;
	public virtual Point PreferredSize => new(DefaultTexture.Width, DefaultTexture.Height);
	public Dictionary<string, Texture2D> Textures { get; } = [];

	public Texture2D DefaultTexture => Textures[DefaultTextureName];

	public string Motion { get; set; } = DefaultMotionName;

	public virtual void UnloadResources(ContentManager parentContentManager)
	{
		parentContentManager.UnloadAssets(Textures.Values.Select(t => t.Name).ToArray());
		Textures.Clear();
	}

	public virtual void Update(GameTime gameTime)
	{

	}

	public virtual void Draw(GameTime gameTime, Rectangle drawingBounds, SpriteBatch spriteBatch)
	{
		if (!Textures.TryGetValue(Motion, out var texture))
			texture = DefaultTexture;

		var destinationRect = AdaptBounds.FitDestinationByCenter(texture.Bounds, drawingBounds);
		spriteBatch.Draw(texture, destinationRect, Color.White);
	}
}