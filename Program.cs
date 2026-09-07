using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kovács Tibor\n2026.09.07.\nGokart időpontfoglaló - Egyéni kisprojekt");
            Gokart gokart = new Gokart("Sexrobot Gokart", "Levél, Erzsébet u. 2, 9221", "+36 20 213 9898", "https://www.sexrobotgokart.com");
            gokart.DisplayInfo();
        }


    }
}
