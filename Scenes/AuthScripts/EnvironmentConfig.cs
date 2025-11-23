using Godot;
using System;
using System.Collections.Generic;

// this helps me use el .env file
public partial class EnvironmentConfig : Node
{
	private static EnvironmentConfig _instance;
	public static EnvironmentConfig Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new EnvironmentConfig();
				_instance.LoadEnvironmentVariables();
			}
			return _instance;
		}
	}

	private Dictionary<string, string> _envVariables = new Dictionary<string, string>();

	public override void _EnterTree()
	{
		if (_instance == null)
		{
			_instance = this;
			LoadEnvironmentVariables();
		}
		else
		{
			QueueFree();
		}
	}

	private void LoadEnvironmentVariables()
	{
		string envPath = "res://.env";
		
		if (!FileAccess.FileExists(envPath))
		{
			GD.PrintErr(".env file not found at: " + envPath);
			return;
		}

		using var file = FileAccess.Open(envPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr("Failed to open .env file");
			return;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine().Trim();
			
			// Skip empty lines and comments
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
				continue;

			// Parse key=value pairs
			int separatorIndex = line.IndexOf('=');
			if (separatorIndex > 0)
			{
				string key = line.Substring(0, separatorIndex).Trim();
				string value = line.Substring(separatorIndex + 1).Trim();
				
				// Remove quotes if present
				if (value.StartsWith("\"") && value.EndsWith("\""))
					value = value.Substring(1, value.Length - 2);
				else if (value.StartsWith("'") && value.EndsWith("'"))
					value = value.Substring(1, value.Length - 2);
				
				_envVariables[key] = value;
			}
		}

		GD.Print($"Loaded {_envVariables.Count} environment variables");
	}

	public string GetValue(string key, string defaultValue = null)
	{
		if (_envVariables.TryGetValue(key, out string value))
		{
			return value;
		}
		return defaultValue;
	}

	public bool HasValue(string key)
	{
		return _envVariables.ContainsKey(key);
	}
}