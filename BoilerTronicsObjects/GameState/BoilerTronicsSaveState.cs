// The purpose of this object is to act as one of the user's saves for the level and act as the base for the level as well

using Godot;

public class gameSave
{
	int save_slot; // -1 = base level; 0-2 = the respecive save slot for the user; any other value should result in an error (TODO: implement such errors)
	int level_id; // id for which level this save is referring to
}
