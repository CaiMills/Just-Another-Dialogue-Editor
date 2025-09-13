using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

[Serializable]
public class Conversation
{
    [JsonPropertyName("_conversation")] public List<Dialogue> conversation { get; set; }  = new List<Dialogue>();
}

[Serializable]
public class Dialogue
{
    [JsonPropertyName("_id")] public int id { get; set; }
    [JsonPropertyName("_name")] public string name { get; set; } = "";
    [JsonPropertyName("_text")] public string text { get; set; } = "";
    [JsonPropertyName("_textSpeed")] public float textSpeed { get; set; } = 1.0f;
    [JsonPropertyName("_portraitPath")] public string portraitPath { get; set; }
    [JsonPropertyName("_connectsTo")] public int connectsTo { get; set; } = -1;
    [JsonPropertyName("_choices")] public List<Choice> choices { get; set; } = new List<Choice>();
    [JsonPropertyName("_offsetX")] public float offsetX { get; set; }
    [JsonPropertyName("_offsetY")] public float offsetY { get; set; }
    [JsonPropertyName("_width")] public float width { get; set; }
    [JsonPropertyName("_height")] public float height { get; set; }
}

[Serializable]
public class Choice
{
    [JsonPropertyName("_text")] public string text { get; set; } = "";
    [JsonPropertyName("_connectsTo")] public int connectsTo { get; set; } = -1;
}
