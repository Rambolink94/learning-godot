using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class RegolithReservoir : PuzzleNode
{
    private const char Rock = '#';
    private const char Air = '.';
    private const char Plus = '+';
    private const char Sand = 'o';
    
    private TileMap _tileMap;
    
    private char[,] _grid;
    private Vector2 _sandSpawnPoint = new(500, 0);
    private Vector2 _sandPosition;
    private int _lowestX = int.MaxValue;
    private int _width;
    private int _height;
    
    public override void _Ready()
    {
        _previousPosition = _sandSpawnPoint;
        _sandPosition = _sandSpawnPoint;
        _tileMap = GetNode<TileMap>("TileMap");
        
        int highestX = 0;
        int highestY = 0;
        _lowestX = int.MaxValue;
        
        var segmentPoints = new List<List<Vector2>>();
        foreach (string line in InputReader.ReadInput(14))
        {
            var currentSegmentPoints = new List<Vector2>();
            var points = line.Split("->");
            foreach (string point in points)
            {
                var parts = point.Split(',');

                var x = int.Parse(parts[0]);
                var y = int.Parse(parts[1]);
                
                currentSegmentPoints.Add(new Vector2(x, y));
                if (x > highestX)
                {
                    highestX = x;
                }
                else if (x < _lowestX)
                {
                    _lowestX = x;
                }

                if (y > highestY)
                {
                    highestY = y;
                }
            }
            
            segmentPoints.Add(currentSegmentPoints);
        }

        _width = highestX - _lowestX + 1;
        _height = highestY + 1;  // 0 is always lowest Y

        // Construct environment
        _grid = new char[_width, _height];
        
        // Draw air and spawn point
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (x == (int)_sandSpawnPoint.X - _lowestX && y == (int)_sandSpawnPoint.Y)
                {
                    _grid[x, y] = Plus;
                    SetTile(new Vector2I(x, y), Plus);
                    continue;
                }
                
                _grid[x, y] = Air;
            }
        }
        
        // Draw rock
        for (int i = 0; i < segmentPoints.Count; i++)
        {
            var currentSegmentPoints = segmentPoints[i];
            for (int j = 0; j < currentSegmentPoints.Count - 1; j++)
            {
                Vector2 point1 = currentSegmentPoints[j];
                Vector2 point2 = currentSegmentPoints[j + 1];

                int minY = (int)Math.Min(point1.Y, point2.Y);
                int maxY = (int)Math.Max(point1.Y, point2.Y);
                int minX = (int)Math.Min(point1.X, point2.X);
                int maxX = (int)Math.Max(point1.X, point2.X);

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        _grid[x - _lowestX, y] = Rock;
                        SetTile(new Vector2I(x - _lowestX, y), Rock);
                    }
                }
            }
        }
    }
    
    private int _totalSandParticles;
    private bool _finished;

    private Vector2 _previousPosition;
    private bool _ignoreErase;
    
    public override void _Process(double delta)
    {
        if (_finished) return;
        
        // Simulate sand falling
        try
        {
            int x = (int)_sandPosition.X - _lowestX;
            int y = (int)_sandPosition.Y;
        
            char currentPointSymbol = _grid[x, y];
            if (currentPointSymbol is Rock or Sand)
            {
                // Check bottom-left
                char bottomLeftPointSymbol = _grid[x - 1, y];
                if (bottomLeftPointSymbol is Air)
                {
                    SetPosition(new Vector2((int)_sandPosition.X - 1, y));
                    return;
                }

                // Check bottom-right
                char bottomRightPointSymbol = _grid[x + 1, y];
                if (bottomRightPointSymbol is Air)
                {
                    SetPosition(new Vector2((int)_sandPosition.X + 1, y));
                    return;
                }
            
                // Can rest
                _grid[x, y - 1] = Sand;
                SetPosition(_sandSpawnPoint);
                SetTile(new Vector2I(x, y - 1), Sand);
                _totalSandParticles++;
                
                return;
            }
            
            SetPosition(new Vector2(_sandPosition.X, y + 1));
            return;
        }
        catch (IndexOutOfRangeException) { }    // If we try and access an invalid index, we've reached the void

        _finished = true;
        Print($"Total Sand Particles: {_totalSandParticles}");
        
        return;

        void SetPosition(Vector2 newPosition)
        {
            _previousPosition = _sandPosition;
            _sandPosition = newPosition;
        }

        void PrintGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                string line = string.Empty;
                for (int x = 0; x < _width; x++)
                {
                    line += _grid[x, y];
                }
                
                Print(line);
            }
            
            Print(string.Empty);
        }
    }

    private void SetTile(Vector2I position, char symbol)
    {
        int x = symbol switch
        {
            Rock => 0,
            Sand => 1,
            Plus => 2,
            Air => -1,
            _ => throw new ArgumentException($"Symbol {symbol} not supported as a tile", nameof(symbol)),
        };

        if (x == -1)
        {
            _tileMap.EraseCell(0, position);
            return;
        }

        _tileMap.SetCell(0, position, 1, new Vector2I(x, 0));
    }
}