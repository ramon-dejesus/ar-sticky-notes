using System;
using ARStickyNotes.Utilities;
using UnityEngine;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// A base class for objects that require identification, metadata, and timestamp tracking.
    /// Includes JSON-serializable fields compatible with Unity's <c>JsonUtility</c>.
    /// </summary>
    [Serializable]
    public class BaseObject
    {
        /// <summary>
        /// A unique identifier (GUID) for the object, generated at runtime.
        /// </summary>
        [SerializeField] public string Id = Guid.NewGuid().ToString("N");
        
        /// <summary>
        /// The internally stored creation timestamp in string format (ISO 8601), 
        /// required due to Unity's <c>JsonUtility</c> limitations with <c>DateTime</c>.
        /// </summary>
        [SerializeField] private string _createdAt = new UnityConverter().ConvertDateTimeToString(DateTime.Now);
        
        /// <summary>
        /// The creation date and time of the object.
        /// This property wraps the string field <c>_createdAt</c> to provide <c>DateTime</c> support
        /// while remaining serializable with Unity's <c>JsonUtility</c>.
        /// </summary>
        public DateTime CreatedAt
        {
            //Datetime objects cannot be converted to json with JsonUtility. This is a workaround.
            get
            {
                return new UnityConverter().ConvertStringToDateTime(_createdAt);
            }
            set
            {
                _createdAt = new UnityConverter().ConvertDateTimeToString(value);
            }
        }

        /// <summary>
        /// The title of the object. Can be used for UI display, sorting, or searching.
        /// </summary>
        [SerializeField] public string Title = "";

        /// <summary>
        /// A longer description or body text for the object.
        /// </summary>
        [SerializeField] public string Description = "";
    }
}