using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UserData
{
	[JsonPropertyName("username")]
	public string Username { get; set; }

	[JsonPropertyName("uuid")]
	public string Uuid { get; set; }

	[JsonPropertyName("friends")]
	public List<string> Friends { get; set; } = new List<string>();

	[JsonPropertyName("achievementsUnlocked")]
	public List<string> AchievementsUnlocked { get; set; } = new List<string>();

	[JsonPropertyName("hoursPlayed")]
	public float HoursPlayed { get; set; } = 0f;

    [JsonPropertyName("linesRun")]
	public float LinesRun { get; set; } = 0f;

	[JsonPropertyName("easterEggsFound")]
	public List<string> EasterEggsFound { get; set; } = new List<string>();

	public static UserData CreateDefault(string uuid, string email)
	{
		return new UserData
		{
			Username = email.Split('@')[0],
			Uuid = uuid,
			Friends = new List<string>(),
			AchievementsUnlocked = new List<string>(),
			HoursPlayed = 0f,
			EasterEggsFound = new List<string>()
		};
	}
}