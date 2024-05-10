using Fage.Runtime.Collections;
using FontStashSharp;
using System.Buffers;

namespace Fage.Runtime.Scenes.Main.Text;

public class ParagraphTypewriterEffect
{
	private static readonly SearchValues<char> LineDelimiters = SearchValues.Create(new char[] { '\r', '\n' });

	private int _lastPosition = 0;
	private int _lineStart = 0;

	private ValueList<char> _paragraphTextStorage = new(128);
	private bool _allCurrentTextPresented = true;

	public DateTime LinePresentedTime { get; private set; }

	public ReadOnlyMemory<char> ParagraphText => _paragraphTextStorage.AsReadonlyMemory();
	private ReadOnlySpan<char> ParagraphTextSpan => _paragraphTextStorage.AsReadonlySpan();

	public int LineStart => _lineStart;
	public int LastPosition => _lastPosition;

	/// <summary>
	/// 当前正在播放的行的索引（从0开始）
	/// </summary>
	/// <remarks>
	/// <para>如果正在播放第一行，这个属性的值就是0。依此类推，播放第二行时，属性值为1。</para>
	/// </remarks>
	public int LineIndexProceeding { get; private set; } = 0;

	/// <summary>
	/// 指示一个段落中的文本是否已全部添加到文本缓冲区
	/// </summary>
	public bool AllLinesAreFed { get; private set; }

	/// <summary>
	/// 指示当前段落是否播放完毕
	/// </summary>
	public bool ParagraphCompleted => AllLinesAreFed && _allCurrentTextPresented;
	/// <summary>
	/// 指示当前行是否播放完毕
	/// </summary>
	public bool CurrentLineCompleted { get; private set; } = true;

	/// <summary>
	/// 更新文本进度
	/// </summary>
	/// <returns>文本位置变动时，返回<see langword="true"/>，没有变化时，返回<see langword="false"/>。</returns>
	public bool TryUpdateTextProgress(TimeSpan textSpeedInterval, SpriteFontBase font)
	{
		ReadOnlySpan<char> paragraphText = ParagraphTextSpan;

		if (ParagraphText.IsEmpty || LastPosition == paragraphText.Length)
		{
			_allCurrentTextPresented = true;
			CurrentLineCompleted = true;
			return false;
		}

		if (IsLineBreakReached(paragraphText, LastPosition) && !_allCurrentTextPresented)
		{
			CurrentLineCompleted = true;
			return false;
		}

		if (textSpeedInterval.Ticks != 0)
		{
			var now = DateTime.UtcNow;
			var elapsedTime = now - LinePresentedTime;

			var desiredHalfWidthCharCount = elapsedTime / textSpeedInterval;
			if (desiredHalfWidthCharCount < 1)
				return false; // 无需推进

			double desiredPresentLength = desiredHalfWidthCharCount * font.FontSize;

			int charsCountToAdd = -1;
			Vector2 measureResult;
			do
			{
				int paragraphPosition = LastPosition + ++charsCountToAdd;

				if (paragraphPosition == ParagraphText.Length || IsLineBreakReached(paragraphText, paragraphPosition))
				{
					break;
				}

				measureResult = font.MeasureString(ParagraphText[LineStart..paragraphPosition].ToString());
			} while (measureResult.X < desiredPresentLength);

			if (charsCountToAdd == 0)
				return false;

			_lastPosition = LastPosition + charsCountToAdd;
			return true;
		}
		else
		{
			// 通过特殊方式关闭了打字机效果
			// 直接呈现一整行
			SkipCurrentLine();
			return true;
		}

		static bool IsLineBreakReached(ReadOnlySpan<char> paragraphText, int paragraphPosition)
		{
			return paragraphText[paragraphPosition] == '\r'
				|| paragraphText[paragraphPosition] == '\n';
		}
	}

	/// <summary>
	/// 略过正在进行的打字机效果，呈现整行文本。
	/// </summary>
	public void SkipCurrentLine()
	{
		ReadOnlySpan<char> remaining = ParagraphText.Span[_lineStart..];

		int lineDelimiterPosition = remaining.IndexOfAny(LineDelimiters);

		CurrentLineCompleted = true;
		if (lineDelimiterPosition != -1)
		{
			_lastPosition = lineDelimiterPosition;
		}
		else
		{
			_allCurrentTextPresented = true;
			_lastPosition = _paragraphTextStorage.Count;
		}
	}

	// TODO 测试需要，可访问性设为 internal
	internal void FitLineBreak(ReadOnlySpan<char> paragraph, ref int position, ref int newLineStart)
	{
		var length = paragraph.Length;
		if (position < length && paragraph[position] == '\r') // CR					
			newLineStart = ++position;

		if (position < length && paragraph[position] == '\n') // CR LF或LF
		{
			newLineStart = ++position;
		}
	}

	internal void PushText(ReadOnlySpan<char> text)
	{
		_paragraphTextStorage.AddRange(text);
	}

	/// <summary>
	/// 将下一行添加到文本缓冲区
	/// </summary>
	/// <param name="lineContent">下一行的文本</param>
	internal void PushNewLine(ReadOnlySpan<char> lineContent)
	{
		if (ParagraphCompleted)
			throw new InvalidOperationException($"段落已结束，不能继续添加文本。" +
				$"在继续添加文本之前，请先调用{nameof(StartNewParagraph)}方法。");

		PushText(lineContent);
		PushText("\n");
	}

	/// <summary>
	/// 清除已添加的段落，并开始新一段文本
	/// </summary>
	/// <param name="initialParagraphText">段落开头的文本</param>
	/// <devdoc>
	/// <remarks>
	/// 为了避免过多API集中在一个类上分散外部开发者注意力，
	/// 本方法可访问性设为<see langword="internal" />并对外隐藏。
	/// 操作段落内容的API由<see cref="TextAreaWriter"/>公开。
	/// </remarks>
	/// </devdoc>
	internal void StartNewParagraph(string initialParagraphText)
	{
		_paragraphTextStorage.AssignFromRange(initialParagraphText.AsSpan());
		StartNewParagraph();
	}

	/// <summary>
	/// 清除之前的文本，并开始新的段落
	/// </summary>
	/// <devdoc>
	/// <remarks>
	/// 为了避免过多API集中在一个类上分散外部开发者注意力，
	/// 本方法可访问性设为<see langword="internal" />并对外隐藏。
	/// 操作段落内容的API由<see cref="TextAreaWriter"/>公开。
	/// </remarks>
	/// </devdoc>
	internal void StartNewParagraph()
	{
		_lineStart = _lastPosition = 0;
		_allCurrentTextPresented = false;
		CurrentLineCompleted = false;
		AllLinesAreFed = false;
		LineIndexProceeding = 0;
		_paragraphTextStorage.Clear();
		LinePresentedTime = DateTime.UtcNow;
	}

	/// <summary>
	/// 标记当前段落已完成，不会添加更多文本
	/// </summary>
	/// <devdoc>
	/// <remarks>
	/// 为了避免过多API集中在一个类上分散外部开发者注意力，
	/// 本方法可访问性设为<see langword="internal" />并对外隐藏。
	/// 操作段落内容的API由<see cref="TextAreaWriter"/>公开。
	/// </remarks>
	/// </devdoc>
	internal void CompleteParagraph()
	{
		AllLinesAreFed = true;
	}

	/// <summary>
	/// 开始呈现下一行
	/// </summary>
	/// <remarks>
	/// 应通过<see cref="CurrentTextAreaComponent"/>间接调用
	/// </remarks>
	internal void StartNextLine()
	{
		FitLineBreak(ParagraphTextSpan, ref _lastPosition, ref _lineStart);
		LineIndexProceeding++;
		CurrentLineCompleted = false;
		LinePresentedTime = DateTime.UtcNow;
	}
}
