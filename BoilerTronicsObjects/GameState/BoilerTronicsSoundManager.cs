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
	private Dictionary<SoundType, AudioStreamPlayer> activePlayers = new();

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
		
		//if already playing, start over
		if (activePlayers.ContainsKey(type)) {
			AudioStreamPlayer existingPlayer = activePlayers[type];
			existingPlayer.Stop();
			existingPlayer.QueueFree();
			activePlayers.Remove(type);
		}
		
		//create soundplayer
		var newPlayer = new AudioStreamPlayer();
		newPlayer.Stream = sounds[type];
		newPlayer.Name = $"{type}_Player";
		AddChild(newPlayer);
		activePlayers[type] = newPlayer;

		newPlayer.Play();
		
		newPlayer.Finished += () =>
		{
			if (IsInstanceValid(newPlayer)) {
				newPlayer.QueueFree();
			}
			activePlayers.Remove(type);
		};
	}
	
	public void StopAllSound() {
		foreach (var child in GetChildren()) {
			if (child is AudioStreamPlayer soundPlayer) {
				soundPlayer.Stop();
				soundPlayer.QueueFree();
			}
		}
		activePlayers.Clear();
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
