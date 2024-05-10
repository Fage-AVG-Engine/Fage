using FontStashSharp;
using Microsoft.Xna.Framework.Content;

namespace Fage.Runtime.UI
{
	/// <summary>
	/// 创建按钮的模版
	/// </summary>
	/// <param name="releasedTextureName"></param>
	/// <param name="pressedTextureName"></param>
	/// <param name="hoverTextureName"></param>
	public class ImageButtonTemplate(
		string releasedTextureName,
		string pressedTextureName,
		string hoverTextureName
	) {
		public string ReleasedTextureName { get; set; } = releasedTextureName;
		public string PressedTextureName { get; } = pressedTextureName;
		public string HoverTextureName { get; } = hoverTextureName;

		public string DisabledTextureName { get; set; } = hoverTextureName;

		public SpriteFontBase? Font { get; set; }
		public Color ReleasedTextColor { get; set; } = Color.White;
		public Color PressedTextColor { get; set; } = Color.Black;
		public Color HoverTextColor { get; set; } = Color.White;
		public Color DisabledTextColor { get; set; } = Color.White;
		
		/// <summary>
		/// 通过已设置的样式，创建带有文本的UI按钮
		/// </summary>
		/// <param name="name">新按钮的名称</param>
		/// <param name="text">按钮中的文本</param>
		/// <param name="contentManager">用于加载资源的<see cref="ContentManager"/></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// <see cref="Font"/>为空
		/// </exception>
		public ImageBasedButton CreateWithText(string name, string text, ContentManager contentManager)
		{
			ArgumentNullException.ThrowIfNull(Font);

			return new ImageBasedButton(
				name, 
				HoverTextureName, 
				ReleasedTextureName,
				PressedTextureName,
				DisabledTextureName,
				contentManager)
			{
				TextFont = Font,
				ButtonText = text,
				ReleasedTextColor = ReleasedTextColor,
				PressedTextColor = PressedTextColor,
				HoverTextColor = HoverTextColor,
				DisabledTextColor = DisabledTextColor
			};
		}

		/// <summary>
		/// 通过已设置的样式，创建UI按钮
		/// </summary>
		/// <param name="name"></param>
		/// <param name="contentManager"></param>
		/// <returns></returns>
		public ImageBasedButton Create(string name, ContentManager contentManager)
		{
			return new ImageBasedButton(name, HoverTextureName, ReleasedTextureName, PressedTextureName, DisabledTextureName, contentManager);
		}
	}
}
