using Godot;
using System;
using System.Linq;

public partial class Histogram : Control
{
	[Export] public float[] Scores = {};

	[Export] public int BucketSize = 10;

	public float UserScore = 0f;
	private bool _hasUserScore = false;

	[Export] public Color BucketColor = new Color("4a90e2");
	[Export] public Color UserBucketColor = new Color("f5a623");

	[Export] public float BarSpacing = 4f;
	[Export] public float LeftPadding = 40f;
	[Export] public float RightPadding = 8f;
	[Export] public float BottomPadding = 40f;

	public override void _Ready()
	{
		LoadScore();
		QueueRedraw();
	}

	private int GetBucketIndex(float score)
	{
		if (score <= 0)
			return 0;

		return (int)((score - 1f) / BucketSize);
	}

	public void changeUserScore(float score)
	{
		var list = Scores.ToList();
		if (_hasUserScore)
			list.Remove(UserScore);
		list.Add(score);
		Scores = list.ToArray();
		UserScore = score;
		_hasUserScore = true;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (Scores == null || Scores.Length == 0)
			return;

		float maxScore = Scores.Max();
		if (maxScore < 0)
			return;

		int numBuckets = (maxScore == 0) ? 1 : GetBucketIndex(maxScore) + 1;

		int[] bucketCounts = new int[numBuckets];
		foreach (float s in Scores)
		{
			if (s < 0)
				continue;

			int idx = GetBucketIndex(s);
			if (idx >= 0 && idx < numBuckets)
				bucketCounts[idx]++;
		}

		int maxCount = bucketCounts.Max();
		if (maxCount == 0)
			return;

		float chartWidth = Size.X - LeftPadding - RightPadding;
		float chartHeight = Size.Y - BottomPadding;
		if (chartWidth <= 0 || chartHeight <= 0)
			return;

		float barWidth = (chartWidth - (numBuckets - 1) * BarSpacing) / numBuckets;
		if (barWidth < 1f)
			barWidth = 1f;

		float baseY = Size.Y - BottomPadding;
		int userBucketIndex = _hasUserScore ? GetBucketIndex(UserScore) : -1;

		var font = GetThemeDefaultFont();
		int fontSize = 14;

		if (_hasUserScore)
			{
				string title = $"Total Score: {UserScore}";
				var titleSize = font.GetStringSize(title, HorizontalAlignment.Center, -1, fontSize);

				float topOfChart = baseY - chartHeight;

				Vector2 titlePos = new Vector2(
					Size.X / 2f,               // centered horizontally
					topOfChart - 20            // 20 pixels ABOVE the chart
				);

				DrawString(
					font,
					titlePos - titleSize / 2f, // center the text
					title,
					HorizontalAlignment.Center,
					-1,
					fontSize,
					Colors.Black
				);
			}



		float yAxisX = LeftPadding;

		DrawLine(
			new Vector2(yAxisX, baseY - chartHeight),
			new Vector2(yAxisX, baseY),
			Colors.Black,
			2f
		);

		int numTicks = Mathf.Min(5, maxCount);
		if (numTicks < 1) numTicks = 1;

		int step = Mathf.CeilToInt(maxCount / (float)numTicks);
		if (step < 1) step = 1;

		for (int i = 0; i <= numTicks; i++)
		{
			int value = i * step;
			if (value > maxCount)
				value = maxCount;

			float t = value / (float)maxCount;
			float ty = baseY - chartHeight * t;

			DrawLine(
				new Vector2(yAxisX - 4, ty),
				new Vector2(yAxisX, ty),
				Colors.Black,
				1.5f
			);

			string yLabel = value.ToString();
			var ySize = font.GetStringSize(yLabel, HorizontalAlignment.Left, -1, fontSize);

			Vector2 yPos = new Vector2(
				yAxisX - 8 - ySize.X,
				ty + ySize.Y * 0.5f
			);

			DrawString(
				font,
				yPos,
				yLabel,
				HorizontalAlignment.Left,
				-1,
				fontSize,
				Colors.Black
			);
		}

		for (int i = 0; i < numBuckets; i++)
		{
			int count = bucketCounts[i];
			float barHeight = chartHeight * count / (float)maxCount;

			float x = LeftPadding + i * (barWidth + BarSpacing);
			float y = baseY - barHeight;

			Color c = (i == userBucketIndex) ? UserBucketColor : BucketColor;

			DrawRect(
				new Rect2(new Vector2(x, y), new Vector2(barWidth, barHeight)),
				c
			);

			float borderThickness = 2f;

			DrawLine(
				new Vector2(x, y),
				new Vector2(x + barWidth, y),
				Colors.Black,
				borderThickness
			);

			DrawLine(
				new Vector2(x, y + barHeight),
				new Vector2(x + barWidth, y + barHeight),
				Colors.Black,
				borderThickness
			);

			DrawLine(
				new Vector2(x, y),
				new Vector2(x, y + barHeight),
				Colors.Black,
				borderThickness
			);

			DrawLine(
				new Vector2(x + barWidth, y),
				new Vector2(x + barWidth, y + barHeight),
				Colors.Black,
				borderThickness
			);
		}

		DrawLine(
			new Vector2(LeftPadding, baseY),
			new Vector2(Size.X - RightPadding, baseY),
			Colors.Black,
			2f
		);

		for (int i = 0; i < numBuckets; i++)
		{
			int start = i * BucketSize + 1;
			int end = (i + 1) * BucketSize;

			if (i == 0)
				start = 0;

			string label = $"{start}-{end}";

			float x = LeftPadding + i * (barWidth + BarSpacing);
			float textX = x + barWidth / 2f;

			Vector2 position = new Vector2(textX, baseY + 28);

			var textSize = font.GetStringSize(label, HorizontalAlignment.Center, -1, fontSize);
			DrawString(
				font,
				position - textSize / 2f,
				label,
				HorizontalAlignment.Center,
				-1,
				fontSize,
				Colors.Black
			);
		}
	}

	public void SetScores(float[] scores, float userScore, bool hasUserScore = true)
	{
		Scores = scores;
		UserScore = userScore;
		_hasUserScore = hasUserScore;
		QueueRedraw();
	}
	public void LoadScore()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		string SavePath = "res://Resources/Levels/leaderboard" + manager.GetLevelID() + ".leaderboard";
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.Print("Leaderboard: Loading: File does not exist!");
			return;
		} // not valid save location
		
		// open up save data
		using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		
		// much copied from Godot's documentation
		while (saveFile.GetPosition() < saveFile.GetLength()) {
			var jsonString = saveFile.GetLine();
			
			// Creates the helper class to interact with JSON.
			var json = new Json();
			var parseResult = json.Parse(jsonString);
			if (parseResult != Error.Ok)
			{
				GD.Print("Leaderboard: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				continue;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			// GD.Print("Leaderboard: Loaded dictionary:", nodeData);

			string name = (string) nodeData["username"];
			float score = (float) nodeData["score"];
			
			// GD.Print("Leaderboard: username: ", name, ", score: ", score);
			// GD.Print("SaveState: metadata: level name: ", levelName);
			Scores = Scores.Append(score).ToArray();
			continue;
		}

		SavePath = "user://Leaderboard/leaderboard" + manager.GetLevelID() + ".leaderboard";
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.Print("Leaderboard: LoadScore failed to find file: ", SavePath);
			return;
		} // not valid save location
		
		// open up save data
		using var savedFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		
		// much copied from Godot's documentation
		// while (saveFile.GetPosition() < saveFile.GetLength()) {
			var jsondString = savedFile.GetLine();

			// Creates the helper class to interact with JSON.
			var jsond = new Json();
			var parsedResult = jsond.Parse(jsondString);
			if (parsedResult != Error.Ok)
			{
				GD.Print("Leaderboard: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				// continue;
				return;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodedData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)jsond.Data);
			
			// GD.Print("Leaderboard: Loaded dictionary:", nodeData);

			string named = (string) nodedData["username"];
			float scored = (float) nodedData["score"];
			
			// GD.Print("Leaderboard: username: ", name, ", score: ", score);
			// GD.Print("SaveState: metadata: level name: ", levelName);
			changeUserScore(scored);
			
	}
}
