using Fage.Runtime.Layering;
using Fage.Runtime.Utility;
using FontStashSharp;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Fage.Runtime.UI
{
	public class ImageBasedButton(string name,
		string hoverTextureName,
		string releasedTextureName,
		string pressedTextureName,
		string disabledTextureName,
		ContentManager contentManager
	) : ILayer, ILayeredMouseHandler
	{
		public readonly string HoverTextureName = hoverTextureName;
		public readonly string ReleasedTextureName = releasedTextureName;
		public readonly string PressedTextureName = pressedTextureName;
		public readonly string DisabledTextureName = disabledTextureName;

		private readonly ContentManager contentManager = contentManager;

		private Texture2D _hoverTexture = null!;
		private Texture2D _releasedTexture = null!;
		private Texture2D _pressedTexture = null!;
		private Texture2D _disabledTexture = null!;

		private UIButtonState _previousState = UIButtonState.Released;

		public UIButtonState State { get; private set; } = UIButtonState.Released;

		public string Name { get; } = name;
		public ILayer? Parent { get; set; }
		public Rectangle DestinationArea { get; set; }

		public event Action<ImageBasedButton>? Clicked;
		public bool IsDisabled { get; set; } = false;

		public string ButtonText { get; set; } = string.Empty;

		[NotNull]
		public SpriteFontBase? TextFont { get; set; }

		public Color ReleasedTextColor { get; set; } = Color.White;
		public Color PressedTextColor { get; set; } = Color.Black;
		public Color HoverTextColor { get; set; } = Color.Black;
		public Color DisabledTextColor { get; set; } = Color.White;

		public Point ButtonSize { get; private set; }

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Color textColor;
			Rectangle destinationArea = DestinationArea;
			Texture2D texture;
			if (!IsDisabled)
			{
				if (State.IsPressed())
				{
					texture = _pressedTexture;
					textColor = PressedTextColor;
				}
				else if (State.IsHovering())
				{
					texture = _hoverTexture;
					textColor = HoverTextColor;
				}
				else
				{
					texture = _releasedTexture;
					textColor = ReleasedTextColor;
				}
			}
			else
			{
				texture = _disabledTexture;
				textColor = DisabledTextColor;
			}

			Layout.RectangleAlignCenter(ref destinationArea, texture.Width, texture.Height);
			spriteBatch.Draw(texture, destinationArea, Color.White);

			if (!string.IsNullOrEmpty(ButtonText))
			{
				Debug.Assert(TextFont != null);
				Vector2 textMetrics = TextFont.MeasureString(ButtonText);
				Vector2 textPosition = new(
					Layout.AlignCenter(DestinationArea.X, DestinationArea.Width, textMetrics.X),
					Layout.AlignCenter(DestinationArea.Y, DestinationArea.Height, textMetrics.Y)
				);
				spriteBatch.DrawString(TextFont, ButtonText, textPosition, textColor);
			}
		}

		public bool HandleInput(ILayer sender, LayeredMouseEventArgs e)
		{
			_previousState = State;
			if (IsDisabled)
			{
				State = UIButtonState.Disabled;
				return false;
			}

			if (e.IsUserInGame && DestinationArea.Contains(e.State.Position))
			{
				if (e.State.LeftButton == ButtonState.Pressed)
				{
					State = UIButtonState.Pressed | UIButtonState.Hovering;
					return true;
				}
				else
				{
					State = UIButtonState.Hovering;
					return false;
				}
			}
			else
			{
				State = UIButtonState.Released;
				return false;
			}
		}

		public void Update(GameTime gameTime)
		{
			if (!IsDisabled && _previousState.IsPressed() && State == UIButtonState.Hovering)
			{
				Clicked?.Invoke(this);
			}
		}

		public void LoadResource()
		{
			_hoverTexture = contentManager.Load<Texture2D>(HoverTextureName);
			SuggestSize(_hoverTexture.Width, _hoverTexture.Height);
			
			_releasedTexture = contentManager.Load<Texture2D>(ReleasedTextureName);
			SuggestSize(_releasedTexture.Width, _releasedTexture.Height); 

			_pressedTexture = contentManager.Load<Texture2D>(PressedTextureName);
			SuggestSize(_pressedTexture.Width, _pressedTexture.Height);
			
			_disabledTexture = contentManager.Load<Texture2D>(DisabledTextureName);
			SuggestSize(_disabledTexture.Width, _disabledTexture.Height);

			void SuggestSize(int width, int height)
			{
				var oldSize = ButtonSize;

				if (oldSize.X < width)
					oldSize.X = width;

				if (oldSize.Y < height)
					oldSize.Y = height;

				ButtonSize = oldSize;
			}
		}

		public void UnloadResource()
		{
			contentManager.UnloadAssets(new[] { HoverTextureName, PressedTextureName, ReleasedTextureName });
		}
	}
}
