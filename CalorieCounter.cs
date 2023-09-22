using Godot;

namespace LearningGodot;

public partial class CalorieCounter : Node3D
{
	private const string InputPath = "res://Input/day_1_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var topThreeElves = new int[3];
		using (var input = FileAccess.Open(InputPath, FileAccess.ModeFlags.Read))
		{
			int currentCalories = 0;
			do
			{
				string line = input.GetLine();

				if (line != string.Empty)
				{
					// Accumulate current elf's carried calories
					currentCalories += int.Parse(line);
					continue;
				}

				// If an empty line was found, then we've accumulated all the current elf's calories.
				for (int i = 0; i < topThreeElves.Length; i++)
				{
					if (topThreeElves[i] < currentCalories)
					{
						// Swap values to increase current and pass off old value to lower elves.
						(topThreeElves[i], currentCalories) = (currentCalories, topThreeElves[i]);
					}
				}
					
				currentCalories = 0;
			}
			while (!input.EofReached());
		}

		int totalElfCalories = 0;
		for (int i = 0; i < topThreeElves.Length; i++)
		{
			GD.Print(topThreeElves[i]);
			totalElfCalories += topThreeElves[i];
		}
		
		GD.Print(totalElfCalories);
	}
}