using System;
using System.Linq;

namespace ARStickyNotes.Utilities
{
    public class TextUtility
    {
        /// <summary>
        /// This class is responsible for text-related utility functions.
        /// </summary>
        public TextUtility() { }

        /// <summary>
        /// Generates a random alphanumeric string.
        /// </summary>
        /// <param name="length">The desired length of the string.</param>
        /// <returns>A randomly generated string.</returns>
        public string GetRandomString(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException("Length must be greater than 0", nameof(length));
            }

            var random = new System.Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
