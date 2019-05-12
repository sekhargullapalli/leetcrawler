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
        //Print string as status on same line in console
        internal static void ShowasStatus(this string s, int length)
        {
            s = s.PadRight(length).Substring(0, length);
            Console.Write($"\r{s}");
        }
        //Print in different colors in console
        internal static void WriteLine (this string s, ConsoleColor col)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.WriteLine(s);
            Console.ForegroundColor = original;
        }
        //Extension to empty a directory
        internal static void Empty(this System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }





        internal static string Banner = @"
.__                 __                               .__                
|  |   ____   _____/  |_    ________________ __  _  _|  |   ___________ 
|  | _/ __ \_/ __ \   __\ _/ ___\_  __ \__  \\ \/ \/ /  | _/ __ \_  __ \
|  |_\  ___/\  ___/|  |   \  \___|  | \// __ \\     /|  |_\  ___/|  | \/
|____/\___  >\___  >__|    \___  >__|  (____  /\/\_/ |____/\___  >__|   
          \/     \/            \/           \/                 \/                   

            ";
    }
}
