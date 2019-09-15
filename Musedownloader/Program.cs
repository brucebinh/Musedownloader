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

        static bool error = false;

        static List<Category> cats = new List<Category>();
        static List<Song> songs = new List<Song>();

        static void SingleMode()
        {
            Console.Clear();
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Parçanın adresi (URL): ");
                Console.ForegroundColor = ConsoleColor.White;
                string s = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                string source = HttpMethods.Get(s, s, ref HttpMethods.cookies);
                string title = HttpMethods.GetBetween(source, "<h1>", "</h1>");
                string midiUrl = HttpMethods.GetBetween(source, "{\"midi\":", "?revision=");
                using (var client = new WebClient())
                {
                    string filename = title.Replace("\n", "").Trim() + ".mid";
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
                Console.Write("Ana menüye dönmek için bir tuşa basınız...");Console.ReadKey();

            }
            catch (Exception)
            {
                Console.Clear();
                Console.Write("Bir hata meydana geldi\n\nProgramdan çıkmak için bir tuşa basınız...");
                Console.ReadKey();
            }
        }

        static void CategoryMode()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
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
            Console.ForegroundColor = ConsoleColor.White;
            if (!int.TryParse(Console.ReadLine(), out cat))
            {
                CategoryMode();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;

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
            Console.Write("\n\nAna menüye dönmek için bir tuşa basın...");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Console.Title = "Musescore - Downloader";

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("1 - Tek şarkı indir");
                Console.WriteLine("2 - Kategori seç ve komple indir (VPN Kullanın IP ban yiyorsunuz)");
                Console.WriteLine("\n\n0 - Programdan Çık");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nİşlem numarası girin: ");
                Console.ForegroundColor = ConsoleColor.White;
                if (int.TryParse(Console.ReadLine(), out int opt))
                {
                    if (opt == 1)
                    {
                        SingleMode();
                        if (error)
                        {
                            return;
                        }
                    }
                    else if (opt == 2)
                    {
                        CategoryMode();
                        if (error)
                        {
                            return;
                        }
                    }
                    else if (opt == 0)
                    {
                        return;
                    }
                }
            }
        }
    }
}
