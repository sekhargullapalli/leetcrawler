using System;

namespace LeetCrawler.Core
{
    public static class ProjectExtensions
    {
        public static string TrimBegining(this string targetString, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return targetString;
            string modifiedString = targetString;
            while (modifiedString.StartsWith(trimString))
            {
                modifiedString = modifiedString.Substring(trimString.Length);
            }
            return modifiedString;
        }
    }
}
