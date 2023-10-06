using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class RopeBridge : PuzzleNode
{
	private const int Length = 10;
	private Vector2 _offset;
	private float _scale = 10f;
	private int _activeVisits = 10;
	
	private readonly HashSet<Vector2I> _visitedPositions = new();
	private readonly Queue<Vector2> _activePositions = new();
	private readonly Vector2I[] _rope = new Vector2I[Length];
	private List<(string Direction, int Steps)> _moves;
	
	private Camera2D _camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_camera = GetNode<Camera2D>("../Camera2D");
		_offset = this.Position;

		_camera.GlobalPosition = _offset;
		
		var moves = new List<(string Direction, int Steps)>();
		foreach (string line in InputReader.ReadInput(9))
		{
			var parts = line.Split(' ');
			string direction = parts[0];
			int steps = int.Parse(parts[1]);
			
			moves.Add((direction, steps));
		}

		_moves = moves;
	}

	private bool _playFreely;
	private bool _playInstantly;
	private bool _processRemainingSteps;
	private int _lastStep;
	private int _currentStep;
	private int _steps;
	private string _direction = string.Empty;
	private bool _initialized;
	private bool _finished;
	private int _currentIndex;
	private List<(string Direction, int Steps)>.Enumerator _enumerator;
	
	public override void _Process(double delta)
	{
		while (_playInstantly)
		{
			ProcessMovement();
		}
		
		if (_currentStep == _lastStep
		    && !_processRemainingSteps
		    && !_playFreely
		    && !_playInstantly
		    || _finished) return;
		
		ProcessMovement();

		MoveCamera((float)delta);
	}

	private void ProcessMovement()
	{
		if (_currentStep == _steps || !_initialized)
		{
			if (!_initialized)
			{
				_enumerator = _moves.GetEnumerator();
				
				_visitedPositions.Add(_rope[^1]);
				_activePositions.Enqueue(_rope[^1]);
			}
			
			if (!_enumerator.MoveNext())
			{
				_enumerator.Dispose();
				_finished = true;
				
				Print($"Count: {_visitedPositions.Count}");
				
				return;
			}

			(_direction, _steps) = _enumerator.Current;
			_currentIndex++;
			_currentStep = 0;
			_processRemainingSteps = false;
			_initialized = true;
		}

		_rope[0] = GetPosition(_rope[0], _direction);

		for (int j = 0; j < Length - 1; j++)
		{
			// Move only if distance requirement was met
			if (!(Distance(_rope[j], _rope[j + 1]) > 1.5)) continue;

			// Move tail also
			Vector2I diff = _rope[j + 1] - _rope[j];
			_rope[j + 1] -= Clamp(diff);

			if (j + 1 == Length - 1)
			{
				// Last rope knot
				_visitedPositions.Add(_rope[j + 1]);
				Print($"Visited: {_rope[j + 1]} {_visitedPositions.Count}");

				if (_activePositions.Count > _activeVisits)
				{
					_activePositions.Dequeue();
				}
				
				_activePositions.Enqueue(_rope[j + 1]);
			}
		}

		QueueRedraw();

		if (_processRemainingSteps || _playFreely)
		{
			_currentStep++;
		}

		_lastStep = _currentStep;
		
		return;
		
		Vector2I GetPosition(Vector2I currentPos, string direction)
		{
			int x = currentPos.X + direction switch
			{
				"R" => 1,
				"L" => -1,
				_ => 0
			};

			int y = currentPos.Y + direction switch
			{
				"U" => -1,
				"D" => 1,
				_ => 0
			};

			return new Vector2I(x, y);
		}

		float Distance(Vector2I a, Vector2I b)
		{
			int deltaX = a.X - b.X;
			int deltaY = a.Y - b.Y;

			return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
		}

		Vector2I Clamp(Vector2I original)
		{
			int x = Mathf.Clamp(original.X, -1, 1);
			int y = Mathf.Clamp(original.Y, -1, 1);

			return new Vector2I(x, y);
		}
	}

	private float _followSpeed = 1.0f;
	
	private void MoveCamera(float delta)
	{
		var head = (Vector2)_rope[0] * _scale + _offset;
		Vector2 newPosition = _camera.GlobalPosition.Lerp(head, _followSpeed * delta);

		_camera.GlobalPosition = newPosition;
	}

	private bool _processed = true;
	
	public override void _Input(InputEvent @event)
	{
		bool ctrlPressed = @event is InputEventWithModifiers { CtrlPressed: true };
		
		if (@event is InputEventKey keyEvent)
		{
			if (!_playFreely && keyEvent.KeyLabel == Key.Right)
			{
				// Step forward
				if (keyEvent.IsPressed())
				{
					if (ctrlPressed && _processed && keyEvent.IsPressed())
					{
						// Do all steps
						_processRemainingSteps = true;
						_processed = false;
					}
					else if (_processed && keyEvent.IsPressed())
					{
						// Do single step
						_currentStep++;
						_processed = false;
					}
				}
				
				if (keyEvent.IsReleased())
				{
					_processed = true;
				}
			}
			else if (keyEvent.KeyLabel == Key.Enter && keyEvent.IsPressed())
			{
				if (ctrlPressed)
				{
					_playInstantly = !_playInstantly;
				}
				else
				{
					_playFreely = !_playFreely;	
				}
			}
		}
	}

	private Color _midColor = GetRandomColor();
	private Color _headColor = GetRandomColor();
	private Color _tailColor = GetRandomColor();
	
	public override void _Draw()
	{
		var text = $"Move: {_currentIndex} / {_moves.Count}\n" +
		           $"Direction: {_direction}\n" +
		           $"Step: {_currentStep + 1} / {_steps}\n" +
		           $"Found: {_visitedPositions.Count}\n" +
		           $"FPS: {Engine.GetFramesPerSecond()}";

		var font = new SystemFont();
		var offset = new Vector2(0, 1f);
		var origin = _camera.GlobalPosition + offset * 100f;
		
		DrawMultilineString(font, origin, text);
		
		var activeColor = Colors.Gray;
		
		// Draw trail
		int index = 1;
		foreach (Vector2 activePosition in _activePositions)
		{
			activeColor.A = index++ / (float)_activeVisits;
			
			var drawPosition = activePosition * _scale + _offset;
			DrawCircle(drawPosition, _scale / 2, activeColor);
		}
		
		// Draw rope
		for (int i = _rope.Length - 1; i >= 0; i--)
		{
			Color color = _midColor;
			if (i == 0)
			{
				color = _headColor;
			}
			else if (i == _rope.Length - 1)
			{
				color = _tailColor;
			}
			
			var position = (Vector2)_rope[i] * _scale + _offset;
			DrawCircle(position, _scale / 2, color);
		}
	}
	
	private static Color GetRandomColor()
	{
		return new Color(
			GD.Randf(),
			GD.Randf(),
			GD.Randf()
		);
	}
}