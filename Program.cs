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
            var rnd = new Random();

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

            List<Racer> rlist = new List<Racer>(); // 150 versenyzo berakasa
            for (int i = 0; i < 150; i++)
            {
                racer = Racer.Generate();
                rlist.Add(racer);
            }

            Console.WriteLine($"\n{rlist.Count} versenyző betöltve!");


            string mainap = DateTime.Now.ToString("dd"); // megnezi hogy honap hanyadik napja van
            Console.WriteLine(mainap);

            List<Racer>[,] tablazat = { { }, { } };

            var rnd = new Random();
            List<Racer> tarolo = new List<Racer>();
            for (int i = 0; i < int.Parse(mainap); i++) // fele legyen 2 oras masik fele 1 oras: index + rnd.Next(0,1) , if 20 skip , random mindegyik 0-15 ig hogy legyen hely a 2 orasoknak 
            {                                           // es mukodjon a skippeles es latszodjon hogy valahol piros
                for (int j = 0; j < 11; j++)
                {
                    tarolo.Clear();
                    for (int k = 0; k < rnd.Next(0,15); k++)
                    {
                        tarolo.Add(rlist[rnd.Next(0, rlist.Count)]);
                    }
                    tablazat[j, i] = tarolo;
                }
            }

            Console.WriteLine("\nNyomjon meg egy billentyűt a kilépéshez...");
            Console.ReadKey();
        }
    }
}
