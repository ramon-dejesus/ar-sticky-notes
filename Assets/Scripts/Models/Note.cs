using System;

[Serializable]
public class Note : Preview
{
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    public string Position { get; set; } = "";
    public string Rotation { get; set; } = "";
    public bool IsPinned { get; set; } = false;
    public string NoteType { get; set; } = "";
}
