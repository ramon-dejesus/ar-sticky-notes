using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoteList
{
    /*
        Using this class instead of directly using the list in the code 
        because a direct list/array cannot be converted to json with JsonUtility.
    */
    [SerializeField] public List<Note> Items = new List<Note>();
    public NoteList() { }
    public NoteList(List<Note> lst)
    {
        Items = lst;
    }
}
