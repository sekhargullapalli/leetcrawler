using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeetCrawler
{
    public class Crawler
    {
        public Crawler(string url)
        {
            Url = url;
            CreateOutputDirectory();            
        }

        public string Url { get; set; } = "";


        string OutputDirectory = string.Empty;
        ConcurrentQueue<Task> TaskList = new ConcurrentQueue<Task>();
        ConcurrentDictionary<string, string> Resources = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> Routes = new ConcurrentDictionary<string, string>();

        public void Crawl()
        {
            ProjectExtensions.Banner.WriteLine(ConsoleColor.DarkCyan);
            "v20190512..\n\n".WriteLine(ConsoleColor.DarkCyan);
            "\nInitiating crawling.. (this could take a short while!!)".WriteLine(ConsoleColor.DarkCyan);

            //Creating Index route crawler
            CreateRouteCrawler(string.Empty); 
            //Identifying resources to download
            while (TaskList.Count != 0)
            {
                Task task;
                if (TaskList.TryDequeue(out task))
                {
                    task.Wait();
                }
            }
            Console.WriteLine("\nDone..");

            int routes = Routes.Count, savedroutes = 0;
            int resources = Resources.Count, savedresources = 0;
            Console.WriteLine($"\n\nTotal routes : {routes}");
            Console.WriteLine($"Total resources : {resources}");

            "\n\nSaving linked routes..".WriteLine(ConsoleColor.DarkCyan);
            Parallel.ForEach(Routes, (route) =>
            {
                try
                {
                    if (route.Value!=string.Empty)
                        File.WriteAllText(route.Key, route.Value);
                    Interlocked.Increment(ref savedroutes);
                }
                catch (Exception){}
                finally
                {
                    Console.Write($"\rHandled {savedroutes} of {routes} routes.. {Path.GetFileName(route.Key).PadRight(50).Substring(0,50)} ");
                }
            });

            "\nDownloading resources..".WriteLine(ConsoleColor.DarkCyan);
            Parallel.ForEach(Resources, (resource) =>
            {
                try
                {
                    HttpClient client = new HttpClient();
                    byte[] urldata = DownloadResource(resource.Value, resource.Key).GetAwaiter().GetResult();
                    if (urldata.Length !=0)
                        File.WriteAllBytes(resource.Key, urldata);
                    Interlocked.Increment(ref savedresources);
                }
                catch (Exception){}
                finally
                {
                    Console.Write($"\rHandled {savedresources} of {resources} resources.. {Path.GetFileName(resource.Key).PadRight(50).Substring(0, 50)} ");
                }
            });
            Console.WriteLine("\n\nDone!");
            TryShowOutputDirectory();
        }        
        private void CreateRouteCrawler(string Route)
        {
            Task task = Task.Factory.StartNew(() => {                  
                if (!Routes.TryAdd(CreatePath(Route),string.Empty)) return;
                string url = (Route == string.Empty) ? Url : string.Concat(Url, "/", Route);
                $"Processing route - {url}".ShowasStatus(100);
                var reader = new UrlReader(url);
                HashSet<string> links = reader.ExtractLinks().GetAwaiter().GetResult();
                Routes.TryUpdate(CreatePath(Route), reader.Content, string.Empty);

                foreach(string link in links)
                {
                    if (link.StartsWith("tretton37img.blob.core.windows.net")) continue;
                    if(Path.GetExtension(link).Trim() != string.Empty)
                    {
                        if (Resources.TryAdd(CreatePath(link), string.Concat(Url, "/", link)))
                            $"Identified resource: {link}".ShowasStatus(100);
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
            string path = OutputDirectory;
            string[] dirs = link.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < dirs.Length - 1; i++)
            {
                path = Path.Combine(path, dirs[i]);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            return Path.Combine(OutputDirectory, link);
        }
        private async Task<byte[]> DownloadResource(string url, string path)
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync(url).Result;
            byte[] resourceData = new byte[0];
            if (response.IsSuccessStatusCode)//text/html
            {
                if (!path.EndsWith("html") && response.Content.Headers.ContentType.MediaType == MediaTypeNames.Text.Html)
                    return resourceData;                
                var responseContent = response.Content;
                resourceData = await responseContent.ReadAsByteArrayAsync();
                if (path.EndsWith("html") && Encoding.Default.GetString(resourceData).Contains("<title>404"))
                    return new byte[0];                
            }
            return resourceData;
        }
        private void TryShowOutputDirectory()
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", OutputDirectory);
            }
            catch (Exception){}
        }
    }
}
