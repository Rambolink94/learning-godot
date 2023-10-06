using Godot;

namespace LearningGodot;

public partial class CampCleanup : PuzzleNode
{
    public override void _Ready()
    {
        int totalContainments = 0;
        foreach (string line in InputReader.ReadInput(4))
        {
            string[] sections = line.Split(',');

            string[] leftRange = sections[0].Split('-');
            string[] rightRange = sections[1].Split('-');

            bool areContained = DoRangesOverlap(
                int.Parse(leftRange[0]),
                int.Parse(leftRange[1]),
                int.Parse(rightRange[0]),
                int.Parse(rightRange[1])
            );

            if (areContained)
            {
                totalContainments++;
            }
        }
        
        Print(totalContainments.ToString());
        
        bool DoRangesOverlap(int leftLower, int leftUpper, int rightLower, int rightUpper)
        {
            return (leftLower <= rightUpper && leftUpper >= rightLower);
        }
    }
}