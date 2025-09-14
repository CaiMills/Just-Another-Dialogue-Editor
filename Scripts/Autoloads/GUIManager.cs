using Godot;

public partial class GUIManager : CanvasLayer
{
    public static GUIManager _instance;
    
    private string[] _guiComponents =
    [
        "res://Scenes/Dialogue.tscn",
        "res://Scenes/MenuConfirmation.tscn"
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
            if (newGui is CanvasLayer canvasGui)
            {
                AddChild(canvasGui);
                canvasGui.Visible = false;
            }
        }
        
        // Connects signals
        Connect("DialogueActivate", new Callable(this, nameof(DialogueGui)));
    }

    public void ChangeGui(CanvasLayer currentGui, string newGui, bool closePrevious = true)
    {
        if (closePrevious)
        {
            currentGui.Visible = false;
        }
        CanvasLayer gui = GetNode<CanvasLayer>(newGui);
        gui.Visible = true;
    }

    public void CloseCurrentGui(CanvasLayer currentGui)
    {
        currentGui.Visible = false;
    }

    public void CloseAllGui()
    {
        foreach (CanvasLayer gui in GetChildren())
        {
            gui.Visible = false;
        }
    }
    
    private void DialogueGui(string jsonPath)
    {
        CanvasLayer dialogue = GetNode<CanvasLayer>("Dialogue");
        dialogue.Visible = true;
    }
}
