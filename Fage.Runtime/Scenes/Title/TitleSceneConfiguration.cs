namespace Fage.Runtime.Scenes.Title;

public class TitleSceneConfiguration
{
	public NewGameConfiguration NewGame { get; set; } = null!;
	public string TitleBackgroundTextureName { get; set; } = null!;
}

public class NewGameConfiguration
{
	public string InitialScript { get; set; } = null!;
}

