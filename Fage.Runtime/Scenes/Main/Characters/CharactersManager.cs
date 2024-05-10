using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Specialized;

namespace Fage.Runtime.Scenes.Main.Characters;

public sealed class CharactersManager : CharactersManagerBase<Character>
{

	public CharactersManager(FageTemplateGame game) : base(game)
	{

	}

	protected override Character LoadCharacterResources(string characterIdentity)
	{
		Character res = new(characterIdentity);
		res.Textures.Add(Character.DefaultTextureName, Contents.Load<Texture2D>(Path.Combine(ResourcesSearchPath, characterIdentity, Character.DefaultTextureName)));

		return res;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		for (int i = 0; i < CharactersOnScreen.Count; i++)
		{
			var charRes = CharactersOnScreen[i];
			charRes.Draw(gameTime, CharacterRectangles[i], spriteBatch);
		}
	}
}