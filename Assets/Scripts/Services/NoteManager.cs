using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public sealed class NoteManager : MonoBehaviour
{
    private List<Note> Notes { get; set; } = null;
    private NoteManager()
    {
        GetNotes();
    }
    private static readonly Lazy<NoteManager> lazy = new Lazy<NoteManager>(() => new NoteManager());
    public static NoteManager Main
    {
        get { return lazy.Value; }
    }

    void Start()
    {

    }
    void Update()
    {

    }

    private string GetRandomString(int length)
    {
        var random = new System.Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public int PreloadNotes()
    {
        DeleteAllNotes();
        for (var x = 0; x < 100; x++)
        {
            var item = GetNewNote();
            item.Title = GetRandomString(10);
            item.Description = GetRandomString(100);
            UpdateNote(item);
        }
        return Notes.Count;
    }
    public List<Note> GetNotes()
    {
        if (Notes == null)
        {
            Notes = new LocalStorage().GetObject<List<Note>>("Notes");
            Notes ??= new List<Note>();
        }
        return Notes;
    }
    private void SaveNotes()
    {
        new LocalStorage().SaveObject("Notes", Notes);
    }
    public Note GetNoteById(string id)
    {
        return Notes.Find(x => x.Id == id);
    }
    public void DeleteNote(string id)
    {
        var item = Notes.Find(x => x.Id == id);
        if (item != null)
        {
            Notes.Remove(item);
        }
        SaveNotes();
    }
    public void DeleteAllNotes()
    {
        Notes.Clear();
        SaveNotes();
    }
    public Note GetNewNote()
    {
        return new Note();
    }
    public void UpdateNote(Note item)
    {
        var tmp = Notes.Find(x => x.Id == item.Id);
        if (tmp != null)
        {
            Notes.Remove(tmp);
        }
        Notes.Add(item);
        SaveNotes();
    }
}
