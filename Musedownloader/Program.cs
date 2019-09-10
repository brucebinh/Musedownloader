using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace Musedownloader
{
    class Program
    {
        static string domain = "http://musescore.com";
        static string baseUrl = "https://musescore.com/sheetmusic";

        static int downloadedCount = 0;
        static int errorCount = 0;

        static List<Category> cats = new List<Category>();
        static List<Song> songs = new List<Song>();

        static void Main(string[] args)
        {
            Console.Title = "Musescore - Downloader";
            HtmlDocument doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(HttpMethods.Get(baseUrl, baseUrl, ref HttpMethods.cookies));
            }
            catch (Exception)
            {
                Console.WriteLine("Sunucu tarafından ban yemiş olabilirsiniz, daha sonra tekrar deneyin.");
                Console.WriteLine("(veya bir VPN kullanın)");
                Console.Write("\nProgramdan çıkmak için bir tuşa basınız...");
                Console.ReadKey();
                return;
            }
            HtmlNodeCollection col = doc.DocumentNode.SelectSingleNode("//*[@id='block-instruments']/div/ul").ChildNodes;
            foreach (HtmlNode item in col)
            {
                if (item.Name != "#text")
                {
                    cats.Add(new Category()
                    {
                        Href = item.ChildNodes[1].Attributes["href"].Value,
                        Name = item.ChildNodes[1].ChildNodes[1].InnerText
                    });
                }
            }

            for (int i = 0; i < cats.Count; i++)
            {
                Console.WriteLine(i + " - " + cats[i].Name);
            }

            int cat = 0;
            Console.Write("\nKategori numarası giriniz: ");
            if (!int.TryParse(Console.ReadLine(), out cat))
            {
                Main(args);
            }

            Console.Clear();
            for (int pageIndex = 1; pageIndex < 101; pageIndex++)
            {
                string page = "&page=" + pageIndex + "#main-content";
                string link = cats[cat].Href + page;
                doc.LoadHtml(HttpMethods.Get(link, link, ref HttpMethods.cookies));
                for (int i = 1; i <= 20; i++)
                {
                    string name = doc.DocumentNode.SelectSingleNode("//div[@class='score-grid score-grid_by_five']/div[" + i + "]/article/div/div[2]/div/h2/a").InnerText;
                    string href = domain + doc.DocumentNode.SelectSingleNode("//div[@class='score-grid score-grid_by_five']/div[" + i + "]/article/div/div[2]/div/h2/a").Attributes["href"].Value;
                    songs.Add(new Song()
                    {
                        Name = name.Trim(),
                        Href = href
                    });
                    Console.WriteLine("Bulunan şarkı: {0}", songs[i - 1].Name);
                }
            }

            Console.Clear();
            foreach (Song s in songs)
            {
                try
                {
                    string source = HttpMethods.Get(s.Href, s.Href, ref HttpMethods.cookies);
                    string midiUrl = HttpMethods.GetBetween(source, "{\"midi\":", "?revision=");
                    using (var client = new WebClient())
                    {
                        string filename = s.Name.Replace("\n", "").Trim() + ".mid";
                        string path = AppDomain.CurrentDomain.BaseDirectory + "Downloads\\" + filename;

                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Downloads\\" + filename))
                        {
                            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Downloads\\"))
                            {
                                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Downloads\\");
                            }
                            midiUrl = midiUrl.Replace("\\", "");
                            midiUrl = midiUrl.Replace("\"", "");
                            try
                            {
                                client.DownloadFile(midiUrl, path);
                                Console.WriteLine("İndirilen dosya: {0}", filename);
                                downloadedCount++;
                            }
                            catch (System.Net.WebException)
                            {
                                Console.WriteLine("Dosya indirilemedi, karakter hatası. ({0})", filename);
                                errorCount++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Hata, bu hata çok fazla kez görüntüleniyorsa sunucu tarafından ban yemiş olabilirsiniz.");
                    errorCount++;
                }
            }

            Console.Clear();
            Console.WriteLine("{0} adet müzik indirildi.", downloadedCount);
            Console.WriteLine("{0} adet hata var.", errorCount);
            Console.Write("\n\nProgramdan çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
