using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class HillClimbing : PuzzleNode
{
    public override void _Ready()
    {
        List<char[]> grid = new();
        int lineNumber = 0;
        Vector2 playerPosition = Vector2.Zero;
        Vector2 highestPosition = Vector2.Zero;
        
        // Setup grid.
        foreach (string line in InputReader.ReadInput(12))
        {
            grid.Add(line.ToCharArray());
            
            var startIndex = line.IndexOf('S');
            if (startIndex != -1)
            {
                // Setup player's starting position.
                playerPosition = new Vector2(startIndex, lineNumber);
                grid[lineNumber][startIndex] = 'a';
            }

            var endIndex = line.IndexOf('E');
            if (endIndex != -1)
            {
                // Setup the end location, aka highest position.
                highestPosition = new Vector2(endIndex, lineNumber);
                grid[lineNumber][endIndex] = 'z';
            }

            lineNumber++;
        }

        DijkstraAlgorithm(playerPosition);
        
        return;

        void DijkstraAlgorithm(Vector2 currentPosition)
        {
            var weights = new Dictionary<Vector2, int>();
            var queue = new PriorityQueue<Vector2, int>();
            
            queue.Enqueue(currentPosition, 0);

            // Initialize weights.
            for (int y = 0; y < grid.Count; y++)
            {
                for (int x = 0; x < grid[y].Length; x++)
                {
                    var position = new Vector2(x, y);
                    weights.Add(position, position == currentPosition ? 0 : Int32.MaxValue);
                }
            }

            while (queue.Count > 0)
            {
                Vector2 position = queue.Dequeue();

                if (position == highestPosition)
                {
                    // Found the destination
                    break;
                }

                var neighbours = GetNeighbours(position);
                foreach (Vector2 neighbour in neighbours)
                {
                    int distance = weights[position] + 1;
                    if (distance < weights[neighbour])
                    {
                        weights[neighbour] = distance;
                        queue.Enqueue(neighbour, distance);
                    }
                }
            }

            Print($"Fewest steps: {weights[highestPosition]}");
        }

        List<Vector2> GetNeighbours(Vector2 currentPosition)
        {
            var neighbours = new List<Vector2>();
            
            int x = (int)currentPosition.X;
            int y = (int)currentPosition.Y;

            int currentHeight = grid[y][x];

            VerifyAndAdd(x - 1, y); // Left
            VerifyAndAdd(x, y + 1); // Top
            VerifyAndAdd(x + 1, y); // Right
            VerifyAndAdd(x, y - 1); // Bottom

            return neighbours;

            void VerifyAndAdd(int innerX, int innerY)
            {
                try
                {
                    var height = grid[innerY][innerX];
                    if (currentHeight + 1 == height || currentHeight >= height)
                    {
                        // If height is either one more than current or less than current
                        neighbours.Add(new Vector2(innerX, innerY));
                    }
                }
                catch (ArgumentOutOfRangeException) {}
                catch (IndexOutOfRangeException) {}
            }
        }
    }
}