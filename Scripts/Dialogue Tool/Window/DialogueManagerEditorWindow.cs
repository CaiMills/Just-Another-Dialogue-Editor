using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

public partial class DialogueManagerEditorWindow : Control
{
    private readonly List<DialogueNode> _flowchartData = new List<DialogueNode>();
    private PackedScene _dialogueNode;
    private Vector2 _defaultPos = new Vector2(80, 80);
    private readonly Dictionary<DialogueNode, bool> _selectedNodes = new Dictionary<DialogueNode, bool>();
    private GraphEdit _graph;
    
    public override void _Ready()
    {
        _graph = GetNode<GraphEdit>("GraphEdit");
        _dialogueNode = GD.Load<PackedScene>("res://Scripts/Dialogue Tool/Nodes/DialogueNode.tscn");
    }
    
    /// <summary>
    /// This adds a new dialogue node object to the graph
    /// </summary>
    private void _on_add_dialogue_button_pressed()
    {
        DialogueNode node = (DialogueNode)_dialogueNode.Instantiate();
        
        node.Title += _flowchartData.Count;
        node._dialogue.id = _flowchartData.Count;
        node.PositionOffset += _defaultPos;
        
        _graph.AddChild(node);
        _flowchartData.Add(node);
    }

    private void _on_load_file_button_pressed()
    {
        // Creates a new file dialog so that the user can use to choose a compatible json file to load as a flowchart
        FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            RootSubfolder = "res://Assets",
            Filters = ["*.json ; JSON Files"]
        };
        fileDialog.Connect("file_selected", new Callable(this, nameof(_on_load_file_dialog_file_selected)));
        
        GetTree().Root.AddChild(fileDialog);
        fileDialog.PopupCentered();
    }

    private void _on_load_file_dialog_file_selected(string path)
    {
        if (FileAccess.FileExists(path))
        {
            GD.Print($"Loading file: {path}");
            
            ClearAll();
            
            // This takes the file path and deserializes the json into a format that's readable by the flowchart
            string fullPath = File.ReadAllText(ProjectSettings.GlobalizePath(path));
            Conversation _json = JsonSerializer.Deserialize<Conversation>(fullPath);

            // Loops through each item in the conversation list to create new versions of the Dialogue Node objects with the same variables
            foreach (Dialogue dialogue in _json.conversation)
            {
                // Instantiates a new node, and loads the appropriate data
                DialogueNode node = (DialogueNode)_dialogueNode.Instantiate();
                node.LoadData(dialogue);
    
                // Resets the node back to hold its previous data
                node.Title += node._dialogue.id;
                node.PositionOffset += new Vector2(node._dialogue.offsetX, node._dialogue.offsetY);
                
                _flowchartData.Add(node);
                _graph.AddChild(node);
            }
            
            // Separate foreach, as it must happen AFTER each node has been initialized
            // This reconnects each of the nodes back together
            foreach (DialogueNode node in _flowchartData)
            {
                if (node._dialogue.choices.Count > 0)
                {
                    for (int i = 0; i < node._dialogue.choices.Count; i++)
                    {
                        int connectsValue = node._dialogue.choices[i].connectsTo;
                        
                        if (connectsValue >= 0 && connectsValue < _flowchartData.Count &&
                            connectsValue != node._dialogue.id)
                        {
                            _on_graph_edit_connection_request(node.Name, i, _flowchartData[connectsValue].Name, 0);
                        }
                    }
                }
                else
                {
                    int connectsValue = node._dialogue.connectsTo;

                    if (connectsValue >= 0 && connectsValue < _flowchartData.Count && connectsValue != node._dialogue.id)
                    {
                        _on_graph_edit_connection_request(node.Name, 0, _flowchartData[connectsValue].Name, 0);
                    }
                }
            }
        }
        else
        {
            GD.Print($"File not found, failed to load: {path}");
        }
    }

    /// <summary>
    /// This creates and opens a new file dialog that will save the file as a json
    /// </summary>
    private void _on_save_file_button_pressed()
    {
        // Stores the necessary node data so it can be deserialized back into the same flowchart layout
        foreach (DialogueNode node in _flowchartData)
        {
            node._dialogue.offsetX = node.PositionOffset.X;
            node._dialogue.offsetY = node.PositionOffset.Y;
            node._dialogue.width = node.Size.X;
            node._dialogue.height = node.Size.Y;
        }
        
        // Creates a new file dialog so that the user can use to choose a save directory
        FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            RootSubfolder = "res://Assets",
            Filters = ["*.json ; JSON Files"]
        };
        fileDialog.Connect("file_selected", new Callable(this, nameof(_on_save_file_dialog_file_selected)));
        
        GetTree().Root.AddChild(fileDialog);
        fileDialog.PopupCentered();
    }
    
    /// <summary>
    /// This handles the saving of the flowcharts data by serializing it into json
    /// </summary>
    /// <param name="path"></param>
    private void _on_save_file_dialog_file_selected(string path)
    {
        // Creates a conversation variable so that the json file will always be in the same format, so it can always be read by the dialogue manager
        Conversation _json = new Conversation();
        foreach (DialogueNode node in _flowchartData)
        {
            _json.conversation.Add(node._dialogue);
        }

        // This serializes the json into a string, and also configures settings to allow for the list and for it to be formatted to be more readable 
        string jsonString = JsonSerializer.Serialize(_json, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });

        // Opens the file path and then rights to the file, creating a new json
        var saveFile = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (FileAccess.FileExists(path))
        {
            saveFile.StoreLine(jsonString);
            saveFile.Close();
            GD.Print($"Saved file to: {path}");
        }
        else
        {
            GD.PrintErr($"Failed to save file to: {path}");
        }
    }
    
    private void _on_clear_all_button_pressed()
    {
        ClearAll();
    }

    /// <summary>
    /// This handles wiping all the charts data
    /// </summary>
    private void ClearAll()
    {
        _flowchartData.Clear();
        foreach (var child in _graph.GetChildren())
        {
            if (child is DialogueNode)
            {
                _DeleteNode((DialogueNode)child);
                _selectedNodes.Clear();
            }
        }
    }

    /// <summary>
    /// This handles the deletion of the dialogue nodes, as well as removing them from any lists
    /// </summary>
    /// <param name="node"></param>
    private void _DeleteNode(DialogueNode node)
    {
        node.GetParent().RemoveChild(node);
        node.QueueFree();
        _flowchartData.Remove(node);
    }

    /// <summary>
    /// This handles the logic for when nodes are connected to each other in the graph, such as determining the read order
    /// </summary>
    /// <param name="from"></param>
    /// <param name="fromSlot"></param>
    /// <param name="to"></param>
    /// <param name="toSlot"></param>
    private void _on_graph_edit_connection_request(StringName from, int fromSlot, StringName to, int toSlot)
    {
        GetNode<GraphEdit>("GraphEdit").ConnectNode(from, fromSlot, to, toSlot);
        DialogueNode fromNode = GetNode<DialogueNode>("GraphEdit/" + from);
        DialogueNode toNode = GetNode<DialogueNode>("GraphEdit/" + to);
        
        GD.Print("Connecting to " + toSlot);
        GD.Print("Connecting from " + fromSlot);

        // If there are no choices, change the standard connects to value
        if (!fromNode._hasChoices)
        {
            fromNode._dialogue.connectsTo = toNode._dialogue.id;
        }
        // If there are choices, then change to the choice connects to values
        else if (fromNode._hasChoices)
        {
            fromNode._dialogue.choices[fromSlot].connectsTo = toNode._dialogue.id;
        }
    }
    
    /// <summary>
    /// This handles the logic for when nodes are disconnected from each other, must be dragged from the right slot
    /// </summary>
    /// <param name="from"></param>
    /// <param name="fromSlot"></param>
    /// <param name="to"></param>
    /// <param name="toSlot"></param>
    private void _on_graph_edit_disconnection_request(StringName from, int fromSlot, StringName to, int toSlot)
    {
        GetNode<GraphEdit>("GraphEdit").DisconnectNode(from, fromSlot, to, toSlot);
        DialogueNode fromNode = GetNode<DialogueNode>("GraphEdit/" + from);
        DialogueNode toNode = GetNode<DialogueNode>("GraphEdit/" + to);

        // If there are no choices, change the standard connects to value
        if (!fromNode._hasChoices)
        {
            fromNode._dialogue.connectsTo = -1;
        }
        // If there are choices, then change to the choice connects to values
        else if (fromNode._hasChoices)
        {
            fromNode._dialogue.choices[fromSlot].connectsTo = -1;
        }
    }
    
    /// <summary>
    /// Handles the logic for when a node is selected
    /// </summary>
    /// <param name="node"></param>
    private void _on_graph_edit_node_selected(DialogueNode node)
    {
        _selectedNodes[node] = true;
    }
    
    /// <summary>
    /// Handles the logic for when a node is deselected
    /// </summary>
    /// <param name="node"></param>
    private void _on_graph_edit_node_deselected(DialogueNode node)
    {
        _selectedNodes[node] = false;
    }

    /// <summary>
    /// Handles the logic for when the 'Delete' key is pressed, deleting all the nodes that are selected
    /// </summary>
    /// <param name="nodes"></param>
    private void _on_graph_edit_delete_nodes_request(StringName[] nodes)
    {
        foreach (DialogueNode selected in _selectedNodes.Keys)
        {
            if (_selectedNodes[selected])
            { 
                // This goes through each item in the list and alters their IDs to account of the missing number
                foreach (DialogueNode node in _flowchartData)
                {
                    if (node._dialogue.id > selected._dialogue.id)
                    {
                        node._dialogue.id--;
                        node.Title = "DIALOGUE_NODE_" + node._dialogue.id;

                        if (node._dialogue.connectsTo != -1)
                        {
                            node._dialogue.connectsTo--;
                        }
                    }
                }
                _DeleteNode(selected);
            }
        }
        _selectedNodes.Clear();
    }
}
