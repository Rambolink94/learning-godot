using Godot;

namespace LearningGodot;

public partial class DragAndZoomCamera : Camera2D
{
    private Vector2 _viewportSize;
    private Vector2 _offset;
    private Vector2 _panStart;
    private Vector2 _panOffset;
    private Vector2 _lastMousePosition;
    private float _scaleAmount = 0.05f;
    private float _zoomFactor = 1.0f;
    private float _zoomSpeed = 0.05f;
    private float _zoomStep = 1.1f;

    public override void _Ready()
    {
        _viewportSize = GetViewport().GetVisibleRect().Size;

        Position = _viewportSize * 0.5f;
    }

    public override void _Input(InputEvent @event)
    {
        // Mouse events
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Middle)
            {
                // Panning
                if (mouseEvent.Pressed)
                {
                    _panStart = mouseEvent.Position;
                    _panOffset = Offset;
                }
            }
            else if (mouseEvent.ButtonIndex is MouseButton.WheelUp)
            {
                ZoomTowardsMousePos(mouseEvent.Position, _zoomStep);
            }
            else if (mouseEvent.ButtonIndex is MouseButton.WheelDown)
            {
                ZoomTowardsMousePos(mouseEvent.Position, 1 / _zoomStep);
            }
        }
        else if (@event is InputEventMouseMotion motionEvent)
        {
            if (motionEvent.ButtonMask == MouseButtonMask.Middle)
            {
                Offset = _panOffset - (motionEvent.Position - _panStart) / _zoomFactor;
            }
        }
    }

    private void ZoomCamera(Vector2 zoomCenter, float zoomAmount)
    {
        _zoomFactor *= 1 + zoomAmount;
        Zoom *= new Vector2(_zoomFactor, _zoomFactor);
        Offset = (Offset + _panStart) * (1 + zoomAmount) - zoomCenter * _zoomFactor;
        _lastMousePosition = zoomCenter;
    }

    private void ZoomTowardsMousePos(Vector2 zoomCenter, float delta)
    {
        var nextZoom = Zoom * delta;
        
        Zoom = nextZoom;
        Position += (-0.5f * _viewportSize + zoomCenter) * (Zoom - nextZoom);
    }
}