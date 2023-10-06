using Godot;

namespace LearningGodot;

public abstract partial class PuzzleNode : Node2D
{
    // TODO: Add a toggle hear that disables various visualizations.
    
    [Signal]
    public delegate void PrintMessageEventHandler(string message);

    protected void Print(string message)
    {
        string messageWithType = $"{GetType().Name} :: {message}";
        GD.Print(messageWithType);
        
        EmitSignal(SignalName.PrintMessage, messageWithType);
    }
}