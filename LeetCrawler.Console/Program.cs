using System;
using static System.Console;
using LeetCrawler.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeetCrawler.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string url = @"https://tretton37.com/";
            Crawler crawler = new Crawler();
            HashSet<string> links = await crawler.ExtractLinks(url);
            foreach(string link in links)
                System.Console.WriteLine(link);

            ReadLine();
        }
    }
}
