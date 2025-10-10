using ARStickyNotes.Services;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ARStickyNotes.UI
{
    public class ARStickyNotesController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Menu button in the UI.
        /// </summary>
        [SerializeField]
        private Button MenuButton;

        /// <summary>
        /// Reference to the Menu options panel in the UI.
        /// </summary>
        [SerializeField]
        private GameObject MenuOptionsPanel;

        /// <summary>
        /// Reference to the Whiteboard button in the UI.
        /// </summary>
        [SerializeField]
        private Button WhiteboardButton;

        /// <summary>
        /// Reference to the Whiteboard prefab to be instantiated.
        /// </summary>
        [SerializeField]
        private GameObject Whiteboard;

        /// <summary>
        /// Reference to the NoteManager for managing notes.
        /// </summary>
        [SerializeField]
        private NoteManager NoteMan;

        /// <summary>
        /// Called when the script instance is being loaded.        /// 
        /// </summary>
        void OnEnable()
        {
            try
            {
                BindElements();
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while binding menu elements.", ex);
            }
        }

        /// <summary>
        /// Binds UI elements to their respective event handlers.
        /// </summary>
        void BindElements()
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

        /// <summary>
        /// Toggles the visibility of the menu options panel.
        /// </summary>
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

        /// <summary>
        /// Displays the whiteboard and loads notes onto it.
        /// </summary>
        void ShowWhiteboard()
        {
            try
            {
                MenuOptionsPanel.SetActive(false);
                if (Whiteboard == null)
                {
                    throw new System.Exception("Whiteboard reference is missing.");
                }
                var item = Instantiate(Whiteboard, transform);
                item = new ARSpawner().SpawnGameObject(item);
                //item.GetComponent<NoteboardController>().LoadNotes(NoteMan.GetNotes());
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while spawning the whiteboard.", ex);
            }
        }
    }
}
