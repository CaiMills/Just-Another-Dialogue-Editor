using Godot;

public partial class DialogueNode : GraphNode
{
    public readonly Dialogue _dialogue = new Dialogue();
    public bool _hasChoices;

    
    [ExportGroup("Node UI")] 
    [Export] private LineEdit _nameBox;
    [Export] private TextEdit _dialogueBox;
    [Export] private SpinBox _textSpeedBox;
    [Export] private TextureRect _portrait;
    
    public override void _Ready()
    {
        // Sets up the slots
        if (!_hasChoices)
        {
            SetSlot(1, true, 0, new Color(1, 1, 1), true, 0, new Color(0, 1, 1));
        }
        
        _hasChoices = false;
        _dialogue.textSpeed = (float)_textSpeedBox.Value;
    }

    /// <summary>
    /// Handles the logic for resizing the node
    /// </summary>
    private void _on_resize_request(Vector2 newSize)
    {
        Size = newSize;
    }

    /// <summary>
    /// Adds a new choice to the list
    /// </summary>
    private void _on_add_choice_button_button_down()
    {
        // Creates and adds choice to list
        Choice choice = new Choice()
        {
            text = "",
            connectsTo = -1
        };
        _dialogue.choices.Add(choice);

        CreateChoice();
    }

    /// <summary>
    /// This adds a choice box to the dialogue node
    /// </summary>
    private void CreateChoice()
    {
        if (!_hasChoices)
        {
            _hasChoices = true;
        }
        // This gets the necessary info so that the choices can be added in the correct position
        Button addChoiceButton = GetNode<Button>("AddChoiceButton");
        
        int index = _dialogue.choices.Count - 1;
        
        // Disables the right slot of the first slot, as it wouldn't be used if there are any choices
        if (IsSlotEnabledRight(1))
        {
            SetSlotEnabledRight(1, false);
        }
        
        // Creates HBoxContainer for the choice box and delete choice button
        HBoxContainer choiceContainer = new HBoxContainer()
        {
            Name = "ChoiceContainer" + index,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        // The position in the hierarchy the new instance will be placed
        AddChild(choiceContainer);
        MoveChild(choiceContainer, addChoiceButton.GetIndex());
        
        // Creates the choice box
        TextEdit choiceBox = new TextEdit
        {
            PlaceholderText = "Choice Text Here...",
            WrapMode = TextEdit.LineWrappingMode.Boundary,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ScrollFitContentHeight = true
        };
        choiceBox.AddThemeFontSizeOverride("font_size", 20);
        choiceBox.TooltipText = "This determines the choices text.";
        
        // This is for data loading; if the choice has any text, then the choice box's text will be set to that rather than be empty
        if (_dialogue.choices[index].text != null)
        {
            choiceBox.Text = _dialogue.choices[index].text;
        }
        // Saves the text whenever there's any change for serialization
        choiceBox.TextChanged += () =>
        {
            _dialogue.choices[index].text = choiceBox.Text;
        };
        choiceContainer.AddChild(choiceBox);
        
        // Sets up connection slots (2 to account for the gap in space)
        SetSlot(index + 2, false, 0, new Color(1, 0, 0, 1), true, 0, new Color(1, 0, 0, 1));
        
        // Adds the choice deletion button to the last choice
        DeleteChoiceButton();
    }

    /// <summary>
    /// This handles instantiating the delete button on the last choice
    /// </summary>
    private void DeleteChoiceButton()
    {
        // TODO
        // Make it remove the height added from the choices (for the first choice as it works for all the others)
        // Make it disconnect from any connected slots when deleted (if possible)
        
        int index = _dialogue.choices.Count - 1;
        HBoxContainer choiceContainer = GetNodeOrNull<HBoxContainer>("ChoiceContainer" + index);

        if (choiceContainer != null)
        {
            // This loops through each choice and deletes all the delete choice buttons, so that only the newest one has the option
            // This is because of a bug that currently exists, where deleting a choice out of index order breaks everything.
            foreach (var hBox in GetChildren())
            {
                if (hBox is HBoxContainer)
                {
                    foreach (var button in hBox.GetChildren())
                    {
                        if (button is Button)
                        {
                            button.QueueFree();
                        }
                    }
                }
            }
            // Creates the delete choice button
            Button choiceDeleteButton = new Button()
            {
                Text = "x"
            };
            choiceDeleteButton.TooltipText = "This removes the aligned choice.";

            // Deletes the choice sharing its index
            choiceDeleteButton.Pressed += () =>
            {
                // Remove from the node box
                choiceContainer.QueueFree();

                // Remove from the data list
                if (_dialogue.choices.Count > index)
                {
                    _dialogue.choices.RemoveAt(index);
                }
                // Remove the slot visually
                ClearSlot(index + 2);
                
                // Restore right connection slot if no more choices
                if (_dialogue.choices.Count == 0)
                {
                    _hasChoices = false;
                    SetSlotEnabledRight(1, true);
                }
                // Removes the space left by the choice
                Size = new Vector2(GetSize().X, GetSize().Y - 1000);
                
                // Creates a new delete button for the next choice in the list
                if (_dialogue.choices.Count > 0)
                {
                    DeleteChoiceButton();
                }
            };
            choiceContainer.AddChild(choiceDeleteButton);
            choiceContainer.MoveChild(choiceDeleteButton, 0);
        }
    }
    
    private void _on_name_box_text_changed(string newName)
    {
        _dialogue.name = newName;
    }

    private void _on_dialogue_box_text_changed()
    {
        string text = _dialogueBox.Text;
        _dialogue.text = text;
    }
    
    private void _on_text_speed_box_value_changed(float value)
    {
        _dialogue.textSpeed = value;
    }

    /// <summary>
    /// This opens the file dialogue screen to select a file
    /// </summary>
    private void _on_portrait_button_pressed()
    {
        FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            RootSubfolder = "res://Assets",
            Filters = ["*.svg ; SVG Files", "*.png ; PNG Files"],
        };
        fileDialog.Connect("file_selected", new Callable(this, nameof(_on_file_dialog_file_selected)));
        
        GetTree().Root.AddChild(fileDialog);
        fileDialog.PopupCentered();
    }
    
    private void _on_file_dialog_file_selected(string path)
    {
        // This stores the variable to be serialized
        _dialogue.portraitPath = path;
        ChangePortrait(path);
    }

    /// <summary>
    /// Handles the changing of the TextureRect to the selected sprite
    /// </summary>
    /// <param name="path"></param>
    private void ChangePortrait(string path)
    {
        // This loads the image so you can see it on the node (purely for aesthetic
        Texture2D texture = ResourceLoader.Load<Texture2D>(path);
        _portrait.Texture = texture;
    }

    /// <summary>
    /// This sets all the text for the box to whatever is stored
    /// </summary>
    public void LoadData(Dialogue loaded)
    {
        // Sets the empty data to be the same as the loaded data
        _dialogue.id = loaded.id;
        _dialogue.name = loaded.name;
        _dialogue.text = loaded.text;
        _dialogue.textSpeed = loaded.textSpeed;
        _dialogue.portraitPath = loaded.portraitPath;
        _dialogue.connectsTo = loaded.connectsTo;
        _dialogue.offsetX = loaded.offsetX;
        _dialogue.offsetY = loaded.offsetY;
        _dialogue.width = loaded.width;
        _dialogue.height = loaded.height;
        
        // Gives all the UI elements the correct data
        _nameBox.Text = _dialogue.name;
        _dialogueBox.Text = _dialogue.text;
        _textSpeedBox.Value = _dialogue.textSpeed;
        if (FileAccess.FileExists(_dialogue.portraitPath))
        {
            ChangePortrait(_dialogue.portraitPath);
        }
        
        // Loops through each choice to recreate them as well
        foreach (Choice choice in loaded.choices)
        {
            Choice newChoice = new Choice()
            {
                text = choice.text,
                connectsTo = choice.connectsTo
            };
            _dialogue.choices.Add(newChoice);
            CreateChoice();
        }
        Size = new Vector2(_dialogue.width, _dialogue.height);
    }
}
