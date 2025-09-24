# Just-Another-Dialogue-Editor

'Just Another Dialogue Editor', or 'JADE', is an open-source custom dialogue creation tool using Godot 4.0+, but it can be used for any dialogue system that uses JSON deserialisation. It works by allowing for the creation or loading of 'Conversations' that contain many different smaller sections of the conversations, with the reading order being decided by both the node's ID and the nodes it's connected to, and then taking this 'Conversation' and serialising it into a JSON file for it to be parsed by a dialogue reader. This project includes a simple dialogue reader built in so that you can test your conversations dynamically.

I plan to update this whenever I need additional features for my own projects, but since it's open-source, feel free to edit it for your own use. :)

## How to Use;
First off, press start to open up a blank conversation, and then add dialogue nodes by either right-clicking on the graph or by pressing the '+ Dialogue' at the top left.

The order the conversation is read in is determined first by the node with the lowest ID (this will always be 0) and then by whichever node it's tethered to. To tether a node to another, left-click on the read sockets on the right of the dialogue you want to start from, and then drag it to the left socket of the node you want to be next.

### Each dialogue node has the following:
* Character Name, which is self-explanatory.
*A text box that is where the actual dialogue for that part of the conversation goes.
*A text speed box which determines how fast each letter of the conversation is read (this is for the typewriter effect), with 0 being instant and 1 being slow.
*An add portrait button, which will allow you to select an image within the project's files that will be used to represent the character for this part of the conversation. **IMPORTANT NOTE: Any pictures you intend to use should be in the same file structure as the one set up in this project; otherwise, it will produce an error.**
*A '+ Choice' button, which adds a choice that will be shown after the dialogue of this section is read. This choice will allow for divergent paths in the conversation, which will be determined by whichever dialogue nodes the choices are tethered to. Any number of choices can be added.

The final few buttons at the top of the screen include a 'Save' button which simply serialises the conversation data into a .json format so that it can be deserialised in a separate dialogue user interface; then there's 'Load', which recreates the flowchart of any compatible .json files; a 'Clear All' button, which wipes all the dialogue nodes in the chart; and finally a red 'X' button in the top right corner, which exits the graph.

The last function is on the main menu, where, by pressing the 'Test' button, you can load any compatible .json files created using this tool to view them in an example dialogue user interface.
