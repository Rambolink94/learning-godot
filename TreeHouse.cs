using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class TreeHouse : Control
{
    public override void _Ready()
    {
        var treePackage = GD.Load<PackedScene>("res://Scenes/tree_2d.tscn");
        var treeGrid = GetNode<GridContainer>("TreeGrid");
        
        // Visible if trees in same row or column are shorter
        // Edge trees always visible
        int lineNumber = 0;
        var rows = new List<string>();
        foreach (string line in InputReader.ReadInput(8))
        {
            // Construct grid
            rows.Add(line);
        }

        treeGrid.Columns = rows.Count;
        
        int margin = 5;
        
        int visibleCount = 0;
        for (int y = 1; y < rows.Count - 1; y++)
        {
            string row = rows[y];
            for (int x = 1; x < row.Length - 1; x++)
            {
                var tree = treePackage.Instantiate<ColorRect>();
                var label = tree.GetNode<Label>("Label");
                label.Text = row[x].ToString();
                
                treeGrid.AddChild(tree);
                
                if (IsVisible(x, y, row, rows.Count, row.Length))
                {
                    tree.Color = Colors.White;
                    visibleCount++;
                }
            }
        }

        // Add outer trees
        visibleCount += rows.Count * 2 + rows[0].Length * 2 - 2;
        
        GD.Print("Visible Count: " + visibleCount);

        bool IsVisible(int x, int y, string row, int rowCount, int columnCount)
        {
            int value = (int)Char.GetNumericValue(row[x]);
            bool visible = true;
            
            // TODO: Consolidate these two loops into one.
            // Process column
            for (int i = 0; i < rowCount; i++)
            {
                int currentValue = (int)Char.GetNumericValue(rows[i][x]);
                
                if (i == y || i == rowCount - 1)
                {
                    // On same value or outer edge
                    if (visible)
                    {
                        // Nothing blocking top or bottom
                        return true;
                    }
                    
                    visible = true;
                    continue;
                }

                if (currentValue >= value)
                {
                    // Discovered a blocker
                    visible = false;
                    
                    if (i > y)
                    {
                        // Blocker on bottom, so break.
                        break;
                    }
                    
                    // Blocker on top, so continue from y.
                    i = y - 1;
                }
            }
                
            // Process row
            visible = true;
            for (int i = 0; i < columnCount; i++)
            {
                int currentValue = (int)Char.GetNumericValue(rows[y][i]);
                
                if (i == x || i == columnCount - 1)
                {
                    // On same value or outer edge
                    if (visible)
                    {
                        // Nothing blocking left or right
                        return true;
                    }
                    
                    visible = true;
                    continue;
                }

                if (currentValue >= value)
                {
                    // Discovered a blocker
                    visible = false;
                    
                    if (i > x)
                    {
                        // Blocker on right, so break.
                        break;
                    }
                    
                    // Blocker on left, so continue from x.
                    i = x - 1;
                }
            }

            // Nothing was blocking this tree
            return false;
        }
    }
}