using Microsoft.Extensions.DependencyInjection;
namespace Minokori.Media.Demo.Godot;

public partial class Main : Node2D
    {
    private int _counter = 0;
    public ServiceProvider Services { get; init; }

    // Called when the node enters the scene tree for the first time.
    public Main()
        {
        ServiceCollection servicesBuilder = new();
        Services = servicesBuilder.BuildServiceProvider();
        }

    public override void _Ready()
        {
        Character a = new("Alice", "./Assets/依神紫苑.psd")
            {
            Position = Vector2.Zero
            };
        //a.SetRelatedPosition(0.5f, 0.5f);
        AddChild(a);
        }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
        {
        }

    public override void _UnhandledInput(InputEvent @event)
        {
        if (@event is InputEventMouseButton keyEvent && keyEvent.Pressed)
            {
            if (keyEvent.ButtonIndex == MouseButton.Left)
                {
                _counter++;

                var a = GetNode<Character>("Alice");
                a["random"].Say($"单击了 {_counter} 次");
                }
            }
        }
    }

