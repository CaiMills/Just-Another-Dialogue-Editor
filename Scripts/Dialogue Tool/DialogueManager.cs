using Godot;
using System;
using System.Text.Json;

public partial class DialogueManager : Control
{
    [ExportCategory("Dialogue UI")]
    private Panel _dialogueBox;
    private RichTextLabel _dialogueText;
    private Panel _nameBox;
    private RichTextLabel _nameText;
    private TextureRect _portrait;

    [ExportCategory("Variables")]
    private Conversation _script;
    private int _lineId;
    private Dialogue _currentLine;
    private Timer _textTimer;
    private bool _isChoosing;

    public override void _Ready()
    {
        // Gets the necessary nodes
        _dialogueBox = GetNode<Panel>("DialogueBox");
        _dialogueText = GetNode<RichTextLabel>("DialogueBox/MarginContainer/DialogueText");
        _nameBox = GetNode<Panel>("NameBox");
        _nameText = GetNode<RichTextLabel>("NameBox/MarginContainer/NameText");
        _portrait = GetNode<TextureRect>("PortraitBox/Portrait");
        
        // Connects it to the correct signal, so it knows when to activate
        if (GUIManager._instance != null)
        {
            GUIManager._instance.Connect("DialogueActivate", new Callable(this, nameof(OnStartConversation)));
        }
        
        // Initializes the timer for the text speed
        _textTimer = new Timer();
        _textTimer.Timeout += _on_Text_Timer_Timeout;
        AddChild(_textTimer);
        
        _isChoosing = false;
    }
    
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept") && GUIManager._instance._isMenuActive && !_isChoosing)
        {
            if (_dialogueText.VisibleCharacters == _dialogueText.Text.Length)
            {
                NextLine();
            }
            // Skips the text speed and immediately displays the current line
            else
            {
                _dialogueText.VisibleCharacters = _dialogueText.Text.Length;
            }
        }
    }

    /// <summary>
    /// Deserializes the json file, and sets up the dialogue UI
    /// </summary>
    /// <param name="jsonPath"></param>
    public void OnStartConversation(string jsonPath)
    {
        if (jsonPath != String.Empty)
        {
            // This opens the file directory and then converts the json data to string
            using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
            string jsonString = file.GetAsText();
            
            // This turns the json file into a dialogue script using the custom Conversation
            // class which allows for the dialogue to be read.
            _script = JsonSerializer.Deserialize<Conversation>(jsonString);

            if (_script != null && _script.conversation != null && _script.conversation.Count > 0)
            {
                _lineId = 0;
                _currentLine = _script.conversation[_lineId];

                GUIManager._instance._isMenuActive = true;
                ClearDialogue();
                Show();

                ReadLine();
            }
            else
            {
                GD.PrintErr($"Conversation data is missing or empty in {jsonPath}");
            }
        }
        else
        { 
            GD.Print("No conversation found");
        }
    }

    /// <summary>
    /// This displays all the info of the current dialogue sentence, and starts the timer for the typewriter effect.
    /// </summary>
    private void ReadLine()
    {
        _nameText.Text = _currentLine.name;
        
        // This is for loading the character portrait
        if (_portrait != null && !string.IsNullOrEmpty(_currentLine.portraitPath))
        {
            var texture = ResourceLoader.Load<Texture2D>(_currentLine.portraitPath);
            
            if (texture != null)
            {
                _portrait.Texture = texture;
            }
            else
            {
                GD.Print("No texture found");
            }
        }
        
        _dialogueText.Text = _currentLine.text;
        _dialogueText.VisibleCharacters = 0;
        
        _textTimer.WaitTime = _currentLine.textSpeed;
        _textTimer.Start();
    }

    /// <summary>
    /// This reads each character individually, giving the typewriter effect. It also sets the other variables to the current line, such as the characters name.
    /// </summary>
    private void _on_Text_Timer_Timeout()
    {
        if (_dialogueText.VisibleCharacters < _dialogueText.Text.Length)
        {
            _dialogueText.VisibleCharacters++;
        }
        else
        {
            _textTimer.Stop();
        }
    }

    /// <summary>
    /// This cycles to the next line in the list, as well as clears all the text from the previous lines.
    /// </summary>
    private void NextLine()
    {
        // If there's another sentence, set it to that next sentence and reset variables
        if (_lineId < _script.conversation.Count - 1)
        {
            // If there is a choice in the sentence, activate the relevant code
            if (_currentLine.choices != null && _currentLine.choices.Count > 0)
            {
                Choices();
            }
            // Sets the next line to whatever sentence is connected to this one
            else
            {
                if (_currentLine.connectsTo > _script.conversation.Count || _currentLine.connectsTo < 0)
                {
                    EndConversation();
                }
                else
                {
                    _lineId = _currentLine.connectsTo;
                    _currentLine = _script.conversation[_lineId];
                    ClearDialogue();
                
                    ReadLine();
                }
            }
        }
        // Otherwise end the conversation
        else
        {
            EndConversation();
        }
    }

    /// <summary>
    /// This is where it will display any dialogue choices if there is any, before going to the next line
    /// </summary>
    private void Choices()
    {
        _isChoosing = true;
        _dialogueText.Hide();
        _nameBox.Hide();
        
        // For the number of choices given...
        for (int i = 0; i < _currentLine.choices.Count; i++)
        {
            // Instantiate a new button with the appropriate text
            Button choice = new Button()
            {
                Text = _currentLine.choices[i].text,
                Alignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            // Style Box Overrides
            StyleBoxFlat normal = new StyleBoxFlat();
            normal.BgColor = new Color(Color.Color8(15, 40, 51));
            choice.AddThemeStyleboxOverride("normal", normal);
            
            StyleBoxFlat hover = new StyleBoxFlat();
            hover.BgColor = new Color(Color.Color8(25, 59, 74));
            choice.AddThemeStyleboxOverride("hover", hover);
            
            // Font Overrides
            Font font = ResourceLoader.Load<FontFile>("res://Assets/Fonts/RocknRollOne-Regular.ttf"); // NOTE: Font is a placeholder
            choice.AddThemeFontOverride("font", font);
            choice.AddThemeFontSizeOverride("font_size", 18);
            _dialogueBox.GetNode<VBoxContainer>("ScrollContainer/ChoicesVBoxContainer").AddChild(choice);
            
            // Connects a signal to each button that if pressed it will activate the following function, passing its index within the VBoxContainer
            choice.Pressed += () =>
            {
                MakeChoice(choice.GetIndex());
            };
        }
    }

    /// <summary>
    /// This handles the logic for when a choice button is selected
    /// </summary>
    /// <param name="choiceIndex"></param>
    private void MakeChoice(int choiceIndex)
    {
        // Clears all the instantiated choice boxes
        foreach (Node child in _dialogueBox.GetNode<VBoxContainer>("ScrollContainer/ChoicesVBoxContainer").GetChildren())
        {
            child.QueueFree();
        }
        
        // Sets the next line to whatever node was connected to the selected choice
        _lineId = _currentLine.choices[choiceIndex].connectsTo;
        _currentLine = _script.conversation[_lineId];
        
        ClearDialogue();
        
        _dialogueText.Show();
        _nameBox.Show();
        _isChoosing = false;
        
        ReadLine();
    }

    /// <summary>
    /// Clears all the current lines data for the next line
    /// </summary>
    private void ClearDialogue()
    {
        _nameText.Text = String.Empty;
        _dialogueText.Text = string.Empty;
        _portrait.Texture = null;
    }

    /// <summary>
    /// Closes the Dialogue Window and resets all the necessary values
    /// </summary>
    private void EndConversation()
    {
        GUIManager._instance._isMenuActive = false;
        this.Hide();
        _script.conversation.Clear();
        _lineId = 0;
    }
}
