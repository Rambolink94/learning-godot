using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public static class InputReader
{
    public static IEnumerable<string> ReadInput(int day)
    {
        string inputPath = $"res://Input/day_{day}_input.txt";
        using (var input = FileAccess.Open(inputPath, FileAccess.ModeFlags.Read))
        {
            do
            {
                yield return input.GetLine();
            }
            while (!input.EofReached());
        }
    }
}