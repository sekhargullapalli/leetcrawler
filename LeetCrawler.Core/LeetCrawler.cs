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
    public class Crawler
    {

        public async Task<byte[]> ReadURLData(string url)
        {
            HttpClient client = new HttpClient();
            byte[] urldata = await client.GetByteArrayAsync(url);
            return urldata;
        }
        public async Task<string> ReadURLContent(string url)
        {
            HttpClient client = new HttpClient();
            byte[] urldata = await client.GetByteArrayAsync(url);
            return Encoding.Default.GetString(urldata);
        }
        
        public async Task<HashSet<string>> ExtractLinks (string url)
        {
            string content = await ReadURLContent(url);
            HashSet<string> links = new HashSet<string>();
            MatchCollection matches =  Regex.Matches(content, @"(href|src)=\""(.*?)\"""); 
            foreach(Match m in matches)
            {
                if (string.IsNullOrEmpty(m.Value.Trim())) continue;
                string link = m.Value.Trim().ToLower();
                link = ProcessLink(link);
                if (!string.IsNullOrEmpty(link))
                    links.Add(link);
            }
            return links;
        }
        private string ProcessLink (string link)
        {
            //Remove attribute names
            link=link.TrimBegining("href");
            link=link.TrimBegining("src");            
            link = link.Trim(new char[] { '=', '"',' ' });
            //Remove external links
            link = (link.StartsWith("http") ||link.StartsWith("javascript:void") ||link.StartsWith(@"//cdn.")) ? string.Empty : link;
            //Remove window location from link
            if (link.Contains("#"))
                link = link.Substring(0, link.IndexOf("#"));
            //Remove caching parameters in css and js links
            if (link.Contains("css?")|| link.Contains("js?"))
                link = link.Substring(0, link.LastIndexOf("?"));
            link = link.TrimEnd(new char[] { '/' });
            return link;
        }


    }
}
