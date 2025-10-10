using System.Collections.Generic;
using ARStickyNotes.Services;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace ARStickyNotes.UI
{

    public class ARStickyNotesController : MonoBehaviour
    {
        [SerializeField]
        private Button MenuButton;

        [SerializeField]
        private GameObject MenuOptionsPanel;

        [SerializeField]
        private Button WhiteboardButton;

        [SerializeField]
        private GameObject Whiteboard;

        [SerializeField]
        private NoteManager NoteMan;

        void OnEnable()
        {
            try
            {
                if (MenuButton != null)
                {
                    MenuButton.onClick.AddListener(ShowMenu);
                }
                else
                {
                    throw new System.Exception("MenuButton reference is missing in MenuManager.");
                }
                if (WhiteboardButton != null)
                {
                    WhiteboardButton.onClick.AddListener(ShowWhiteboard);
                }
                else
                {
                    throw new System.Exception("WhiteboardButton reference is missing in MenuManager.");
                }
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while binding menu elements.", ex);
            }
        }
        void ShowMenu()
        {
            try
            {
                if (MenuOptionsPanel == null)
                {
                    throw new System.Exception("MenuModal reference is missing.");
                }
                MenuOptionsPanel.SetActive(!MenuOptionsPanel.activeSelf);
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while binding menu elements.", ex);
            }
        }
        void ShowWhiteboard()
        {
            try
            {
                if (Whiteboard == null)
                {
                    throw new System.Exception("Whiteboard reference is missing.");
                }
                MenuOptionsPanel.SetActive(false);
                var tmp = Instantiate(Whiteboard, transform);
                var item = new ARSpawner().SpawnGameObject(tmp);
                //item.GetComponent<NoteboardController>().LoadNotes(NoteMan.GetNotes());
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                //ErrorReporter.Report("An error occurred while spawning the whiteboard.", ex);
            }
        }
    }
}
