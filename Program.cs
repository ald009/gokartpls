using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace gokartpls
{
    class Gokart
    {
        public string Cegnev { get; set; }
        public string Cim { get; set; }
        public string Tel { get; set; }
        public string Domain { get; set; }
        public Gokart(string cegnev, string cim, string tel, string domain)
        {
            Cegnev = cegnev;
            Cim = cim;
            Tel = tel;
            Domain = domain;
        }
        public void DisplayInfo()
        {
            Console.WriteLine($"Név: {Cegnev}, Cím: {Cim}, Tel.: {Tel}, Weboldal: {Domain}");
        }
    }

    class Racer
    {
        private static Random rnd = new Random();

        public string Vezeteknev { get; set; }
        public string Keresztnev { get; set; }
        public DateTime SzuletesiDatum { get; set; }
        public bool Elmúlt18 { get; set; }
        public string VersenyzoAzonosito { get; set; }
        public string Email { get; set; }

        public void Display()
        {
            Console.WriteLine("--- Versenyző adatai ---");
            Console.WriteLine($"Vezetéknév: {Vezeteknev}");
            Console.WriteLine($"Keresztnév: {Keresztnev}");
            Console.WriteLine($"Születési idő: {SzuletesiDatum:yyyy.MM.dd}");
            Console.WriteLine($"Elmúlt-e 18: {Elmúlt18}");
            Console.WriteLine($"Versenyző-azonosító: {VersenyzoAzonosito}");
            Console.WriteLine($"Email: {Email}");
        }

        public static Racer Generate(string vezetekPath = "vezeteknevek.txt", string keresztPath = "keresztnevek.txt")
        {

            string[] ReadCsvNames(string path) => File.ReadAllText(path).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            var vezetekList = ReadCsvNames(vezetekPath); if (vezetekList.Length == 0) throw new InvalidOperationException("no surnames"); var v = vezetekList[rnd.Next(vezetekList.Length)].Trim(new char[] { (char)39 });
            var keresztList = ReadCsvNames(keresztPath); if (keresztList.Length == 0) throw new InvalidOperationException("no given names"); var k = keresztList[rnd.Next(keresztList.Length)].Trim(new char[] { (char)39 });

            var start = new DateTime(1960, 1, 1);
            var end = DateTime.Today.AddYears(-10);
            int rangeDays = (end - start).Days;
            var dob = start.AddDays(rnd.Next(rangeDays + 1));

            bool isAdult = IsAtLeast18(dob);

            string fullNameNoAccents = RemoveDiacritics(v + " " + k).Replace(" ", "");
            string datePart = dob.ToString("yyyyMMdd");
            string id = $"GO-{fullNameNoAccents}-{datePart}";

            string emailLocal = (RemoveDiacritics(v).ToLowerInvariant() + "." + RemoveDiacritics(k).ToLowerInvariant());
            string email = emailLocal + "@gmail.com";

            return new Racer
            {
                Vezeteknev = v,
                Keresztnev = k,
                SzuletesiDatum = dob,
                Elmúlt18 = isAdult,
                VersenyzoAzonosito = id,
                Email = email
            };
        }

        static bool IsAtLeast18(DateTime dob)
        {
            var today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return age >= 18;
        }

        static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            /*
             KT
             2026.09.07
             Gokart időpontfoglaló - Egyéni kisprojekt
             */

            string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
            string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
            string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
            string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";

            string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
            string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
            // Background colors (use bright backgrounds for visibility)
            string RESET = Console.IsOutputRedirected ? "" : "\x1b[0m";
            string BG_RED = Console.IsOutputRedirected ? "" : "\x1b[101m";
            string BG_GREEN = Console.IsOutputRedirected ? "" : "\x1b[102m";
            string BG_YELLOW = Console.IsOutputRedirected ? "" : "\x1b[103m";
            string FG_BLACK = Console.IsOutputRedirected ? "" : "\x1b[30m";
            string FG_WHITE = Console.IsOutputRedirected ? "" : "\x1b[97m";

            Console.WriteLine("KT");
            Console.WriteLine("2026.09.07");
            Console.WriteLine("Gokart időpontfoglaló - Egyéni kisprojekt\n");

            Gokart gokart = new Gokart("Sexrobot Gokart", "Levél, Erzsébet u. 2, 9221", "+36 20 213 9898", "https://www.sexrobotgokart.com");
            gokart.DisplayInfo();

            Console.WriteLine();
            var racer = Racer.Generate();   //test berakas
            racer.Display();

            var rnd = new Random();
            int v = rnd.Next(50, 150); // random versenyző szám 50-150 között MERT eleg rossz ha pl van 6 versenyzo és rosszul mukodik akkor minden

            List<Racer> rlist = new List<Racer>();
            for (int i = 0; i < v; i++)
            {
                racer = Racer.Generate();
                rlist.Add(racer);
            }

            Console.WriteLine($"\n{rlist.Count} versenyző betöltve!");


            string mainap = DateTime.Now.ToString("dd"); // megnezi hogy honap hanyadik napja van

            List<Racer>[,] tablazat = new List<Racer>[11, 30 - int.Parse(mainap)];

            const int MaxPerCell = 20;

            for (int i = 0; i < (30 - int.Parse(mainap)); i++) // fele legyen 2 oras masik fele 1 oras: index + rnd.Next(0,1) , if 20 skip , random mindegyik 0-15 ig hogy legyen hely a 2 orasoknak 
            {                                           // es mukodjon a skippeles es latszodjon hogy valahol piros
                for (int j = 0; j < 11; j++)
                {

                    int count = rnd.Next(0, 19);

                     var selected = rlist
                       .OrderBy(x => rnd.Next())
                       .Take(count)
                       .ToList();

                    if (tablazat[j, i] == null) tablazat[j, i] = new List<Racer>();
                    tablazat[j, i].AddRange(selected);
                    if (tablazat[j, i].Count > MaxPerCell)
                        tablazat[j, i].RemoveRange(MaxPerCell, tablazat[j, i].Count - MaxPerCell);

                    if (j != 10 && tablazat[j, i].Count < 21)
                    {
                        count = rnd.Next(0, (int)Math.Round(tablazat[j, i].Count / 1.3));
                        selected = tablazat[j, i]
                           .OrderBy(x => rnd.Next())
                           .Take(count)
                           .ToList();

                        if (tablazat[j + 1, i] == null) tablazat[j + 1, i] = new List<Racer>();
                        tablazat[j + 1, i].AddRange(selected);
                        if (tablazat[j + 1, i].Count > MaxPerCell)
                            tablazat[j + 1, i].RemoveRange(MaxPerCell, tablazat[j + 1, i].Count - MaxPerCell);
                    }

                }
            }
            

            
            int cols = tablazat.GetLength(1);
            int rows = tablazat.GetLength(0);

            
            Console.Write("Date       ");
            for (int t = 0; t < rows; t++)
            {
                int startHour = 8 + t;
                int endHour = startHour + 1;
                Console.Write($" {startHour:00}-{endHour:00} ");
            }
            Console.WriteLine();

            
            DateTime today = DateTime.Today;
            for (int c = 0; c < cols; c++)
            {
                var day = today.AddDays(c);
                Console.Write(day.ToString("yyyy.MM.dd") + " ");

                for (int r = 0; r < rows; r++)
                {
                    var cell = tablazat[r, c];
                    int count = (cell == null) ? 0 : cell.Count;

                    string bg;
                    if (count >= 20) bg = BG_RED;
                    else if (count > 17) bg = BG_YELLOW;
                    else if (count > 0) bg = BG_GREEN;
                    else bg = "";

                    // choose foreground color to contrast
                    string fg = string.IsNullOrEmpty(bg) ? "" : FG_BLACK;

                    // print cell as background block with centered count (width 7)
                    string content = count.ToString().PadLeft(2).PadRight(2);
                    string padded = content.PadLeft(5).PadRight(5);

                    if (!string.IsNullOrEmpty(bg))
                        Console.Write($"{bg}{fg} {padded} {RESET}");
                    else
                        Console.Write($" {padded} ");
                }

                Console.WriteLine();
            }
            

            Console.WriteLine("\nNyomjon meg egy billentyűt a kilépéshez...");
            Console.ReadKey();
        }
    }
}
