using Godot;
using System;
using System.Collections.Generic;
using LearningGodot;

public partial class RopeBridge : Control
{
	private GridContainer _grid;
	private Slot[,] _cells;
	private List<string>.Enumerator _moves;
	private int _size;
	private int _multiplier = 2;

	private readonly Rope _head = new(HeadChar, Colors.Gold);
	private readonly Rope _tail = new(TailChar, Colors.BlueViolet);
	
	private const string HeadChar = "H";
	private const string TailChar = "T";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		List<string> moves = new();
		foreach (string line in InputReader.ReadInput(9))
		{
			int value = int.Parse(line.Split(' ')[1]);
			if (value > _size)
			{
				// TODO: Calculate max horizontal and vertical movement
				// Get how big the grid needs to be.
				_size = value;
			}
			
			moves.Add(line);
		}

		_moves = moves.GetEnumerator();

		var cellPackage = GD.Load<PackedScene>("res://Scenes/cell.tscn");
		_grid = GetNode<GridContainer>("BridgeGrid");
		
		// Generate grid
		var scaledSize = _size * _multiplier;
		_cells = new Slot[scaledSize, scaledSize];
		_grid.Columns = scaledSize;
		int middle = scaledSize / 2;
		for (int y = 0; y < scaledSize; y++)
		{
			for (int x = 0; x < scaledSize; x++)
			{
				var cell = cellPackage.Instantiate<ColorRect>();
				bool isStartingCell = x == middle && y == middle;

				var slot = new Slot(cell, isStartingCell);
				_cells[x, y] = slot;

				if (isStartingCell)
				{
					// Initialize head and tail.
					slot.UpdateSlot(_head);

					_head.Position = new Vector2(x, y);
					_tail.Position = new Vector2(x, y);
				}
			
				_grid.AddChild(cell);
			}
		}

		return;

		int GetDirectionIncrement(Vector2 currentPos, string direction)
		{
			return direction switch
			{
				"R" => 1,
				"L" => -1,
				"D" => (int)currentPos.X + _size,
				"U" => (int)currentPos.X + _size * -1,
				_ => throw new ArgumentException("Direction doesn't exist.", nameof(direction)),
			};
		}
	}

	double _timePerMove = 0.25f;
	double _increment = 0;

	private string _direction;
	private int _steps = 0;
	private int _currentStep = 0;
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_increment += delta;
		
		// Get next move
		if (_currentStep == _steps && _moves.MoveNext())
		{
			var parts = _moves.Current!.Split(' ');
			_direction = parts[0];
			_steps = int.Parse(parts[1]);
			_currentStep = 0;
		}
		
		for (; _currentStep < _steps; _currentStep++)
		{
			if (_increment < _timePerMove)
			{
				// Has not passed enough time
				return;
			}

			_increment = 0;
			
			// Move head
			MoveRope(_head, _direction);

			int distance = (int)_head.Position.DistanceTo(_tail.Position);
			if (distance > 1)
			{
				// Move tail
			}
			
			UpdateSlots();
		}
	}

	private void MoveRope(Rope rope, string direction)
	{
		Vector2 position = rope.Position;
		
		var oldSlot = GetSlot((int)position.X, (int)position.Y);
		oldSlot.IsVisited = true;
		oldSlot.UpdateSlot();

		float x = position.X + direction switch
		{
			"R" => 1,
			"L" => -1,
			_ => 0
		};
			
		float y = position.Y + direction switch
		{
			"U" => 1,
			"D" => -1,
			_ => 0
		};


		rope.Position = new Vector2(x, y);
	}
	
	private void UpdateSlots()
	{
		var headSlot = GetSlot((int)_head.Position.X, (int)_head.Position.Y);
		var tailSlot = GetSlot((int)_tail.Position.X, (int)_tail.Position.Y);

		tailSlot.UpdateSlot(_tail);
		headSlot.UpdateSlot(_head);
	}
	
	private Slot GetSlot(int x, int y)
	{
		return _cells[x, y];
	}
	
	private class Rope
	{
		public string Symbol { get; }
	    
		public Color Color { get; }
	    
		public Vector2 Position { get; set; }

		public Rope(string symbol, Color color)
		{
			Symbol = symbol;
			Color = color;
		}
	}
    
	private class Slot
	{
		private readonly ColorRect _rect;
		private Label _label;
		
		public Slot(ColorRect rect, bool isVisited = false)
		{
			_rect = rect;
			IsVisited = isVisited;
			Text = string.Empty;
		}
		
		public bool IsVisited { get; set; }

		public Color Color
		{
			set => _rect.Color = value;
		}

		public string Text
		{
			get
			{
				var label = GetLabel();

				return label.Text;
			}

			set
			{
				var label = GetLabel();
				
				label.Text = value;
			}
		}

		public void UpdateSlot(Rope rope = null)
		{
			Text = rope?.Symbol ?? string.Empty;
			Color = rope?.Color ?? (IsVisited ? Colors.Ivory : Colors.Black);
		}

		private Label GetLabel()
		{
			if (_label == null)
			{
				_label = _rect.GetNode<Label>("Label");
			}

			return _label;
		}
	}
	
	private bool _dragging = false;
	private float _scaleAmount = 0.05f;
	private Vector2 _offset = Vector2.Zero;
	private float _zoomFactor = 1.0f;
	private double _timeIncrement = 0.1D;
	
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
        else if (@event is InputEventKey keyEvent)
        {
	        if (keyEvent.KeyLabel == Key.Left)
	        {
		        _timePerMove += _timeIncrement;
	        }
	        else if (keyEvent.KeyLabel == Key.Right)
	        {
		        _timePerMove -= _timeIncrement;
	        }
	        
	        _timePerMove = Mathf.Clamp(_timePerMove, 0.01D, 2D);
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
}
