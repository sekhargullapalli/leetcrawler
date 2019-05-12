using System;
using static System.Console;
using System.IO;
using LeetCrawler.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeetCrawler.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = @"https://tretton37.com/";
            Crawler crawler = new Crawler(url);            
            crawler.Crawl();
            ReadLine();
        }
    }
}
