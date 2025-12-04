using Godot;
using System.Buffers;
using System.Collections.Generic;

public partial class StoryWindow : Window
{
	BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

	[Export] public int LevelId = 11;
	

	private Label _context;

	// ------------------------------------------
	// STORY DICTIONARY
	// ------------------------------------------
	private static readonly Dictionary<int, string> Stories = new()
	{
		// ---------- 1.X ----------
		{ 11, 
@"It seems like you’ve been struggling in your academic career at Purdue University within your computer science degree and failed CS 307! You couldn’t bear to let down professor Turkstra and so you’ve dropped out. You purchased a factory to operate and you realized you have no idea how to build and operate one, but must now learn based on the limited assembly you learned in CS250 due to the ancient nature of the factory components. Best of luck!

Seems like this factory is mostly set up already, looks like you get to learn the basics." },

		{ 12, "Seems like the factory needs its components programmed. Time to put your limited Purdue Computer Science skills to use!" },
		{ 13, "Looks like some twisting and turning is ingrained into this factory! Might need to pick up some experience on rotating." },
		{ 14, "Seems like this factory layout is a bit more complex than just navigating using rails and rotators. Seems like conveyors may be of great use!" },
		{ 15, "Seems like you’ve had a greater deal of fortune in your mastery of conveyors and rotators! Time to add on and use switches, they seem to work better in some cases." },
		{ 16, "Time to fix a factory outline! Use your newfound materials and skills to complete this factory based on the recently learned tools." },

		// ---------- 2.X ----------
		{ 21,
@"After your success with your last set of factories, you began to think highly of yourself (perhaps too highly). It seems like your temporary ego boost has led you to the purchase of another set of factories that seems to be slightly different from the last. You must now reprove your factory building and programming skills to make up for the lost opportunity from dropping out all those years ago as you must now use some new objects.

Put it all together! Take the previously learned skills to build a factory on your own using the tools and skills you previously learned!" },

		{ 22, "Time to add to the complexity of the code. Looks like you’ll need to perform a repetitive set of instructions… I wonder what that refers to???" },
		{ 23, "Similar to the previous level but this time you’ll need to perform a repetitive set of instructions for multiple sets!" },
		{ 24, "Multiple types of materials! Make sure that you correctly load the materials for output, otherwise your consumers won’t be happy with you." },
		{ 25, "Looks like you have a smaller factory to work with this time. Keep making sure that the right materials are going to the corresponding outputs!" },
		{ 26, "Looks like your output does not match the current input! Looks like you’ll have to change the material using something that can turn that raw material into iron bars." },

		// ---------- 3.X ----------
		{ 31,
@"It seems like your continued success with these factories has become quite popular. In fact, you were contacted by a project manager who is offering boatloads of money to repair his factories and get it back into production. Such an opportunity would allow you to show professor Turkstra that you didn’t need him to pass you and that you were better off dropping out anyways. Adjust your previous skillset as you must now use new machinery to create necessary outputs.

Seems like you need to turn some raw iron input into an iron bar." },

		{ 32, "Looks like you’re starting with iron bars this time around. Your output is still different though, it seems like a new machine must be used." },
		{ 33, "Seems like you’ll have to use multiple machines to get the desired output this time! This factory is not making things easy for you!" },
		{ 34, "Looks like you’re starting with iron plates this time around. Your output is still different though, it seems like a new machine must be used." },
		{ 35, "Seems like you’ll have to use multiple machines to get the desired output again! These factories are not making things easy for you!" },
		{ 36, "Time to put your new skills with these machines to use! Take use of the furnace and other machines to create the output needed starting from the raw material." },

		// ---------- 4.X ----------
		{ 41,
@"The last few factories didn’t get you the satisfaction you needed as you begin to realize that you would only be proud of yourself after becoming the ultimate factory designer. You begin to seek out factory after factory in the hopes of showing off to everyone who doubted you as each factory seems to be getting more complex and pushes you to be better and better as you must learn to maneuver around uniquely set factories.

Looks like there are some obstacles in the way throughout this run down factory, make sure you avoid them!" },

		{ 42, "Continue to avoid the obstacles on the ceiling and floor. Looks like you got yourself another run down factory." },
		{ 43, "Continue to avoid the obstacles on the ceiling and floor. Looks like you got yourself another run down factory! Seems like you don’t have the best luck right now." },
		{ 44, "Continue to avoid the obstacles on the ceiling and floor. Looks like all these factories are ancient with the amount of issues you are finding yourself in the middle of!" },
		{ 45, "Continue to avoid the obstacles on the ceiling and floor. After working with so many run down factories, it seems like you’ve gotten used to the number of obstacles!" },
		{ 46, "Continue to avoid the obstacles on the ceiling and floor. You must be tired of these run down factories, luckily this may just be the last one!" },

		// ---------- 5.X ----------
		{ 51,
@"Seems like you’re really getting the hang of things now as you are approaching the status of factory mogul by your peers. Despite the continuing success, you are still learning as you go when coming across new materials, tools, and upgrades as these factories introduce you to new challenges never seen before.

Looks like you’re moving past just using iron as a build material as you find that steel is now demanded." },

		{ 52, "This time around you’ll need to use a steel heater to ensure that you can transport the steel produced over longer distances." },
		{ 53, "This time around you’ll need to use a steel cooler to ensure that you can transport the steel produced over short distances. You’ll likely find it useful to use water pipes to assist in the process." },
		{ 54, "You are now being asked to make gears for which you’ll find lube pipes useful in the process." },
		{ 55, "Gears are needed once again, but must be transported over long distances. Make sure you use the component which allows for longer transportation (similar to three factories ago)." },
		{ 56, "You are truly achieving much more than you initially throughout you could as you find yourself beating every challenge that comes your way. Time to create a full steel build chain using all your previously learned skills." },
	};

	// ------------------------------------------
	// USE THE DICTIONARY
	// ------------------------------------------
	public override void _Ready()
	{
		_context = GetNode<Label>("MarginContainer/Context");
		LevelId = manager.GetLevelID();
		if (Stories.TryGetValue(LevelId, out var text))
			_context.Text = text;
		else
			_context.Text = "No story found for this level.";
	}
}
