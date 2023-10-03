using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class TreeHouse : Control
{
    private readonly List<CellData> _trees = new();
    private readonly Dictionary<int, Color> _originalColors = new();
    
    private GridContainer _treeGrid = null;
    private List<string> _rows = new();
    
    private int _rowCount;
    private int _columnCount;
    
    public override void _Ready()
    {
        var treePackage = GD.Load<PackedScene>("res://Scenes/tree_2d.tscn");
        _treeGrid = GetNode<GridContainer>("TreeGrid");
        
        // Visible if trees in same row or column are shorter
        // Edge trees always visible
        _rows = new List<string>();
        foreach (string line in InputReader.ReadInput(8))
        {
            // Construct grid
            _rows.Add(line);
        }

        _rowCount = _rows.Count;
        _columnCount = _rows[0].Length;
        _treeGrid.Columns = _columnCount;
        
        int visibleCount = 0;
        for (int y = 0; y < _rowCount; y++)
        {
            string row = _rows[y];
            for (int x = 0; x < _columnCount; x++)
            {
                var tree = treePackage.Instantiate<ColorRect>();
                var label = tree.GetNode<Label>("Label");
                label.Text = row[x].ToString();
                
                _treeGrid.AddChild(tree);

                BlockerInfo blockerInfo = GetBlockers(x, y);
                
                bool visible = blockerInfo.IsVisible();
                if (visible)
                {
                    tree.Color = Colors.Red;

                    visibleCount++;
                }

                string count = visible ? $"\nCount: {visibleCount}" : string.Empty;
                tree.TooltipText = $"Position: {x}, {y} \nVisible: {visible} {count}";

                var cellData = new CellData
                {
                    Position = new Vector2(x, y),
                    BlockerInfo = blockerInfo,
                    ColorRect = tree,
                };
                
                _trees.Add(cellData);
            }
        }
        
        // 1809
        GD.Print("Visible Count: " + visibleCount);
    }

    private BlockerInfo GetBlockers(int x, int y)
    {
        var value = (int)Char.GetNumericValue(_rows[y][x]);
        
        var leftBlocker = GetBlockerInRange(0, x - 1, -1, true);
        var rightBlocker = GetBlockerInRange(x + 1, _columnCount, 1, true);
        var upBlocker = GetBlockerInRange(0, y - 1, -1, false);
        var bottomBlocker = GetBlockerInRange(y + 1, _rowCount, 1, false);

        return new BlockerInfo
        {
            Blockers = new [] { leftBlocker, upBlocker, rightBlocker, bottomBlocker },
        };

        Vector2 GetBlockerInRange(int min, int max, int direction, bool xAxis)
        {
            int i = direction > 0 ? min : max;
            for (; direction > 0 ? i < max : i >= min; i += direction)
            {
                int comparison = (int)Char.GetNumericValue(xAxis ? _rows[y][i] : _rows[i][x]);
                if (comparison >= value)
                {
                    return xAxis ? new Vector2(i, y) : new Vector2(x, i);
                }
            }

            return Vector2.Inf;
        }
    }

    private struct BlockerInfo
    {
        public Vector2[] Blockers;

        public readonly bool IsVisible()
        {
            foreach (Vector2 blocker in Blockers)
            {
                if (blocker == Vector2.Inf)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private struct CellData
    {
        public Vector2 Position;
        public BlockerInfo BlockerInfo;
        public ColorRect ColorRect;
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
            else if (mouseEvent.ButtonIndex is MouseButton.Left && mouseEvent.IsPressed())
            {
                HighlightCells(mouseEvent);
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

            QueueRedraw();
            _treeGrid.Position = position;
        }
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

    private void HighlightCells(InputEventMouseButton mouseEvent)
    {
        int index = 0;
        foreach (CellData cellData in _trees)
        {
            var rect = cellData.ColorRect.GetRect();
            if (rect.HasPoint((mouseEvent.GlobalPosition - _treeGrid.GlobalPosition) / _treeGrid.Scale))
            {
                foreach (Vector2 blocker in cellData.BlockerInfo.Blockers)
                {
                    if (!blocker.IsFinite()) continue;
                    
                    int i = (int)blocker.Y * _columnCount + (int)blocker.X;
                    CellData data = _trees[i];
                    Highlight(data.ColorRect, Colors.BlueViolet, i);
                }

                Color color = cellData.BlockerInfo.IsVisible() ? Colors.Gold : Colors.Aqua;
                // If tree's position is at mouse position
                Highlight(cellData.ColorRect, color, index);
            }

            index++;
        }
        
        void Highlight(ColorRect rect, Color color, int i)
        {
            if (rect.Color == color)
            {
                rect.Color = _originalColors[i];
            }
            else
            {
                _ = _originalColors.TryAdd(i, rect.Color);
                rect.Color = color;
            }
        }
    }
}