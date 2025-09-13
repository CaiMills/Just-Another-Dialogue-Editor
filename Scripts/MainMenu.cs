using Godot;
using System;

public partial class MainMenu : Control
{
    private void _on_start_pressed()
    {
        GUIManager._instance.ChangeGui(this, "DialogueGraph");
    }
    
    private void _on_exit_pressed()
    {
        GetTree().Quit();
    }
}
