using Godot;
using System.Collections.Generic;

public partial class BoilerTronicsSoundManager : Node
{
	public static BoilerTronicsSoundManager SoundManager { get; private set; }

	private Dictionary<string, AudioStream> sounds = new();
	private AudioStreamPlayer soundPlayer;
	private string currentSound = "";
	private int currentPriority = 0;

	public override void _Ready()
	{
		//only one sound manager
		if (SoundManager != null)
		{
			return;
		}

		SoundManager = this;
		Name = "BoilerTronicsSoundManager";

		soundPlayer = new AudioStreamPlayer();
		soundPlayer.Name = "SoundPlayer";
		AddChild(soundPlayer);

		//set sounds
		sounds["error"] = GD.Load<AudioStream>("res://Resources/errorSound.wav");
		sounds["move"] = GD.Load<AudioStream>("res://Resources/moving.wav");
		sounds["grab"] = GD.Load<AudioStream>("res://Resources/grab.wav");
		sounds["drop"] = GD.Load<AudioStream>("res://Resources/grab.wav");
		sounds["rotate"] = GD.Load<AudioStream>("res://Resources/turning.wav");
	}

	//play the sound called by name
	public void PlaySound(string soundName, int priority) {
		if (sounds.ContainsKey(soundName)) {
			if(soundPlayer == null) {
				GD.Print("soundplayer null");
			}
			if (priority < currentPriority && soundPlayer.Playing) {
				return;
			}

			if (soundPlayer.Playing) {
				soundPlayer.Stop();
			}

			soundPlayer.Stream = sounds[soundName];
			soundPlayer.Play();

			currentSound = soundName;
			currentPriority = priority;
		}
	}
	
	public void StopSound() {
		if((soundPlayer != null) && (soundPlayer.Playing)) {
			soundPlayer.Stop();
		}
		currentSound = "";
		currentPriority = 0;
	}
	
	public float GetCurrentVolume() {
		int masterBus = AudioServer.GetBusIndex("Master");
		float db = AudioServer.GetBusVolumeDb(masterBus);
		return Mathf.DbToLinear(db);
	}

	public void SetCurrentVolume(float vol) {
		int masterBus = AudioServer.GetBusIndex("Master");
		float db = Mathf.LinearToDb(vol);
		AudioServer.SetBusVolumeDb(masterBus, db);
	}
}
