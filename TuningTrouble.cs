using System;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class TuningTrouble : Node2D
{
    public override void _Ready()
    {
        int firstMarkerIndex = 0;
        int offset = 13;
        var data = InputReader.ReadInput(6).First().AsSpan();
        for (int i = offset; i < data.Length; i++)
        {
            if (FoundUniqueSet(i, data))
            {
                firstMarkerIndex = i;
                break;
            }
        }

        var slice = data.Slice(firstMarkerIndex - offset, offset + 1);
        GD.Print(firstMarkerIndex + 1);

        bool FoundUniqueSet(int i, ReadOnlySpan<char> data)
        {
            var usedCharacters = new char[offset];
            int j = i;
            for (int k = 0; j >= i - offset; j--, k++)
            {
                if (usedCharacters.Contains(data[j]))
                {
                    break;
                }

                if (j == i - offset)
                {
                    // Compared all characters and found no duplicates
                    return true;
                }
                
                usedCharacters[k] = data[j];
            }
            
            return false;
        }
    }
}