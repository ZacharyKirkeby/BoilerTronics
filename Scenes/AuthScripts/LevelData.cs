using System;
using System.Collections.Generic;

// This makes life cleaner for sending / storing levels, bc then i can universally translate it for firestore
public class LevelData
{
	public string LevelId { get; set; }
	public string CreatorId { get; set; }
	public string CreatorName { get; set; }
	public string LevelName { get; set; }
	public string Description { get; set; }
	public string LevelDataJson { get; set; }
	public string Difficulty { get; set; }
	public List<string> Tags { get; set; }
	
	public override string ToString() {
		
		string output = "{ ";
		output += "LevelId: " + this.LevelId + ", ";
		output += "LevelId: " + this.CreatorId + ", ";
		output += "LevelId: " + this.CreatorName + ", ";
		output += "LevelId: " + this.LevelName + ", ";
		output += "LevelId: " + this.Description + ", ";
		output += "LevelId: " + this.LevelDataJson + ", ";
		output += "LevelId: " + this.Difficulty + ", ";
		output += "LevelId: " + this.Tags.ToString();
		output += " }";
		
		return output;
	}
}
