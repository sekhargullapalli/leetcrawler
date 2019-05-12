using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Threading.Tasks;

namespace LeetCrawler.Core
{
    public class Crawler
    {
        public Crawler(string url)
        {
            Url = url;
            CreateOutputDirectory();            
        }

        public string Url { get; set; } = "";      
        public string OutputDirectory { get; set; } = "";


        ConcurrentQueue<Task> TaskList = new ConcurrentQueue<Task>();
        ConcurrentDictionary<string, string> Resources = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> Routes = new ConcurrentDictionary<string, string>();

        public void Crawl()
        {
            CreateRouteCrawler(string.Empty); //Index route crawler

            while (TaskList.Count != 0)
            {
                Task task;
                if (TaskList.TryDequeue(out task))
                {
                    task.Wait();
                }
            }
            Console.WriteLine("Resources Identified!");
            Console.WriteLine($"Total Routes: {Routes.Count}");
            Console.WriteLine($"Total Resources: {Resources.Count}");
        }
        
        private void CreateRouteCrawler(string Route)
        {
            Task task = Task.Factory.StartNew(() => {                  
                if (!Routes.TryAdd(CreatePath(Route),string.Empty)) return;
                string url = (Route == string.Empty) ? Url : string.Concat(Url, "/", Route);
                Console.WriteLine($"+Processing Url - {url}");                
                var reader = new UrlReader(url);
                HashSet<string> links = reader.ExtractLinks().GetAwaiter().GetResult();
                Routes.TryUpdate(CreatePath(Route), reader.Content, string.Empty);

                foreach(string link in links)
                {
                    if (link.StartsWith("tretton37img.blob.core.windows.net")) continue;
                    if(Path.GetExtension(link).Trim() != string.Empty)
                    {
                        if (Resources.TryAdd(CreatePath(link), string.Concat(Url, "/", link))) 
                            Console.WriteLine($" - resource: {link}");                        
                    }
                    else
                        CreateRouteCrawler(link);
                }
           });
           TaskList.Enqueue(task);
        }
        private void CreateOutputDirectory()
        {
            OutputDirectory = Path.GetFullPath("crawlerOutput");
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
            new DirectoryInfo(OutputDirectory).Empty();
        }
        private string CreatePath(string link)
        {
            if (link == string.Empty) link = "index.html";
            if (Path.GetExtension(link) == string.Empty)
                link += ".html";
            return Path.Combine(OutputDirectory, link);
        }

        
        




    }
}
