using System;
using Godot;

namespace LearningGodot;

public partial class RucksackReorganization : Sprite2D
{
	private const string InputPath = "res://Input/day_2_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int totalRepeats = 0;
		foreach (string line in InputReader.ReadInput(3))
		{
			int rucksackSize = line.Length;
			int compartmentSize = rucksackSize / 2;

			ReadOnlySpan<char> leftCompartment = line.AsSpan()[..compartmentSize];
			ReadOnlySpan<char> rightCompartment = line.AsSpan()[compartmentSize..];

			int lowerOffset = 96;
			int upperOffset = 38;

			char repeatChar = (char)0;
			bool found = false;
			for (int i = 0; i < compartmentSize; i++)
			{
				char comparison = leftCompartment[i];
				for (int j = 0; j < compartmentSize; j++)
				{
					if (comparison == rightCompartment[j])
					{
						repeatChar = comparison;
						found = true;
						break;
					}
				}

				if (found)
				{
					break;
				}
			}

			totalRepeats += char.IsLower(repeatChar) ? repeatChar - lowerOffset : repeatChar - upperOffset;

			// A = 65
			// a = 97
		}
		
		GD.Print(totalRepeats);
	}

	private class Rucksack
	{
		// Has two compartments
		// All items into one of two
		// One item wrong
		// Type is letter
		// A and a not same
		// Each half of string is compartment
		
		// Priority:
		// Lower => 1 - 26
		// Upper => 27 - 52
	}
}