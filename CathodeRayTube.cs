using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class CathodeRayTube : PuzzleNode
{
    private int _register = 1;
    private int _signalMarker = 20;
    private int _signalIncrement = 40;
    private int _clockTick;
    private int _signalStrength;

    private int _screenWidth = 40;
    private int _screenPosition;
    private List<string> _lines = new();
    
    public override void _Ready()
    {
        // Register X
        // Clock ticker
        // Signal Strength = cycle # * register

        foreach (string line in InputReader.ReadInput(10))
        {
            int remainingTicks = 1;
            int processedTicks = 0;
            int parsedValue = 0;
            var commandParts = line.Split(' ');
            bool initialized = false;
            
            while (processedTicks < remainingTicks)
            {
                if (!initialized && commandParts.Length > 1)
                {
                    // addx command, so change remaining to 2.
                    remainingTicks = 2;
                    
                    parsedValue = int.Parse(commandParts[1]);
                    initialized = true;
                }

                _clockTick++;
                processedTicks++;

                if (_clockTick == _signalMarker)
                {
                    _signalStrength += _clockTick * _register;
                    _signalMarker += _signalIncrement;
                }
            }

            _register += parsedValue;
        }
        
        Print($"Signal Strength: {_signalStrength}");
    }
}