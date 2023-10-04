using Godot;
using System;
using System.Collections.Generic;
using LearningGodot;

public partial class RopeBridge : Control
{
	private GridContainer _grid;
	private const string HeadChar = "H";
	private const string TailChar = "T";
	private const string StartChar = "s";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int size = 0;
		List<string> moves = new();
		foreach (string line in InputReader.ReadInput(9))
		{
			int value = int.Parse(line.Split(' ')[1]);
			if (value > size)
			{
				// Get how big the grid needs to be.
				size = value;
			}
			
			moves.Add(line);
		}

		var cellPackage = GD.Load<PackedScene>("res://Scenes/cell.tscn");
		_grid = GetNode<GridContainer>("BridgeGrid");

		Vector2I head = Vector2I.Zero;
		Vector2I tail = Vector2I.Zero;
		
		// Generate grid
		var cells = new Slot[size * size];
		_grid.Columns = size;
		int middle = size / 2;
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				var cell = cellPackage.Instantiate<ColorRect>();
				bool isStartingCell = x == middle && y == middle;

				var slot = new Slot(cell, isStartingCell);
				cells[y * size + x] = slot;

				if (isStartingCell)
				{
					slot.Text = HeadChar;

					head = new Vector2I(x, y);
					tail = new Vector2I(x, y);
				}
			
				_grid.AddChild(cell);
			}
		}
		// Handle moves
		foreach (string move in moves)
		{
			var parts = move.Split(' ');
			string direction = parts[0];
			int steps = int.Parse(parts[1]);
			
			MoveRope(head, direction, steps, HeadChar);
		}

		void MoveRope(Vector2I rope, string direction, int steps, string symbol)
		{
			int newIndex = GetDirectionIncrement(rope, direction);
			var currentSlot = GetSlot(rope.X, rope.Y);
			var newCell = GetSlot(rope.X, newIndex);

			currentSlot.Text = string.Empty;
		}

		Slot GetSlot(int x, int y)
		{
			return cells[y * size + x];
		}

		int GetDirectionIncrement(Vector2I currentPos, string direction)
		{
			return direction switch
			{
				"R" => 1,
				"L" => -1,
				"D" => currentPos.X + size,
				"U" => currentPos.X + size * -1,
				_ => throw new ArgumentException("Direction doesn't exist.", nameof(direction)),
			};
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
                    motionEvent.Position.X - _grid.Position.X,
                    motionEvent.Position.Y - _grid.Position.Y);
            }

            var position = new Vector2(
                motionEvent.Position.X - _offset.X,
                motionEvent.Position.Y - _offset.Y);
            
            _grid.Position = position;
        }
    }

    private void Zoom(Vector2 mousePosition, float zoomAmount)
    {
        var anchor = new Vector2(
            (mousePosition.X - _grid.Position.X) / _zoomFactor,
            (mousePosition.Y - _grid.Position.Y) / _zoomFactor);

        // Update zoom factor
        _zoomFactor *= zoomAmount;
        
        // Move and zoom in relation to mouse position
        _grid.Scale *= zoomAmount;
        _grid.Position = new Vector2(
            mousePosition.X - anchor.X * _zoomFactor,
            mousePosition.Y - anchor.Y * _zoomFactor);
    }

    private class Rope
    {
	    public string Symbol { get; }
	    
	    public Vector2 Position { get; set; }

	    public Rope(string symbol)
	    {
		    Symbol = symbol;
	    }
    }
    
	private class Slot
	{
		public Slot(ColorRect rect, bool isStart = false)
		{
			Rect = rect;
			IsVisited = isStart;
			IsStart = isStart;
			Text = string.Empty;
		}
		
		private Label _label;
		
		public bool IsVisited { get; set; }
		
		public bool IsStart { get; private set; }
		
		public ColorRect Rect { get; }

		public string Text
		{
			get
			{
				var label = GetLable();

				return label.Text;
			}

			set
			{
				var label = GetLable();
				
				label.Text = value == string.Empty && IsStart ? StartChar : value;
			}
		}

		private Label GetLable()
		{
			if (_label == null)
			{
				_label = Rect.GetNode<Label>("Label");
			}

			return _label;
		}
	}
}
