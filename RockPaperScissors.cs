using System;
using Godot;

namespace LearningGodot;

public partial class RockPaperScissors : Node
{
	private const string InputPath = "res://Input/day_2_input.txt";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int totalScore = 0;
		int opponentTotalScore = 0;

		foreach (string line in InputReader.ReadInput(2))
		{
			string[] plays = line.Split(' ');

			PlayType opponentPlay = GetPlay(plays[0]);
			PlayType yourPlay = GetPlayForDesiredOutcome(plays[1], opponentPlay);

			int outcome = CalculateOutcome(opponentPlay, yourPlay);

			opponentTotalScore += (int)opponentPlay + 6 - outcome;
			totalScore += (int)yourPlay + outcome;
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

	private PlayType GetPlayForDesiredOutcome(string outcome, PlayType opponentPlay)
	{
		// Default to draw
		PlayType play = opponentPlay;
		if (outcome == "X")
		{
			// Round must be a loss
			play = opponentPlay == PlayType.Rock ? PlayType.Scissors : opponentPlay - 1;
		}
		else if (outcome == "Z")
		{
			// Round must be a win
			play = opponentPlay == PlayType.Scissors ? PlayType.Rock : opponentPlay + 1;
		}

		return play;
	}

	private PlayType GetPlay(string play)
	{
		return play switch
		{
			"A" => PlayType.Rock,
			"B" => PlayType.Paper,
			"C" => PlayType.Scissors,
			_ => throw new ArgumentException($"Unknown play {play} provided", nameof(play)),
		};
	}
	
	private enum PlayType
	{
		Rock = 1,
		Paper = 2,
		Scissors = 3,
	}
}