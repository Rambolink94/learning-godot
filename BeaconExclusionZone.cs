using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class BeaconExclusionZone : PuzzleNode
{
    public override void _Ready()
    {
        int searchRow = 0;
        var sensors = new List<Sensor>();
        
        int minX = int.MaxValue;
        int maxX = 0;
        int minY = 0;
        int maxY = int.MaxValue;
        
        foreach (string line in InputReader.ReadInput(15))
        {
            if (line == string.Empty) continue;

            if (line.Contains("Search"))
            {
                searchRow = int.Parse(line.Split(':')[1]);
                continue;
            }
            
            var parts = line.Split(':');
            var sensorPos = ParseLocation(parts[0]);
            var beaconPos = ParseLocation(parts[1]);

            var sensor = new Sensor(sensorPos, beaconPos);
            sensors.Add(sensor);
            
            Bounds bounds = sensor.Bounds;
            
            minX = Math.Min(minX, bounds.Min.X);
            maxX = Math.Max(maxX, bounds.Max.X);
            minY = Math.Max(minY, bounds.Min.Y);
            maxY = Math.Min(maxY, bounds.Max.Y);
            
            continue;

            Vector2I ParseLocation(string part)
            {
                var locationInfo = part.Split(',');
                int x = int.Parse(locationInfo[0].Split('=')[1]);
                int y = int.Parse(locationInfo[1].Split('=')[1]);

                return new Vector2I(x, y);
            }
        }
        
        int invalidPositions = 0;
        for (int x = minX; x <= maxX; x++)
        {
            var currentPos = new Vector2I(x, searchRow);
            if (sensors.Any(sensor => sensor.BeaconPosition != currentPos
                                      && sensor.DistanceFrom(currentPos) <= sensor.DistanceToBeacon))
            {
                invalidPositions++;
            }
        }
        
        // PrintGrid();
        
        Print($"Invalid positions: {invalidPositions}");
        
        return;
        
        void PrintGrid()
        {
            foreach (Sensor sensor in sensors)
            {
                for (int y = maxY; y < minY; y++)
                {
                    string line = string.Empty;
                    for (int x = minX; x <= maxX; x++)
                    {
                        var currentPos = new Vector2I(x, y);
                        char symbol = '.';
                        if (sensor.Position == currentPos)
                        {
                            symbol = 'S';
                        }
                        else if (sensor.BeaconPosition == currentPos)
                        {
                            symbol = 'B';
                        }
                        else if (sensor.DistanceFrom(currentPos) <= sensor.DistanceToBeacon)
                        {
                            symbol = currentPos.Y == searchRow ? '@' : '#';
                        }
                        else if (currentPos.X >= sensor.Bounds.Min.X &&
                                 currentPos.X <= sensor.Bounds.Max.X &&
                                 currentPos.Y <= sensor.Bounds.Min.Y &&
                                 currentPos.Y >= sensor.Bounds.Max.Y)
                        {
                            symbol = '$';
                        }
                        else if (currentPos.Y == searchRow)
                        {
                            symbol = '*';
                        }
                        

                        line += symbol;
                    }
                
                    Print(line);
                }
                
                Print(string.Empty);
            }
        }
    }

    private record Sensor
    {
        public Sensor(Vector2I position, Vector2I beaconPosition)
        {
            Position = position;
            BeaconPosition = beaconPosition;
            
            DistanceToBeacon = ManhattanDistance(Position, BeaconPosition);
            
            var min = new Vector2I(Position.X - DistanceToBeacon, Position.Y + DistanceToBeacon);
            var max = new Vector2I(Position.X + DistanceToBeacon, Position.Y - DistanceToBeacon);

            Bounds = new Bounds(min, max);
        }
        
        public Vector2I Position { get; }
        
        public Vector2I BeaconPosition { get; }
        
        public int DistanceToBeacon { get; }

        public Bounds Bounds { get; }
        
        public int DistanceFrom(Vector2I other)
        {
            return ManhattanDistance(Position, other);
        }
        
        private int ManhattanDistance(Vector2I a, Vector2I b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
    
    private readonly record struct Bounds(Vector2I Min, Vector2I Max);
}