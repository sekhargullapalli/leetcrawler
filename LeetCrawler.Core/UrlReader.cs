using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LeetCrawler.Core
{
    internal class UrlReader
    {
        internal UrlReader(string url) =>
            this.Url = url.Trim().TrimEnd(new char[] {'/'});

        internal string Url { get; set; } = "";
        internal string Content { get; set; } = string.Empty;

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

        private async Task<string> ReadURLContent(string url)
        {
            HttpClient client = new HttpClient();
            byte[] urldata = await client.GetByteArrayAsync(url);
            return Encoding.Default.GetString(urldata);
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
            //Remove window location from link
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
    }
}
