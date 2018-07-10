using System;
using System.IO;
using System.Text;
using LenteApp.Impl.Artyom;

namespace LenteApp.CUI.Artyom
{
    public class Mainh
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            ArtyomSearch artyomSearch = new ArtyomSearch();
            artyomSearch.Initialize();

            foreach (var k in Directory.EnumerateFiles(ArtyomSearch.RootDirectory, "*.txt"))
            {
                if (k != "MyDictionary.txt") artyomSearch.AddFileToIndex(k);
            }

            var input = File.ReadAllText(Path.Combine(ArtyomSearch.RootDirectory, "Input.txt"));
            foreach (var k in artyomSearch.DoSearch(input)) Console.WriteLine(k);
        }
    }
}