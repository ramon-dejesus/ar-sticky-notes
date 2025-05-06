using System;
using UnityEngine;

[Serializable]
public class Note : BaseObject
{
    [SerializeField] public string Position = "";
    [SerializeField] public string Rotation = "";
    [SerializeField] public bool IsPinned = false;
    [SerializeField] public string NoteType = "";
    public Note() { }
}
