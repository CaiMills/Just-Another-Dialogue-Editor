using Godot;

public partial class MenuConfirmation : CanvasLayer
{
    private void _on_confirm_pressed()
    {
        GUIManager._instance.CloseCurrentGui(this);
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }

    private void _on_deny_pressed()
    {
        GUIManager._instance.CloseCurrentGui(this);
    }
}
