using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LenteApp.Impl.Sergey;
using LenteApp.LibraryBase;

namespace LenteApp.CUI.Sergey
{
    static class Program
    {
        static void Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            FileStream filePregunta = new FileStream(Path.Combine(SergeySearch.RootPath, "pregunta.txt"), FileMode.Open, FileAccess.Read);
            TextReader textoPregunta = new StreamReader(filePregunta);
            SergeySearch os = new SergeySearch();
            os.Initialize();

            FileStream file_flujo = new FileStream(Path.Combine(SergeySearch.RootPath, "names of files.txt"), FileMode.Open, FileAccess.Read);
            TextReader texto_leyendo = new StreamReader(file_flujo);
            string nombre = texto_leyendo.ReadLine();
            while (nombre != null)
            {
                os.AddFileToIndex(nombre);
                nombre = texto_leyendo.ReadLine();
            }

            string pregunta = textoPregunta.ReadLine();
            while (pregunta != null)
            {
                List<SearchResult> resultado = os.DoSearch(pregunta);
                foreach (var result in resultado)
                {
                    Console.WriteLine(result);
                }

                Console.WriteLine();
                pregunta = textoPregunta.ReadLine();
            }
        }
    }
}