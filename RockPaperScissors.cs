using System;
using Godot;
using Array = System.Array;

namespace LearningGodot;

public partial class RockPaperScissors : Node
{
	private const string InputPath = "res://Input/day_2_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int totalScore = 0;
		int opponentTotalScore = 0;
		
		using (var input = FileAccess.Open(InputPath, FileAccess.ModeFlags.Read))
		{
			do
			{
				string line = input.GetLine();

				string[] plays = line.Split(' ');

				PlayType opponentPlay = GetPlay(plays[0]);
				PlayType yourPlay = GetPlay(plays[1]);

				int outcome = CalculateOutcome(opponentPlay, yourPlay);
				
				opponentTotalScore += (int)opponentPlay + 6 - outcome;
				totalScore += (int)yourPlay + outcome;
			}
			while (!input.EofReached());
		}

		bool win = totalScore > opponentTotalScore;
	}

	private int CalculateOutcome(PlayType opponentPlay, PlayType yourPlay)
	{
		if (opponentPlay == yourPlay)
		{
			return 3;
		}
		
		if (yourPlay == opponentPlay + 1 || yourPlay - opponentPlay == -2)
		{
			return 6;
		}

		return 0;
	}

	private PlayType GetPlay(string play)
	{
		switch (play)
		{
			case "A":
			case "X":
				return PlayType.Rock;
			case "B":
			case "Y":
				return PlayType.Paper;
			case "C":
			case "Z":
				return PlayType.Scissors;
			default:
				throw new ArgumentException($"Unknown play {play} provided", nameof(play));
		}
	}
	private enum PlayType
	{
		Rock = 1,
		Paper = 2,
		Scissors = 3,
	}
}