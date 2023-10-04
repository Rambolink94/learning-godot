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
        var cellPackage = GD.Load<PackedScene>("res://Scenes/cell.tscn");
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
        int bestScenicScore = int.MinValue;
        Vector2 bestScorePosition = Vector2.Inf;
        for (int y = 0; y < _rowCount; y++)
        {
            string row = _rows[y];
            for (int x = 0; x < _columnCount; x++)
            {
                var tree = cellPackage.Instantiate<ColorRect>();
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
                int scenicScore = blockerInfo.GetScenicScore(_columnCount, _rowCount);
                if (scenicScore > bestScenicScore)
                {
                    bestScenicScore = scenicScore;
                    bestScorePosition = blockerInfo.Origin;
                }
                
                tree.TooltipText = $"Position: {x}, {y} \nScore: {scenicScore} \nVisible: {visible} {count}";

                var cellData = new CellData
                {
                    Position = new Vector2(x, y),
                    BlockerInfo = blockerInfo,
                    ColorRect = tree,
                };
                
                _trees.Add(cellData);
            }
        }
        
        GD.Print($"Visible Count: {visibleCount} ScenicScore: {bestScenicScore} at {bestScorePosition}");
    }

    private BlockerInfo GetBlockers(int x, int y)
    {
        var value = (int)Char.GetNumericValue(_rows[y][x]);
        
        var leftBlocker = GetBlockerInRange(0, x - 1, -1, Axis.X);
        var rightBlocker = GetBlockerInRange(x + 1, _columnCount, 1, Axis.X);
        var upBlocker = GetBlockerInRange(0, y - 1, -1, Axis.Y);
        var bottomBlocker = GetBlockerInRange(y + 1, _rowCount, 1, Axis.Y);

        return new BlockerInfo
        {
            Origin = new Vector2(x, y),
            Blockers = new [] { leftBlocker, upBlocker, rightBlocker, bottomBlocker },
        };

        Blocker GetBlockerInRange(int min, int max, int direction, Axis axis)
        {
            Vector2 position = Vector2.Inf;
            int i = direction > 0 ? min : max;
            for (; direction > 0 ? i < max : i >= min; i += direction)
            {
                int comparison = (int)Char.GetNumericValue(axis == Axis.X ? _rows[y][i] : _rows[i][x]);
                if (comparison >= value)
                {
                    position = axis == Axis.X ? new Vector2(i, y) : new Vector2(x, i);
                    break;
                }
            }

            return new Blocker
            {
                Position = position,
                Axis = axis,
                Direction = direction,
            };
        }
    }

    private struct BlockerInfo
    {
        public Vector2 Origin;
        
        public Blocker[] Blockers;

        public readonly bool IsVisible()
        {
            foreach (Blocker blocker in Blockers)
            {
                if (blocker.Position == Vector2.Inf)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly int GetScenicScore(int columnCount, int rowCount)
        {
            int total = 1;
            foreach (Blocker blocker in Blockers)
            {
                Vector2 diff = Vector2.Zero;
                if (!blocker.Position.IsFinite())
                {
                    // If direction is negative, then value is 0.
                    // Else if axis is X axis, use column count otherwise row count.
                    if (blocker.Axis == Axis.X)
                    {
                        int x = blocker.Direction < 0 ? 0 : columnCount - 1;
                        diff.X = MathF.Abs(Origin.X - x);
                    }
                    else
                    {
                        int y = blocker.Direction < 0 ? 0 : rowCount - 1;
                        diff.Y = MathF.Abs(Origin.Y - y);;
                    }
                }
                else
                {
                    diff = blocker.Position - Origin;
                }
                
                int diffX = Mathf.Max((int)Mathf.Abs(diff.X), 1);
                int diffY = Mathf.Max((int)Mathf.Abs(diff.Y), 1);

                int value = diffX * diffY;

                // Multiply the blocker's difference to the total
                // The difference gives how many trees are between origin and blocker
                total *= Mathf.Max(value, 1);
            }

            return total;
        }
    }

    private struct Blocker
    {
        public Vector2 Position;
        public int Direction;
        public Axis Axis;
    }
    
    private enum Axis
    {
        X,
        Y,
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
                foreach (Blocker blocker in cellData.BlockerInfo.Blockers)
                {
                    if (!blocker.Position.IsFinite()) continue;
                    
                    int i = (int)blocker.Position.Y * _columnCount + (int)blocker.Position.X;
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