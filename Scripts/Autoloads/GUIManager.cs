using System;
using Godot;

public partial class GUIManager : CanvasLayer
{
    public static GUIManager _instance;
    
    private string[] _guiComponents =
    [
        "res://Scenes/MenuConfirmation.tscn",
        "res://Scenes/Dialogue.tscn"
    ];
    
    public bool _isMenuActive;
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
            if (newGui is CanvasLayer canvas)
            {
                AddChild(canvas);
                canvas.Visible = false;
            }
            if (newGui is Control control)
            {
                AddChild(control);
                control.Visible = false;
            }
        }
        
        // Connects signals
        Connect("DialogueActivate", new Callable(this, nameof(DialogueGui)));
        
        //Set default variables
        _isMenuActive = false;
    }

    public void ChangeGui(CanvasLayer currentGui, string newGui, bool closePrevious = true)
    {
        if (closePrevious)
        {
            currentGui.Visible = false;
        }
        CanvasLayer gui = GetNode<CanvasLayer>(newGui);
        gui.Visible = true;
        _isMenuActive = true;
    }

    public void CloseCurrentGui(CanvasLayer currentGui)
    {
        currentGui.Visible = false;
        _isMenuActive = false;
    }

    public void CloseAllGui()
    {
        foreach (CanvasLayer gui in GetChildren())
        {
            gui.Visible = false;
        }
        _isMenuActive = false;
    }
    
    private void DialogueGui(string jsonPath)
    {
        Control dialogue = GetNode<Control>("Dialogue");
        dialogue.Visible = true;
        _isMenuActive = true;
    }
}
