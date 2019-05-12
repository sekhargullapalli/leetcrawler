using System;

namespace LeetCrawler.Core
{
    internal static class ProjectExtensions
    {
        //Extenson to remove a repeating string at the begining of the string
        internal static string TrimBegining(this string targetString, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return targetString;
            string modifiedString = targetString;
            while (modifiedString.StartsWith(trimString))
            {
                modifiedString = modifiedString.Substring(trimString.Length);
            }
            return modifiedString;
        }
        //Extension to empty a directory
        internal static void Empty(this System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
