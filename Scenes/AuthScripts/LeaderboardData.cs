using System.Collections.Generic;

public class LeaderboardData
{
	public List<ScoreData> AboveUser { get; set; }
	public ScoreData UserScore { get; set; }
	public List<ScoreData> BelowUser { get; set; }
	public int UserRank { get; set; }
	public int TotalEntries { get; set; }
}