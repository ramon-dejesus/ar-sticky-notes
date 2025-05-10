using System;
using System.Data;
using System.Linq;
using UnityEngine;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;

namespace ARStickyNotes.Services
{
    public class NoteManager : MonoBehaviour
    {
        private NoteList Notes { get; set; } = null;
        private LocalStorage Storage { get; set; } = null;
        public void Test()
        {
            try
            {
                Debug.Log(PreloadNotes());
                Debug.Log(PreloadNotes(true));
            }
            catch (Exception ex)
            {
                GetExceptionTrace(ex);
            }
        }
        public NoteManager() { }

        void Start()
        {
            try
            {
                Storage = new LocalStorage(Application.persistentDataPath, new UnityEncryption().GetUniquePassword());
                GetNotes();
            }
            catch (Exception ex)
            {
                GetExceptionTrace(ex);
            }
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
        private string PreloadNotes(bool forcedDelete = false)
        {
            if (forcedDelete)
            {
                DeleteAllNotes();
            }
            if (Notes.Items.Count == 0)
            {

                for (var x = 0; x < 5; x++)
                {
                    var item = GetNewNote();
                    item.Title = GetRandomString(10);
                    item.Description = GetRandomString(100);
                    UpdateNote(item);
                }
            }
            Notes = null;
            GetNotes();
            return new UnityConverter().ConvertObjectToJson(Notes);
        }
        private string GetNotesFilename()
        {
            var env = new UnityEncryption();
            return new UnityConverter().ConvertStringToBase64(env.Encrypt(env.GetUniquePassword(), "47F8F007168A4DB9834E0746922695A4"));
        }
        public NoteList GetNotes()
        {
            try
            {
                if (Notes == null)
                {
                    Notes = Storage.GetObject<NoteList>(GetNotesFilename());
                    Notes ??= new NoteList();
                }
                return Notes;
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        private void SaveNotes()
        {
            Storage.SaveObject(GetNotesFilename(), Notes);
        }
        public Note GetNoteById(string id)
        {
            try
            {
                return Notes.Items.Find(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        public void DeleteNote(string id)
        {
            try
            {
                var item = Notes.Items.Find(x => x.Id == id);
                if (item != null)
                {
                    Notes.Items.Remove(item);
                }
                SaveNotes();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        public void DeleteAllNotes()
        {
            try
            {
                Notes.Items.Clear();
                SaveNotes();
                Notes = new NoteList();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        public Note GetNewNote()
        {
            try
            {
                return new Note();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        public void UpdateNote(Note item)
        {
            try
            {
                var tmp = Notes.Items.Find(x => x.Id == item.Id);
                if (tmp != null)
                {
                    Notes.Items.Remove(tmp);
                }
                Notes.Items.Add(item);
                SaveNotes();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        private Exception GetExceptionTrace(Exception ex)
        {
            Debug.LogError(ex.ToString());
            return new Exception(ex.Message);
        }
    }
}