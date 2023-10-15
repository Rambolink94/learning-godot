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
    private Vector2I _sandSpawnPoint;
    private Vector2I _sandPosition;
    private int _lowestX = int.MaxValue;
    private int _width;
    private int _height;
    private int _xOffset;
    
    public override void _Ready()
    {
        _tileMap = GetNode<TileMap>("TileMap");
        
        int highestX = 0;
        int highestY = 0;
        _lowestX = int.MaxValue;
        
        var segmentPoints = new List<List<Vector2I>>();
        foreach (string line in InputReader.ReadInput(14))
        {
            var currentSegmentPoints = new List<Vector2I>();
            var points = line.Split("->");
            foreach (string point in points)
            {
                var parts = point.Split(',');

                var x = int.Parse(parts[0]);
                var y = int.Parse(parts[1]);
                
                currentSegmentPoints.Add(new Vector2I(x, y));
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

        _sandSpawnPoint = new Vector2I(500 - _lowestX, 0);
        _sandPosition = _sandSpawnPoint;

        _height = highestY + 3;  // 0 is always lowest Y
        _xOffset = _height * 2;
        _width = highestX - _lowestX + 1 + _xOffset;

        // Construct environment
        _grid = new char[_width, _height];
        
        // Draw air and spawn point
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (x == _sandSpawnPoint.X + _xOffset / 2 && y == _sandSpawnPoint.Y)
                {
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
                Vector2I point1 = currentSegmentPoints[j];
                Vector2I point2 = currentSegmentPoints[j + 1];

                int minY = Math.Min(point1.Y, point2.Y);
                int maxY = Math.Max(point1.Y, point2.Y);
                int minX = Math.Min(point1.X, point2.X);
                int maxX = Math.Max(point1.X, point2.X);

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        SetTile(new Vector2I(x - _lowestX + _xOffset / 2, y), Rock);
                    }
                }
            }
        }
        
        // Draw floor
        for (int x = 0; x < _width; x++)
        {
            SetTile(new Vector2I(x, _height - 1), Rock);
        }
    }
    
    private bool _finished;
    private Stack<Vector2I> _path = new();
    private HashSet<Vector2I> _particles = new();
    
    public override void _Process(double delta)
    {
        if (_finished) return;
        
        // Simulate sand falling
        try
        {
            int x = _sandPosition.X + _xOffset / 2;
            int y = _sandPosition.Y;
        
            char currentPointSymbol = _grid[x, y];
            if (currentPointSymbol is Rock or Sand)
            {
                if (x == _sandSpawnPoint.X + _xOffset / 2 && y == _sandSpawnPoint.Y)
                {
                    // This allows us to escape out the try.
                    // Perhaps isn't the best way to do it...but it does work.
                    throw new IndexOutOfRangeException();
                }
                
                // Check bottom-left
                char bottomLeftPointSymbol = _grid[x - 1, y];
                if (bottomLeftPointSymbol is Air)
                {
                    _sandPosition = new Vector2I(_sandPosition.X - 1, y);
                    return;
                }

                // Check bottom-right
                char bottomRightPointSymbol = _grid[x + 1, y];
                if (bottomRightPointSymbol is Air)
                {
                    _sandPosition = new Vector2I(_sandPosition.X + 1, y);
                    return;
                }
            
                // Can rest
                if (!_path.TryPop(out _sandPosition))
                {
                    _sandPosition = _sandSpawnPoint;
                }

                var pos = new Vector2I(x, y - 1);
                SetTile(pos, Sand);
                _particles.Add(pos);
                
                return;
            }
            
            _sandPosition = new Vector2I(_sandPosition.X, y + 1);
            _path.Push(_sandPosition);
            
            return;
        }
        catch (IndexOutOfRangeException) { }    // If we try and access an invalid index, we've reached the void

        _finished = true;
        Print($"Total Sand Particles: {_particles.Count}");
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

        _grid[position.X, position.Y] = symbol;
        _tileMap.SetCell(0, position, 1, new Vector2I(x, 0));
    }
    
    private void PrintGrid()
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