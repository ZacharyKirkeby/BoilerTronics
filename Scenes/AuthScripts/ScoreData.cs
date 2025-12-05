using System;
using System.Collections.Generic;

// what can i say i sometimes like objects
public class ScoreData
{
	public string UserId { get; set; }
	public string Username { get; set; }
	public string LevelId { get; set; }
	public int Score { get; set; }
    public int Steps {get; set; }
	public Dictionary<string, object> Metadata { get; set; }
}