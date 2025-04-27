using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            //print(PreloadNotes());
        }
        catch (Exception ex)
        {
            //print(ex.ToString());
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private string GetRandomString(int length)
    {
        var random = new System.Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    private string PreloadNotes()
    {
        var ids = GetIds();
        if (ids.Count == 0)
        {
            for (var x = 0; x < 100; x++)
            {
                var item = GetNewNote();
                item.Title = GetRandomString(10);
                item.Description = GetRandomString(100);
                ids.Add(item.Id);
            }
            SaveIds(ids);
        }
        return JsonUtility.ToJson(ids);
    }
    private void SaveIds(List<string> ids)
    {
        new LocalStorage().SaveObject("Ids", ids);
    }
    private void DeleteId(string id)
    {
        var ids = GetIds();
        ids.Remove(id);
        SaveIds(ids);
    }
    private void AddId(string id)
    {
        var ids = GetIds();
        ids.Add(id);
        SaveIds(ids);
    }
    private List<string> GetIds()
    {
        var lst = new LocalStorage().GetObject<List<string>>("Ids");
        return lst;
    }
    public List<Preview> GetAllNotePreviews()
    {
        var items = new List<Preview>();
        foreach (string id in GetIds())
        {
            var item = new LocalStorage().GetObject<Preview>(id);
            if (item != null)
            {
                items.Add(item);
            }
        }
        return items;
    }
    public Note GetNoteById(string id)
    {
        return new LocalStorage().GetObject<Note>(id);
    }
    public void DeleteNote(string id)
    {
        DeleteId(id);
        new LocalStorage().DeleteObject(id);
    }
    public Note GetNewNote()
    {
        return new Note();
    }
    public string UpdateNote(Note item)
    {
        if (item.CreatedAt == DateTime.MinValue)
        {
            AddId(item.Id);
        }
        item.CreatedAt = DateTime.Now;
        new LocalStorage().SaveObject(item.Id, item);
        return "";
    }
}
