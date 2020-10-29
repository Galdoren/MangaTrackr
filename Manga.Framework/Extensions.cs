using System;
using System.Text.RegularExpressions;

namespace Manga.Framework
{
    public static class Extensions
    {
        /// <summary>
        /// Removes the extra spaces
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveExtaSpaces(this string text)
        {

            Regex regex = new Regex(@"\s{2,}", RegexOptions.IgnorePatternWhitespace |
                RegexOptions.Singleline);
            text = regex.Replace(text.Trim(), " "); //This line removes extra spaces and make space exactly one.
            //To remove the  space between the end of a word and a punctuation mark used in the text we will
            //be using following line of code
            regex = new Regex(@"\s(\!|\.|\?|\;|\,|\:)"); // “\s” whill check for space near all puntuation marks in side ( \!|\.|\?|\;|\,|\:)”); )
            text = regex.Replace(text, "$1");
            return text;
        }

        /// <summary>
        /// Removes the illegal characters from the string which retains a file to be created with this name
        /// </summary>
        /// <param name="text">String to modify</param>
        /// <returns>Modified String which is valid for being a file name</returns>
        public static string RemoveIllegalCharacters(this string text)
        {
            string illegalCharacters = new string(System.IO.Path.GetInvalidPathChars()) + new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(illegalCharacters)));
            text = regex.Replace(text, " ");
            return text;
        }
        /// <summary>
        /// Removes the illegal characters for sqlite database
        /// </summary>
        /// <param name="text">String to modify</param>
        /// <returns>Modified String which is valid for being an entry</returns>
        public static string SQLIllagalCharacters(this string text)
        {
            Regex regex = new Regex(string.Format("[{0}]", Regex.Escape("'")));
            text = regex.Replace(text, " ");
            return text;
        }

        public static bool Contains(this string source, string toCheck, System.StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool FindKeyByValue<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> dictionary, TValue value, out TKey key)
        {
            if (dictionary == null)
                throw new System.ArgumentNullException("dictionary");

            foreach (System.Collections.Generic.KeyValuePair<TKey, TValue> pair in dictionary)
                if (value.Equals(pair.Value))
                {
                    key = pair.Key;
                    return true;
                }

            key = default(TKey);
            return false;
        }
    }
}
