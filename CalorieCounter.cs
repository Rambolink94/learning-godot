using Godot;

namespace LearningGodot;

public partial class CalorieCounter : PuzzleNode
{
	private const string InputPath = "res://Input/day_1_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var topThreeElves = new int[3];
		int currentCalories = 0;
		
		foreach (string line in InputReader.ReadInput(1))
		{
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

		int totalElfCalories = 0;
		for (int i = 0; i < topThreeElves.Length; i++)
		{
			Print(topThreeElves[i].ToString());
			GD.Print(topThreeElves[i]);
			totalElfCalories += topThreeElves[i];
		}
		
		Print(totalElfCalories.ToString());
		GD.Print(totalElfCalories);
	}
}
