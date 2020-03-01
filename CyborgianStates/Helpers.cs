using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CyborgianStates
{
    public static class Helpers
    {
        /// <summary>
        /// Converts nation/region name to format that can be used on api calls
        /// </summary>
        /// <param name="text">The text to ensure format on</param>
        /// <returns>Formated string</returns>
        public static string ToID(string text)
        {
            return text?.Trim().ToLower(CultureInfo.InvariantCulture).Replace(' ', '_').Trim('@');
        }

        /// <summary>
        /// An API Id back to nation/region name
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Formated string convert back to name</returns>
        public static string FromID(string text)
        {
            return text?.Trim().ToLower(CultureInfo.InvariantCulture).Replace('_', ' ');
        }
    }
}
