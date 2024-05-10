
namespace Fage.Runtime.Integration;

interface IAchievement
{
	void Achieved(string achievementName);

	void SetProgress(string achievementName, int progress);

	void GetProgressInteger(string achievementName);

	void SetProgress(string achievementName, float progress);

	void GetProgressFloat(string achievementName);
}