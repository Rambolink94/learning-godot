using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace LearningGodot;

public partial class PuzzleManager : Control
{
	private readonly Dictionary<string, Node2D> _spawnedPuzzles = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var puzzleContainer = GetNode<Node2D>("../../PuzzleContainer");
		var buttonContainer = GetNode<Control>("ButtonContainer");
		var console = GetNode<RichTextLabel>("Console");
		
		var puzzles = buttonContainer.GetChildren();
		
		foreach (Node puzzle in puzzles)
		{
			if (puzzle is Button button)
			{
				string formattedText = FormatLabel(button.Text);
				var puzzlePackage = GD.Load<PackedScene>($"res://Scenes/Puzzles/{formattedText}.tscn");

				button.Pressed += () =>
				{
					// When the button is pressed, create an instance if one doesn't exist.
					if (!_spawnedPuzzles.ContainsKey(formattedText))
					{
						var puzzleInstance = puzzlePackage.Instantiate<PuzzleNode>();
						puzzleInstance.PrintMessage += message => 
						{
							console.Text += $"\n{message}";
						};
						
						puzzleContainer.AddChild(puzzleInstance);
						
						_spawnedPuzzles.Add(formattedText, puzzleInstance);
					}
				};
			}
		}
	}

	private static string FormatLabel(string buttonText)
	{
		// Converts a upper pascal case string to snake case
		string input = buttonText.Replace(" ", string.Empty);
		return Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToLower();
	}
}