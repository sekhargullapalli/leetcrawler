﻿using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeetCrawler
{
    /// <summary>
    /// Reads the content of the url and extracts the links
    /// </summary>
    internal class UrlReader
    {
        internal UrlReader(string url) =>
            this.Url = url.Trim().TrimEnd(new char[] {'/'});

        internal string Url { get; set; } = "";
        /// <summary>
        /// html content of the route
        /// </summary>
        internal string Content { get; set; } = string.Empty;

        /// <summary>
        /// Link exctration from the url using regular expressions
        /// </summary>
        /// <returns></returns>
        internal async Task<HashSet<string>> ExtractLinks()
        {
            HashSet<string> Links = new HashSet<string>();
            string content = await ReadURLContent(Url);
            Content = content;
            MatchCollection matches = Regex.Matches(content, @"(href|src)=\""(.*?)\""");
            foreach (Match m in matches)
            {
                if (string.IsNullOrEmpty(m.Value.Trim())) continue;
                string link = m.Value.Trim().ToLower();
                link = ProcessLink(link);
                if (!string.IsNullOrEmpty(link))
                    Links.Add(link);
            }
            return Links;
        }
        

        #region private memebers

        private async Task<string> ReadURLContent(string url)
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync(url).Result;
            string responseString = string.Empty;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;                
                responseString = await responseContent.ReadAsStringAsync();             
            }
            //Explicit handling of 404 since the status returned for not found route is 200 OK
            if (responseString.Contains("<title>404")) responseString = string.Empty;
            return responseString;
        }       
        private string ProcessLink(string link)
        {
            //Remove attribute names
            link = link.TrimBegining("href");
            link = link.TrimBegining("src");
            link = link.Trim(new char[] { '=', '"', ' ' });
            link = link.TrimBegining("../");
            link = link.Trim(new char[] { '/' });
            //Remove external links and mail addresses
            link = (link.StartsWith("http")
                || link.StartsWith("www")
                || link.StartsWith("tel")
                || link.StartsWith("cdn")
                || link.StartsWith("javascript")
                || link.StartsWith("mailto")
                || link.StartsWith(@"//cdn.")) ? string.Empty : link;
            //Remove dynamic links
            if (link.Contains("/{")) link = string.Empty;
            //Remove window bookmarks from link
            if (link.Contains("#"))
                link = link.Substring(0, link.IndexOf("#"));
            //Remove caching parameters in css and js links
            if (link.Contains("css?") || link.Contains("js?"))
                link = link.Substring(0, link.LastIndexOf("?"));
            link = link.Trim(new char[] { '/' });
            //Replace replative paths
            link = link.Replace("../", "").Trim();
            return link;
        }

        #endregion #region private memebers
    }
}
