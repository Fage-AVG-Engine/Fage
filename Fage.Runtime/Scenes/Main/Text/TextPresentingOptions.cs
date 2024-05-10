using Fage.Runtime.Bindable;
using FontStashSharp;
using System.Text;

namespace Fage.Runtime.Scenes.Main.Text;

public class TextPresentingOptions
{
	/// <summary>
	/// 剧情文本后附加的标记的颜色。
	/// </summary>
	public Color MarkerColor { get; set; } = new Color(75, 75, 75);

	/// <summary>
	/// 剧情文本后绘制的标记。
	/// </summary>
	public string EndMarkerText { get; set; } = "▶";

	/// <summary>
	/// 剧情文本的颜色。
	/// </summary>
	public Color TextColor { get; set; } = Color.Black;
	/// <summary>
	/// 剧情文本的字体大小。
	/// </summary>
	/// <remarks>
	/// 更改字体大小后，需要调用<see cref="UpdateFonts"/>应用更改。
	/// </remarks>
	public float FontSize { get; set; } = 16;
	/// <summary>
	/// 剧情文本使用的字体。
	/// </summary>
	/// <remarks>
	/// <remarks>
	/// <para>若要更换字体，请先新字体文件名的列表赋值到<see cref="FontNames"/>上，再调用<see cref="UpdateFonts"/>方法。</para>
	/// <para>请注意，剧情文本和角色姓名使用大小不同的同一组字体。更换了文本使用的字体会一并更换角色姓名使用的字体。</para>
	/// </remarks>
	/// </remarks>
	public SpriteFontBase Font { get; private set; } = null!;

	/// <summary>
	/// 角色姓名的文字颜色。
	/// </summary>
	public Color NameTextColor { get; set; } = Color.White;
	/// <summary>
	/// 角色姓名的字体大小。
	/// </summary>
	/// <remarks>
	/// 更改字体大小后，需要调用<see cref="UpdateFonts"/>应用更改。
	/// </remarks>
	public float NameFontSize { get; set; } = 20;
	/// <summary>
	/// 角色名称指示条使用的字体。
	/// </summary>
	/// <remarks>
	/// <para>若要更换字体，请先新字体文件名的列表赋值到<see cref="FontNames"/>上，再调用<see cref="UpdateFonts"/>方法。</para>
	/// <para>请注意，剧情文本和角色姓名使用大小不同的同一组字体。更换了角色姓名使用的字体会一并更换文本使用的字体。</para>
	/// </remarks>
	public SpriteFontBase NameFont { get; private set; } = null!;

	public FontSystem FontSystem { get; }

	/// <summary>
	/// 字体文件的搜索路径。
	/// </summary>
	public string FontSearchPath { get; set; }

	/// <summary>
	/// 控制文字渐入速度，这个属性优先级高于<see cref="TextSpeed"/>
	/// </summary>
	/// <remarks>
	/// <para>在一个段落中的文字还没有全部呈现到屏幕上的情况下，每经过该变量指定的间隔，就往不完整的文本后面附加一些字符。</para>
	/// <para></para>
	/// </remarks>
	public TimeSpan TextSpeedInterval { get; set; } = TimeSpan.FromMilliseconds(100);

	/// <summary>
	/// 以百分比的方式控制文字渐入速度。最大值为100%，最小值为0%。
	/// </summary>
	/// <remarks>
	/// 百分比经过一定的转换，换算到<see cref="TextSpeedInterval"/>上。
	/// </remarks>
	public BindableDouble TextSpeed { get; }

	/// <summary>
	/// 字体文件名。
	/// </summary>
	/// <remarks>
	/// 修改后需要手动调用<see cref="ApplyNewFont"/>更改字体
	/// </remarks>
	public IReadOnlyList<string> FontFileNames { get; set; } = ["LXGWNeoXiHei.ttf"];

	public string FontNames
	{
		get
		{
			if (FontFileNames.Count > 0)
			{
				StringBuilder sb = new(FontFileNames.Aggregate(0, (expectedCharsCount, name) => expectedCharsCount + name.Length + 4) + 4);
				sb.Append("[ \"").Append(FontFileNames[0]).Append('\"');

				for (int i = 1; i < FontFileNames.Count; i++)
				{
					sb.Append(", \"").Append(FontFileNames[i]).Append('\"');
				}

				sb.Append(" ]");
				return sb.ToString();
			}
			else
			{
				return "[]";
			}
		}
	}

	public TextPresentingOptions(Game game)
	{
		FontSearchPath = game.Content.RootDirectory;
		FontSystem = new();
		TextSpeed = new(0.5)
		{
			MaxValue = 1.0,
			MinValue = 0.0
		};
		TextSpeed.ValueChanged += _ => ApplyTextSpeed();
	}

	public void UnloadResources()
	{
		FontSystem.Dispose();
		TextSpeed.Dispose();
	}

	public void UpdateFonts()
	{
		FontSystem.Reset();
		foreach (var fontFileName in FontFileNames)
		{
			var fontUsing = File.OpenRead(Path.Combine(FontSearchPath, fontFileName));
			FontSystem.AddFont(fontUsing);
		}
		Font = FontSystem.GetFont(FontSize);
		NameFont = FontSystem.GetFont(NameFontSize);
	}

	/// <summary>
	/// 将<see cref="TextSpeed"/>指定的文本渐入速度应用到<see cref="TextSpeedInterval"/>。
	/// </summary>
	/// <remarks>
	/// 在修改<see cref="TextSpeed"/>的值时自动调用。在启动时手动调用一次即可。
	/// </remarks>
	public void ApplyTextSpeed()
	{
		// TODO 想一个河里的算法和数值，先用着反比例
		// 当前算法：
		// 实际间隔 = 四舍五入(最快间隔 + 常数比例 * 倒数(最慢速度 + 速度范围长度 * 速度百分比))
		const long FastestTextSpeedTicks = 75_0000;

		const double SpeedStep = 100_0000;
		const double SpeedDividerRangeMax = 12;
		const double SpeedDividerMin = 0.08;

		double speedTicksMultiplier = 1 / (SpeedDividerMin + SpeedDividerRangeMax * TextSpeed.Value);
		long ticks = (long)Math.Round(FastestTextSpeedTicks + SpeedStep * speedTicksMultiplier);

		TextSpeedInterval = TimeSpan.FromTicks(ticks);
	}
}
