using Godot;

public class BoilerTronicsSaveState
{
	// This should define what the save state of the current level is
	// We should serialize this and de-serialize it to save that level
	int save_slot; // -1 = base level; 0-2 = the respecive save slot for the user; any other value should result in an error (TODO: implement such errors)
	int level_id; // id for which level this save is referring to
}
