using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        Visible = true;
    }
    
    private void _on_start_pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/DialogueGraph.tscn");
    }

    private void _on_test_pressed()
    {
        // Creates a new file dialog so that the user can use to choose a compatible json file to load as a flowchart
        FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            RootSubfolder = "res://Assets",
            Filters = ["*.json ; JSON Files"]
        };
        fileDialog.Connect("file_selected", new Callable(this, nameof(_on_test_dialog_file_selected)));
        
        GetTree().Root.AddChild(fileDialog);
        fileDialog.PopupCentered();
    }

    private void _on_test_dialog_file_selected(string path)
    {
        if (FileAccess.FileExists(path))
        {
            GUIManager._instance.EmitSignal(nameof(GUIManager.DialogueActivate), path);
        }
    }
    
    private void _on_exit_pressed()
    {
        GetTree().Quit();
    }
}
