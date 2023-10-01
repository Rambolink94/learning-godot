using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class TreeHouse : Control
{
    private GridContainer _treeGrid = null;
    private List<Control> _trees = new();
    
    public override void _Ready()
    {
        var treePackage = GD.Load<PackedScene>("res://Scenes/tree_2d.tscn");
        _treeGrid = GetNode<GridContainer>("TreeGrid");
        
        // Visible if trees in same row or column are shorter
        // Edge trees always visible
        int lineNumber = 0;
        var rows = new List<string>();
        foreach (string line in InputReader.ReadInput(8))
        {
            // Construct grid
            rows.Add(line);
        }

        _treeGrid.Columns = rows.Count;
        
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
                
                _treeGrid.AddChild(tree);
                _trees.Add(tree);
                
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

    private bool _dragging = false;
    private float _scaleAmount = 0.05f;
    private Vector2 _offset = Vector2.Zero;
    private float _zoomFactor = 1.0f;

    public override void _Input(InputEvent @event)
    {
        // Mouse events
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Middle)
            {
                // Panning
                _offset = Vector2.Zero;
                _dragging = mouseEvent.Pressed;
            }
            else if (mouseEvent.ButtonIndex is MouseButton.WheelUp)
            {
                Zoom(mouseEvent.Position, 1 + _scaleAmount);
            }
            else if (mouseEvent.ButtonIndex is MouseButton.WheelDown)
            {
                Zoom(mouseEvent.Position, 1 - _scaleAmount);
            }
        }
        else if (@event is InputEventMouseMotion motionEvent && _dragging)
        {
            if (Mathf.IsZeroApprox(_offset.X) && Mathf.IsZeroApprox(_offset.Y))
            {
                // Calculate new offset
                _offset = new Vector2(
                    motionEvent.Position.X - _treeGrid.Position.X,
                    motionEvent.Position.Y - _treeGrid.Position.Y);
            }

            var position = new Vector2(
                motionEvent.Position.X - _offset.X,
                motionEvent.Position.Y - _offset.Y);

            GD.Print(position);

            QueueRedraw();
            _treeGrid.Position = position;
        }
    }

    public override void _Draw()
    {
        var font = new SystemFont();
        DrawString(font, _treeGrid.Position, _treeGrid.Position.ToString());
    }

    private void Zoom(Vector2 mousePosition, float zoomAmount)
    {
        var anchor = new Vector2(
            (mousePosition.X - _treeGrid.Position.X) / _zoomFactor,
            (mousePosition.Y - _treeGrid.Position.Y) / _zoomFactor);

        // Update zoom factor
        _zoomFactor *= zoomAmount;
        
        // Move and zoom in relation to mouse position
        _treeGrid.Scale *= zoomAmount;
        _treeGrid.Position = new Vector2(
            mousePosition.X - anchor.X * _zoomFactor,
            mousePosition.Y - anchor.Y * _zoomFactor);
    }
}