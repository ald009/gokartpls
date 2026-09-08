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
            Console.WriteLine($"Név: {Cegnev}, Cím: {Cim}, Telefonszám: {Tel}, Weboldal: {Domain}");
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
            var vezetek = ReadLinesOrDefault(vezetekPath);
            var kereszt = ReadLinesOrDefault(keresztPath);

            var v = vezetek[rnd.Next(vezetek.Count)].Trim();
            var k = kereszt[rnd.Next(kereszt.Count)].Trim();

            // generate a birth date between 1950-01-01 and today-1y
            var start = new DateTime(1950, 1, 1);
            var end = DateTime.Today.AddYears(-1);
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

        static List<string> ReadLinesOrDefault(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Try to extract names wrapped in single or double quotes: 'Name', "Name"
                        var names = new List<string>();
                        var matches = Regex.Matches(text, "'([^']*)'|\"([^\"]*)\"");
                        foreach (Match m in matches)
                        {
                            var val = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                            if (!string.IsNullOrWhiteSpace(val)) names.Add(val.Trim());
                        }
                        if (names.Count > 0) return names;

                        // Fallback: split by commas and trim quotes/whitespace
                        var splitted = text.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim().Trim('\'', '"'))
                                            .Where(s => !string.IsNullOrWhiteSpace(s))
                                            .ToList();
                        if (splitted.Count > 0) return splitted;

                        // Last fallback: treat as lines
                        var lines = text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                        if (lines.Count > 0) return lines;
                    }
                }
            }
            catch { }
            return new List<string> { "Kovács", "Dénes" };
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
            Console.WriteLine("Gokart időpontfoglaló - Egyéni kisprojekt\n");

            // existing demo gokart
            Gokart gokart = new Gokart("Sexrobot Gokart", "Levél, Erzsébet u. 2, 9221", "+36 20 213 9898", "https://www.sexrobotgokart.com");
            gokart.DisplayInfo();

            Console.WriteLine();
            var racer = Racer.Generate();
            racer.Display();

            Console.WriteLine("\nNyomjon meg egy billentyűt a kilépéshez...");
            Console.ReadKey();
        }
    }
}
