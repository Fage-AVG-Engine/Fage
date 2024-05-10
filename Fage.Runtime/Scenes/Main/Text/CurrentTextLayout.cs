using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fage.Runtime.Scenes.Main.Text
{
	public struct CurrentTextLayout
	{
		public int MarginLeft;
		public int MarginRight;
		public int MarginBottom;

		public int TextPaddingLeft;
		public int TextPaddingRight;
		public int TextPaddingTop;

		/// <summary>
		/// 文本组件的高度
		/// </summary>
		/// <remarks>
		/// 当这个值设为0时，文本组件的高度将在下次调用<see cref="CurrentTextAreaComponent.OnDeviceReset"/>时，根据背景图片的高度自动计算一次。
		/// 计算完成后，新的高度值将被缓存在这个变量中。
		/// </remarks>
		public int Height;
	}
}