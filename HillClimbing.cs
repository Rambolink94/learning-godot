using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class HillClimbing : PuzzleNode
{
    private List<char[]> _grid = new();
    private List<Vector2> _startingPoints = new();
    private Vector2 _highestPosition = Vector2.Zero;
    
    public override void _Ready()
    {
        int lineNumber = 0;
        
        // Setup grid.
        foreach (string line in InputReader.ReadInput(12))
        {
            _grid.Add(line.ToCharArray());
            
            for (int i = 0; i < line.Length; i++)
            {
                if (_grid[lineNumber][i] == 'S')
                {
                    // Convert starting 'S' to an 'a'
                    _grid[lineNumber][i] = 'a';
                }
                else if (_grid[lineNumber][i] == 'E')
                {
                    // Setup the end location, aka highest position.
                    _highestPosition = new Vector2(i, lineNumber);
                    _grid[lineNumber][i] = 'z';
                }

                if (_grid[lineNumber][i] == 'a')
                {
                    // Add all low points to starting points.
                    _startingPoints.Add(new Vector2(i, lineNumber));
                }
            }

            lineNumber++;
        }
    }

    private bool _statusReported;
    private int _index;
    private int _shortestRoute = int.MaxValue;
    private Vector2 _shortestStartingPoint = Vector2.Zero;
    
    public override void _Process(double delta)
    {
        if (_index < _startingPoints.Count)
        {
            int steps = DijkstraAlgorithm(_startingPoints[_index]);

            string stepsString = string.Empty;
            if (steps < _shortestRoute)
            {
                _shortestRoute = steps;
                _shortestStartingPoint = _startingPoints[_index];

                stepsString = $" with fewer steps of {_shortestRoute}";
            }
            
            Print($"Processed {_startingPoints[_index]}{stepsString} :: ({_index + 1} / {_startingPoints.Count})");

            _index++;
        }

        if (_index < _startingPoints.Count || _statusReported) return;
        
        Print($"Shortest path is {_shortestStartingPoint} with {_shortestRoute} steps");

        _statusReported = true;

        return;

        int DijkstraAlgorithm(Vector2 currentPosition)
        {
            var weights = new Dictionary<Vector2, int>();
            var queue = new PriorityQueue<Vector2, int>();
            
            queue.Enqueue(currentPosition, 0);

            // Initialize weights.
            for (int y = 0; y < _grid.Count; y++)
            {
                for (int x = 0; x < _grid[y].Length; x++)
                {
                    var position = new Vector2(x, y);
                    weights.Add(position, position == currentPosition ? 0 : Int32.MaxValue);
                }
            }

            while (queue.Count > 0)
            {
                Vector2 position = queue.Dequeue();

                if (position == _highestPosition)
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

            return weights[_highestPosition];
        }

        List<Vector2> GetNeighbours(Vector2 currentPosition)
        {
            var neighbours = new List<Vector2>();
            
            int x = (int)currentPosition.X;
            int y = (int)currentPosition.Y;

            int currentHeight = _grid[y][x];

            VerifyAndAdd(x - 1, y); // Left
            VerifyAndAdd(x, y + 1); // Top
            VerifyAndAdd(x + 1, y); // Right
            VerifyAndAdd(x, y - 1); // Bottom

            return neighbours;

            void VerifyAndAdd(int innerX, int innerY)
            {
                try
                {
                    var height = _grid[innerY][innerX];
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