using System;
using System.Collections.Generic;
using System.Text;
using Godot;

namespace LearningGodot;

public partial class SupplyStacks : Node2D
{
    public override void _Ready()
    {
        var stacks = new List<Stack<char>>();
        
        bool initialized = false;
        bool moveReady = false;
        foreach (string line in InputReader.ReadInput(5))
        {
            if (!initialized)
            {
                // First entrance, initialize data.
                int stackCount = line.Length / 4 + 1;
                for (int i = 0; i < stackCount; i++)
                {
                    stacks.Add(new Stack<char>());
                }
                
                initialized = true;
            }
            
            if (line == string.Empty)
            {
                // Found blank line before movement begins
                for (int i = 0; i < stacks.Count; i++)
                {
                    // Recreate stacks in reverse order.
                    stacks[i] = new Stack<char>(stacks[i]);
                }
                
                moveReady = true;
                continue;
            }

            if (line.Trim()[0] == '1')
            {
                // Found stack numbering line
                continue;
            }

            if (!moveReady)
            {
                // Still preparing initial stacks
                for (int i = 1, j = 0; i < line.Length; i += 4, j++)
                {
                    if (char.IsUpper(line[i]))
                    {
                        stacks[j].Push(line[i]);
                    }
                }
                
                continue;
            }
            
            // Parse move command
            var span = line.AsSpan();
            int indexOfFrom = span.IndexOf('f');
            var movePositions = span.Slice(indexOfFrom + 4, span.Length - indexOfFrom - 4);
            int indexOfTo = movePositions.IndexOf('t');
            
            int from = int.Parse(movePositions.Slice(0, indexOfTo - 1).Trim());
            int to = int.Parse(movePositions.Slice(indexOfTo + 2, movePositions.Length - indexOfTo - 2).Trim());
            int amountToMove = int.Parse(span.Slice(4, indexOfFrom - 4));

            var grabbedBoxes = new List<char>();
            for (int i = 0; i < amountToMove && stacks[from - 1].Count > 0; i++)
            {
                 grabbedBoxes.Add(stacks[from - 1].Pop());
            }

            for (int i = amountToMove - 1; i >= 0; i--)
            {
                // Insert backwards to maintain order
                stacks[to - 1].Push(grabbedBoxes[i]);
            }
        }

        var topStacksBuilder = new StringBuilder();
        for (int i = 0; i < stacks.Count; i++)
        {
            topStacksBuilder.Append(stacks[i].Pop());
        }
        
        GD.Print(topStacksBuilder.ToString());
    }
}