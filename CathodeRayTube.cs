using System.Collections.Generic;
using System.Text;

namespace LearningGodot;

public partial class CathodeRayTube : PuzzleNode
{
    private int _register = 1;
    private int _clockTick;
    private int _signalStrength;

    private int _screenWidth = 40;
    private int _screenPosition;
    private List<string> _crtLines = new();

    private const char Period = '.';
    private const char Hash = '#';
    
    public override void _Ready()
    {
        // Register X
        // Clock ticker
        // Signal Strength = cycle # * register

        char[] buffer = new char[_screenWidth];
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

                var character = Period;
                if (_screenPosition == _register - 1 ||
                    _screenPosition == _register ||
                    _screenPosition == _register + 1)
                {
                    character = Hash;
                }

                buffer[_screenPosition++] = character;

                if (_screenPosition == _screenWidth)
                {
                    _crtLines.Add(new string(buffer));
                    buffer = new char[_screenWidth];

                    _screenPosition = 0;
                }
            }

            _register += parsedValue;
        }

        StringBuilder builder = new();
        builder.AppendLine();
        foreach (var line in _crtLines)
        {
            builder.AppendLine(line);
        }
        
        Print(builder.ToString());

        return;
    }
}