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

        public static List<Racer>[,] CreateTable(List<Racer> rlist, Random rnd, int MaxPerCell)
        {
            DateTime today = DateTime.Today;
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            // number of columns: from today (inclusive) until end of month
            int cols = daysInMonth - today.Day + 1;
            List<Racer>[,] tablazat = new List<Racer>[11, cols];

            for (int i = 0; i < cols; i++)
            {
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

            return tablazat;
        }

        public static void PrintTable(List<Racer>[,] tablazat)
        {
            string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
            string BG_RED = Console.IsOutputRedirected ? "" : "\x1b[101m";
            string BG_GREEN = Console.IsOutputRedirected ? "" : "\x1b[102m";
            string BG_YELLOW = Console.IsOutputRedirected ? "" : "\x1b[103m";
            string FG_BLACK = Console.IsOutputRedirected ? "" : "\x1b[30m";
            string RESET = Console.IsOutputRedirected ? "" : "\x1b[0m";

            int cols = tablazat.GetLength(1); // cols
            int rows = tablazat.GetLength(0);
            Console.WriteLine("");
            Console.Write("Date       ");
            for (int t = 0; t < rows; t++)
            {
                int startHour = 8 + t;
                int endHour = startHour + 1;
                Console.Write($" {startHour:00}-{endHour:00} ");
            }
            Console.WriteLine();

            DateTime today = DateTime.Today; // today
            for (int c = 0; c < cols; c++)
            {
                var day = today.AddDays(c); // day offset
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

                    string fg = string.IsNullOrEmpty(bg) ? "" : FG_BLACK;

                    string content = count.ToString().PadLeft(2).PadRight(2);
                    string padded = content.PadLeft(5).PadRight(5);

                    if (!string.IsNullOrEmpty(bg))
                        Console.Write($"{bg}{fg} {padded} {RESET}");
                    else
                        Console.Write($" {padded} ");
                }

                Console.WriteLine();
            }
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


            const int MaxPerCell = 20;

            // create and display the initial table
            var tablazat = Gokart.CreateTable(rlist, rnd, MaxPerCell);
            Gokart.PrintTable(tablazat);
            
            
            Console.WriteLine("\nSzeretné manuálisan módosítani a foglalásokat? (i/n)");
            var modify = Console.ReadLine();
            if (!string.IsNullOrEmpty(modify) && modify.Trim().ToLowerInvariant() == "i")
            {
                ManualAdjustBookings(rlist, tablazat, MaxPerCell);
                Console.WriteLine("\nA módosítások megtörténtek. Nyomjon meg egy billentyűt a kilépéshez...");
            }

            Console.ReadKey();
        }

        static void ShowRacers(List<Racer> rlist)
        {
            Console.WriteLine("\n--- Versenyzők listája ---");
            foreach (var r in rlist)
            {
                Console.WriteLine($"{r.VersenyzoAzonosito} - {r.Vezeteknev} {r.Keresztnev} ({r.SzuletesiDatum:yyyy.MM.dd})");
            }
        }

        static void ManualAdjustBookings(List<Racer> rlist, List<Racer>[,] tablazat, int MaxPerCell)
        {
            ShowRacers(rlist);
            Gokart.PrintTable(tablazat);
            Console.WriteLine("\nAdja meg a versenyző(ke)t azonosító szerint (vesszővel elválasztva):");
            var idsLine = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(idsLine)) return;
            var ids = idsLine.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            DateTime today = DateTime.Today;

            foreach (var id in ids)
            {
                var racer = rlist.FirstOrDefault(x => x.VersenyzoAzonosito.Equals(id, StringComparison.InvariantCultureIgnoreCase));
                if (racer == null)
                {
                    Console.WriteLine($"Nem található versenyző: {id}");
                    continue;
                }

                // Show full racer details when selected
                Console.WriteLine();
                racer.Display();
                Console.WriteLine("\nAdja meg a cél dátumot (példa: 2026.09.26 vagy 09.26):");
                var dateStr = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(dateStr)) { Console.WriteLine("Üres dátum, kihagyás."); continue; }

                DateTime date;
                string[] formats = new[] { "yyyy.MM.dd", "MM.dd", "yyyy-M-d", "yyyy/MM/dd", "MM/dd" };
                if (!DateTime.TryParseExact(dateStr.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    Console.WriteLine("Érvénytelen dátumformátum.");
                    continue;
                }
                if (date.Year == 1 || date.Year < today.Year - 1) 
                    date = new DateTime(today.Year, date.Month, date.Day);

                var colIndex = (date - today).Days;
                if (colIndex < 0 || colIndex >= tablazat.GetLength(1))
                {
                    Console.WriteLine("A megadott dátum nincs a táblázatban vagy túl távoli.");
                    continue;
                }

                Console.WriteLine("Adja meg a kezdőórákat vesszővel elválasztva (példa: 15,16 vagy tartományként 15-17):");
                var hoursLine = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(hoursLine)) { Console.WriteLine("Nincs megadva óra, kihagyás."); continue; }

                var hourTokens = hoursLine.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);
                var hours = new List<int>();
                foreach (var tok in hourTokens)
                {
                    if (tok.Contains("-"))
                    {
                        var parts = tok.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int sh) && int.TryParse(parts[1], out int eh))
                        {
                            for (int h = sh; h < eh; h++) hours.Add(h);
                        }
                    }
                    else if (int.TryParse(tok, out int h)) hours.Add(h);
                }

                if (hours.Count == 0) { Console.WriteLine("Nincs érvényes óra megadva."); continue; }

                // Remove racer from all cells first (move)
                for (int r = 0; r < tablazat.GetLength(0); r++)
                {
                    for (int c = 0; c < tablazat.GetLength(1); c++)
                    {
                        var cell = tablazat[r, c];
                        if (cell != null && cell.RemoveAll(x => x.VersenyzoAzonosito.Equals(racer.VersenyzoAzonosito, StringComparison.InvariantCultureIgnoreCase)) > 0)
                        {
                            // removed
                        }
                    }
                }


                foreach (var hour in hours)
                {
                    var rowIndex = hour - 8;
                    if (rowIndex < 0 || rowIndex >= tablazat.GetLength(0))
                    {
                        Console.WriteLine($"Óra {hour} kívül esik a foglalási tartományon.");
                        continue;
                    }

                    if (tablazat[rowIndex, colIndex] == null) tablazat[rowIndex, colIndex] = new List<Racer>();
                    var cell = tablazat[rowIndex, colIndex];
                    if (cell.Count >= MaxPerCell)
                    {
                        Console.WriteLine($"A {date:yyyy.MM.dd} {hour}:00 slot tele van (max {MaxPerCell}).");
                        continue;
                    }
                    if (!cell.Any(x => x.VersenyzoAzonosito.Equals(racer.VersenyzoAzonosito, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        cell.Add(racer);
                        Console.WriteLine($"Hozzáadva: {racer.VersenyzoAzonosito} -> {date:yyyy.MM.dd} {hour}:00");
                    }
                }
            }
           
            Console.WriteLine();
            Gokart.PrintTable(tablazat);
        }
    }
}
