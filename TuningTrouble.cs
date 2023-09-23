using System;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class TuningTrouble : Node2D
{
    public override void _Ready()
    {
        int firstMarkerIndex = 0;
        var data = InputReader.ReadInput(6).First().AsSpan();
        for (int i = 3; i < data.Length; i++)
        {
            if (FoundUniqueSet(i, data))
            {
                firstMarkerIndex = i;
                break;
            }
        }

        var slice = data.Slice(firstMarkerIndex - 3, 4);
        GD.Print(slice.ToString());

        bool FoundUniqueSet(int i, ReadOnlySpan<char> data)
        {
            var usedCharacters = new char[3];
            int j = i;
            for (int k = 0; j >= i - 3; j--, k++)
            {
                if (usedCharacters.Contains(data[j]))
                {
                    break;
                }

                if (j == i - 3)
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