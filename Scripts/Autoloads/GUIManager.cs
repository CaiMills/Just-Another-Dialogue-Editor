using Godot;

public partial class GUIManager : CanvasLayer
{
    public static GUIManager _instance;
    
    private string[] _guiComponents =
    [
        "res://Scenes/GraphWindow.tscn",
        "res://Scenes/Dialogue.tscn"
    ];
    
    [Signal] public delegate void DialogueActivateEventHandler(string jsonPath);
    
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        
        // This is needed to make the manager's variables and functions accessible
        if (_instance != null)
        {
            GD.Print("Found instance of GUI Manager. Destroying the newest one.");
            QueueFree();
            return;
        }
        _instance = this;
        
        // Adds the packed scenes as children of the GUIManager, and then sets them invisible
        foreach (string gui in _guiComponents)
        {
            var newGui = ResourceLoader.Load<PackedScene>(gui).Instantiate();
            if (newGui is Control controlGui)
            {
                AddChild(controlGui);
                controlGui.Visible = false;
            }
        }
        
        // Connects signals
        Connect("DialogueActivate", new Callable(this, nameof(DialogueGui)));
    }
    
    private void DialogueGui(string jsonPath)
    {
        Control dialogue = GetNode<Control>("Dialogue");
        dialogue.Visible = true;
    }
}
