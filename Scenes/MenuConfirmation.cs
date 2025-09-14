using Godot;

public partial class MenuConfirmation : Control
{
    private void _on_confirm_pressed()
    {
        GUIManager._instance.ChangeGui(this, "Dialogue");

    }

    private void _on_deny_pressed()
    {
        GUIManager._instance.CloseCurrentGui(this);
    }
}
