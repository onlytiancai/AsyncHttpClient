using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace AsynHttpClient
{
    internal class UrlGenerator
    {
        private static HashSet<string> _urlThisPageLinks = new HashSet<string>();
        private static HashSet<string> _urlNextLevelPageLinks = new HashSet<string>();
        private static Regex _urlReg = new Regex(@"http://(\w+\.)?\w+\.\w+");

        public static IEnumerable<string> GetLinksFromUrl(string url = "http://www.hao123.com")
        {
            _urlNextLevelPageLinks.Add(url);
            string result = GetResponseFromUrl(url);
            if (result == null) return null;
            return GetAllLinksFromPage(result);

        }

        private static IEnumerable<string> GetAllLinksFromPage(string result)
        {
            MatchCollection colls = _urlReg.Matches(result);
            IEnumerable<string> thisPageLinks = GetThisPageLinks(colls);
            IEnumerable<string> nextLevelPageLinks = GetNextLevelPageLinks(colls);
            IEnumerable<string> allLinks = thisPageLinks.Union(nextLevelPageLinks);
            return allLinks;
        }

        private static IEnumerable<string> GetThisPageLinks(MatchCollection colls)
        {
            foreach (Match match in colls)
            {
                string link = match.Groups[0].Value;
                if (_urlThisPageLinks.Contains(link))
                    continue;
                _urlThisPageLinks.Add(link);
                yield return link;
            }
        }

        private static IEnumerable<string> GetNextLevelPageLinks(MatchCollection colls)
        {
            foreach (Match match in colls)
            {
                string link = match.Groups[0].Value;
                if (_urlNextLevelPageLinks.Contains(link))
                    continue;
                _urlThisPageLinks.Add(link);

                var links = GetLinksFromUrl(link);
                if (links == null) continue;

                foreach (string eachLink in links)
                {
                    yield return eachLink;
                }
            }
        }
        private static string GetResponseFromUrl(string url)
        {
            WebClient client = new WebClient();
            string result = null;
            try
            {
                result = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLinksFromUrl error:{0}", ex.Message);
            }
            return result;
        }
    }
}
