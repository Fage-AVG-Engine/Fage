namespace Fage.Runtime.Scenes.Main.Characters;

public struct CharactersLayout
{
	/// <summary>
	/// 角色形象间的水平间隙。
	/// </summary>
	/// <remarks>
	/// 当多个角色形象同时出现在屏幕上时，会在它们之间插入间隙。这个变量的值控制间隙的大小。间隙大小以像素为单位。
	/// </remarks>
	public int HorizontalGap = 10;

	/// <summary>
	/// 角色形象的基线
	/// </summary>
	/// <remarks>
	/// 角色形象的基线到屏幕底端的距离，以像素为单位。基线的含义可参考字体相关的术语。
	/// </remarks>
	public int Baseline;

	/// <summary>
	/// 角色形象顶端到基线的距离，具体可以看注释中的灵魂作画
	/// </summary>
	// -|----|-----顶端-----
	//  | 人 |           ^  
	//  |    |           |  
	//  |    |           |  
	//  | 物 |           |  
	//  |    |    AscentPercent，比如75%
	//  |    |           |  
	//  | 形 |           |  
	//  |    |           v  
	// -|----|-----基线-----
	//  | 像 |              
	//  |____|              
	public float AscentPercent = 0.75f;

	public CharactersLayout()
	{

	}
}