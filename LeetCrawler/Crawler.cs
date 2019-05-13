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
            //Output directory is created in application startup
            CreateOutputDirectory();
        }

        public string Url { get; set; } = "";

        
        string OutputDirectory = string.Empty; 

        //Concurrent collections
        //Task list to hold the recursive tasks of crawling identified routes
        ConcurrentQueue<Task> TaskList = new ConcurrentQueue<Task>();

        //Dictionary to hold the resource path (on disk) and its url. The resource can be an image, style sheet, script file or a blob block
        ConcurrentDictionary<string, string> Resources = new ConcurrentDictionary<string, string>();

        //Dictionary to hold the path to save (on disk) and the html content.
        ConcurrentDictionary<string, string> Routes = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// asyncronous crawler for tretton37.com
        /// first the routes and resources are identified and then they are downloaded/ saved
        /// </summary>
        public void Crawl()
        {
            Console.Clear();
            ProjectExtensions.Banner.WriteLine(ConsoleColor.DarkCyan);
            "v20190512..\n\n".WriteLine(ConsoleColor.DarkCyan);
            "\nInitiating crawling.. (this could take a few seconds!!)".WriteLine(ConsoleColor.DarkCyan);

            //Creating Index route crawler
            CreateRouteCrawler(string.Empty); 
            //Picking crawling tasks from the concurrent queue
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

            //Saving the content of routes as html files
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

            //Try download and save all resources
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


        #region private memebers
        //The recursive asyncronous crawler for a route
        private void CreateRouteCrawler(string Route)
        {
            Task task = Task.Factory.StartNew(() => {

                if (!Routes.TryAdd(CreatePath(Route),string.Empty)) return; //Route already crawled
                string url = (Route == string.Empty) ? Url : string.Concat(Url, "/", Route);
                $"Processing route - {url}".ShowasStatus(100);

                var reader = new UrlReader(url);
                HashSet<string> links = reader.ExtractLinks().GetAwaiter().GetResult();

                Routes.TryUpdate(CreatePath(Route), reader.Content, string.Empty);

                foreach(string link in links)
                {
                    if (link.StartsWith("tretton37img.blob.core.windows.net")) //block blob link
                    {
                        string savePath = link.Replace("tretton37img.blob.core.windows.net",string.Empty)+".png";                        
                        if (Resources.TryAdd(CreatePath(savePath), "http://"+link))
                            $"Identified resource: {link}".ShowasStatus(100);
                    }
                    else if(Path.GetExtension(link).Trim() != string.Empty) //other resources such as images, style sheets etc
                    {
                        if (Resources.TryAdd(CreatePath(link), string.Concat(Url, "/", link)))
                            $"Identified resource: {link}".ShowasStatus(100);
                    }
                    else
                        CreateRouteCrawler(link); //recursive call for another route
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
        
        //Create path takes the link from crawler and creates path to save on disk
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
            return Path.Combine(path, dirs[dirs.Length-1]);
        }

        //reurns the content of a url as byte[], byte[0] if 404
        private async Task<byte[]> DownloadResource(string url, string path)
        {            
            HttpClient client = new HttpClient();
            var response = client.GetAsync(url).Result;
            byte[] resourceData = new byte[0];
            if (response.IsSuccessStatusCode)
            {
                if (!path.EndsWith("html") && response.Content.Headers.ContentType.MediaType == MediaTypeNames.Text.Html)
                    return resourceData;                
                var responseContent = response.Content;
                resourceData = await responseContent.ReadAsByteArrayAsync();
                //Explicit handling of 404 since the status returned for not found route is 200 OK
                if (path.EndsWith("html") && Encoding.Default.GetString(resourceData).Contains("<title>404"))
                    return new byte[0];                
            }
            return resourceData;
        }
        
        //On windows opens the output directory in explorer
        private void TryShowOutputDirectory()
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", OutputDirectory);
            }
            catch (Exception){}
        }

        #endregion private memebers

    }
}
