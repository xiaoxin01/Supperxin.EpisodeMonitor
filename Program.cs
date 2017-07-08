using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;

namespace Supperxin.EpisodeMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please input username and password!");
                Console.Read();
                return;
            }
            var baseUrl = "https://chdbits.co";
            var loginUrl = "/takelogin.php";
            var username = args[0];
            var password = args[1];
            var resultQueryUrl = "/torrents.php?search=楚乔传&notnewword=1";
            var duration = 30 * 60 * 1000;


            var cookieContainer = LoginAndGetCookie(baseUrl, loginUrl, username, password);
            var originEpisode = string.Empty;
            while (true)
            {
                var html = GetSearchResultHtml(baseUrl, resultQueryUrl, cookieContainer);
                var latestEpisode = AnalyzeHtmlToGetLatestEpisode(html);

                if (originEpisode != latestEpisode)
                {
                    NotifyMe(latestEpisode);
                    originEpisode = latestEpisode;
                }
                else
                {
                    Console.Write(".");
                }

                Thread.Sleep(duration);
            }
        }

        private static void NotifyMe(string episode)
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("==============================");
            Console.WriteLine("New episode : " + episode);
            Console.WriteLine("==============================");
        }

        private static string AnalyzeHtmlToGetLatestEpisode(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var input = doc.DocumentNode.SelectNodes("//table")
                .Where(n => n.Attributes.Contains("class") && n.Attributes["class"].Value == "torrentname")
                .First();

            var latestEpisode = Regex.Match(input.InnerHtml, @"<b>(?<tag>.+)</b>").Result("$1");

            return latestEpisode;
        }

        private static string GetSearchResultHtml(string baseUrl, string resultQueryUrl, CookieContainer cookieContainer)
        {
            var html = string.Empty;

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) })
            {
                var result = client.GetAsync(resultQueryUrl).Result;
                html = result.Content.ReadAsStringAsync().Result;
                result.EnsureSuccessStatusCode();
            }

            return html;
        }

        private static CookieContainer LoginAndGetCookie(string baseUrl, string loginUrl, string username, string password)
        {
            var baseAddress = new Uri(baseUrl);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                });
                var result = client.PostAsync(loginUrl, content).Result;
                result.EnsureSuccessStatusCode();
            }

            return cookieContainer;
        }
    }
}
