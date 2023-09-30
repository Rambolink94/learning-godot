using System;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class RucksackReorganization : Node2D
{
	private const string InputPath = "res://Input/day_2_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int lowerOffset = 96;
		int upperOffset = 38;
		
		int totalRepeats = 0;
		int groupSize = 3;

		int groupCount = 0;
		while (true)
		{
			var group = InputReader.ReadInput(3).Skip(groupCount++ * groupSize).Take(groupSize).ToArray();
			if (group.Length == 0)
			{
				// Reached end of file
				break;
			}
			
			var badgeChar = GetBadgeChar(group);

			int offset = char.IsLower(badgeChar) ? lowerOffset : upperOffset;
			totalRepeats += badgeChar - offset;
		}
		
		GD.Print(totalRepeats);
	}

	private char GetBadgeChar(string[] group)
	{
		for (int i = 0; i < group[0].Length; i++)
		{
			for (int j = 0; j < group[1].Length; j++)
			{
				for (int k = 0; k < group[2].Length; k++)
				{
					if (group[0][i] == group[1][j] && group[1][j] == group[2][k])
					{
						return group[0][i];
					}
				}
			}
		}

		// Found nothing.
		return (char)0;
	}
}