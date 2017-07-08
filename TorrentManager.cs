using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;

namespace Supperxin.EpisodeMonitor
{
    /// <summary>
    /// This class is a record for upload bt file to uTorrent client.
    /// </summary>
    public class TorrentManager
    {
        public static void AddDownloadTask()
        {

            HttpClient client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes("user:password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            HttpResponseMessage response = client.GetAsync("http://ip:port/gui/token.html").Result;
            HttpContent content = response.Content;

            // ... Check Status Code
            Console.WriteLine("Response StatusCode: " + (int)response.StatusCode);

            // ... Read the string.
            string result = content.ReadAsStringAsync().Result;


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);

            var input = doc.DocumentNode.SelectNodes("//div")
                .First();

            var token = input.InnerText;

            var filename = "d:\\test.torrent";
            var bytes = File.ReadAllBytes(filename);

            using (var torrentDataContent = new MultipartFormDataContent())
            {
                var dataContent = new ByteArrayContent(bytes);
                // dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                // {
                //     Name = "\"torrent_file\"",
                //     FileName = "\"" + "test.torrent" + "\""
                // };

                torrentDataContent.Add(dataContent, "torrent_file", filename);

                // http://developers.de/blogs/damir_dobric/archive/2013/09/10/problems-with-webapi-multipart-content-upload-and-boundary-quot-quotes.aspx
                var boundaryValue = torrentDataContent.Headers.ContentType.Parameters.FirstOrDefault(p => p.Name == "boundary");
                boundaryValue.Value = boundaryValue.Value.Replace("\"", String.Empty);

                using (var message = client.PostAsync("http://ip:port/gui/?action=add-file&token=" + token, torrentDataContent).Result)
                {
                    var addTaskResult = message.Content.ReadAsStringAsync().Result;
                }
            }


            return;
        }
    }
}