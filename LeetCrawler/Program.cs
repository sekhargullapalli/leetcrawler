using static System.Console;

namespace LeetCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = @"https://tretton37.com/";
            new Crawler(url).Crawl();
            ReadLine();
        }
    }
}
