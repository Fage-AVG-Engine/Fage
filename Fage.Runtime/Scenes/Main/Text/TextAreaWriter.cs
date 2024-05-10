namespace Fage.Runtime.Scenes.Main.Text;

public class TextAreaWriter(ParagraphTypewriterEffect typewriterEffect)
{
	public ParagraphTypewriterEffect TypewriterEffect { get; } = typewriterEffect;

	/// <summary>
	/// 标记当前段落已完成，不会继续添加新的文本
	/// </summary>
	public void CompleteParagraph() => TypewriterEffect.CompleteParagraph();

	/// <summary>
	/// 清除之前的文本，并开始新的段落
	/// </summary>
	public void StartNewParagraph() => TypewriterEffect.StartNewParagraph();

	/// <summary>
	/// 将下一行添加到文本缓冲区
	/// </summary>
	/// <param name="lineContent">下一行的文本</param>
	public void PushNewLine(string lineContent) => TypewriterEffect.PushNewLine(lineContent);
}
