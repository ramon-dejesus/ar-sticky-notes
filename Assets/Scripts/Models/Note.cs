using System;

[Serializable]
public class Note : BaseObject
{    
    public string Position { get; set; } = "";
    public string Rotation { get; set; } = "";
    public bool IsPinned { get; set; } = false;
    public string NoteType { get; set; } = "";
}
