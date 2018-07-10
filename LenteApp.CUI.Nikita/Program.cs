using LenteApp.Impl.Artyom;
using LenteApp.Impl.Nikita;
using LenteApp.Impl.Sergey;
using LenteApp.LibraryBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace LenteApp.CUI.Nikita
{
    internal class Program
    {
        public static List<string> AddFile()
        {
            List<string> filesPaths = new List<string>();
            string fileRoad = "";
            bool addFile = true;
            while (addFile)
            {
                Console.WriteLine("Вы хотите добавить еще файл для поиска? Y-Да, N-Нет");
                ConsoleKeyInfo addKey = Console.ReadKey();
                if (addKey.KeyChar == 'y')
                {
                    Console.WriteLine();
                    Console.WriteLine("Укажите путь файлу");
                    fileRoad = Console.ReadLine();
                    filesPaths.Add(fileRoad);
                }
                else
                {
                    Console.WriteLine();
                    addFile = false;
                }


            }

            return filesPaths;
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        private static void Main(string[] args)
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Console.WriteLine("failed to get output console mode");
                Console.ReadKey();
                return;
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                Console.WriteLine($"failed to set output console mode, error code: {GetLastError()}");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Выберите реализацию которую хотите увидеть");
            Console.WriteLine("  1)Артем");
            Console.WriteLine("  2)Никита");
            Console.WriteLine("  3)Сергей");
            ConsoleKeyInfo searchKey = Console.ReadKey();
            Console.WriteLine();
            ISearchImplBase engine;
            switch (searchKey.KeyChar)
            {
                case '1':
                    {
                        engine = new ArtyomSearch();
                        break;
                    }
                case '3':
                    {
                        engine = new SergeySearch();

                        break;
                    }
                default:
                    {
                        engine = new NikitaSearch();
                        break;
                    }
            }

            engine.Initialize();

            foreach (var filePath in AddFile())
            {
                engine.AddFileToIndex(filePath);
            }

            string inputText = File.ReadAllText(Path.Combine(NikitaSearch.DataDirectory, "vvod.txt"));
            foreach (var lines in engine.DoSearch(inputText))
            {
                //Console.WriteLine(lines);
                if (lines.Score != 0)
                {
                    Console.Write("\u001b[1;33m>>>  \u001b[0m");
                    Console.Write("\u001b[1;37m");
                    Console.Write(lines.FilePath);
                    Console.WriteLine("\u001b[0m");
                    Console.WriteLine("  " + lines.BestContentExtract);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            #if false
            Console.WriteLine("░░░░░░░█▐▓▓░████▄▄▄█▀▄▓▓▓▌█");
            Console.WriteLine("░░░░░▄█▌▀▄▓▓▄▄▄▄▀▀▀▄▓▓▓▓▓▌█");
            Console.WriteLine("░░░▄█▀▀▄▓█▓▓▓▓▓▓▓▓▓▓▓▓▀░▓▌█ ");
            Console.WriteLine("░░█▀▄▓▓▓███▓▓▓███▓▓▓▄░░▄▓▐█▌ ");
            Console.WriteLine("░█▌▓▓▓▀▀▓▓▓▓███▓▓▓▓▓▓▓▄▀▓▓▐█ ");
            Console.WriteLine("▐█▐██▐░▄▓▓▓▓▓▀▄░▀▓▓▓▓▓▓▓▓▓▌█▌");
            Console.WriteLine("█▌███▓▓▓▓▓▓▓▓▐░░▄▓▓███▓▓▓▄▀▐█");
            Console.WriteLine("█▐█▓▀░░▀▓▓▓▓▓▓▓▓▓██████▓▓▓▓▐█");
            Console.WriteLine("▌▓▄▌▀░▀░▐▀█▄▓▓██████████▓▓▓▌█▌");
            Console.WriteLine("▌▓▓▓▄▄▀▀▓▓▓▀▓▓▓▓▓▓▓▓█▓█▓█▓▓▌█▌");
            Console.WriteLine("█▐▓▓▓▓▓▓▄▄▄▓▓▓▓▓▓█▓█▓█▓█▓▓▓▐█");
#endif
            Console.ReadLine();
        }
    }
}
