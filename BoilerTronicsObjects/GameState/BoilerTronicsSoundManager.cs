using Godot;
using System.Collections.Generic;

public enum SoundType {
	Error,
	Move,
	Grab,
	Drop,
	Rotate
}

public partial class BoilerTronicsSoundManager : Node
{
	public static BoilerTronicsSoundManager SoundManager { get; private set; }

	private Dictionary<SoundType, AudioStream> sounds = new();

	public override void _Ready()
	{
		//only one sound manager
		if (SoundManager != null)
		{
			return;
		}

		SoundManager = this;
		Name = "BoilerTronicsSoundManager";

		//load sounds
		sounds[SoundType.Error] = GD.Load<AudioStream>("res://Resources/Sounds/errorSound.wav");
		sounds[SoundType.Move] = GD.Load<AudioStream>("res://Resources/Sounds/moving.wav");
		sounds[SoundType.Grab] = GD.Load<AudioStream>("res://Resources/Sounds/grab.wav");
		sounds[SoundType.Drop] = GD.Load<AudioStream>("res://Resources/Sounds/grab.wav");
		sounds[SoundType.Rotate] = GD.Load<AudioStream>("res://Resources/Sounds/turning.wav");
	}

	//play the sound called by name
	public void PlaySound(SoundType type) {
		if (!sounds.ContainsKey(type)) {
			GD.Print("sound not found");
		}
		
		//create soundplayer
		var soundPlayer = new AudioStreamPlayer();
		soundPlayer.Stream = sounds[type];
		soundPlayer.Name = $"{type}_Player";
		AddChild(soundPlayer);
		
		soundPlayer.Finished += () =>
		{
			soundPlayer.QueueFree();
		};

		soundPlayer.Play();
	}
	
	public void StopAllSound() {
		foreach (var child in GetChildren()) {
			if (child is AudioStreamPlayer player)
				player.Stop();
		}
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
