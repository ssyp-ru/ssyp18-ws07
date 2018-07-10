using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using LenteApp.LibraryBase;

namespace LenteApp.Impl.Artyom
{
    public class ArtyomSearch : ISearchImplBase
    {
        private bool _contains;
        private readonly List<int> Res = new List<int>();
        private readonly Dictionary<string, string> Dict = new Dictionary<string, string>();
        private readonly List<string> Ordinary = new List<string>();
        private readonly List<string> Exact = new List<string>();

        private static readonly char[] Letters =
        {
            'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x',
            'c', 'v', 'b', 'n', 'm'
        };

        private static readonly string[] Symbols =
            {"\n", ";", "\"", "\r\n", ",", "(", ")", " ", ",", "!", "?", ".", ":", "*", "|", "@", "\t", "%"};

        private static readonly string[] Symbols1 =
            {"\"", "\r\n", ",", "(", ")", " ", ",", "!", "?", ".", ":", "*", "|", "\n", "@", "%", "\t"};

        private readonly HashSet<string> NewRequestWords = new HashSet<string>();
        private readonly List<HashSet<string>> HashList = new List<HashSet<string>>();
        private readonly HashSet<string> _requestWords = new HashSet<string>();
        private readonly List<string> FileList = new List<string>();

        public static string RootDirectory => Path.Combine(Environment.CurrentDirectory, "Data", "Artyom");

        private static char Swap_letters(char eng)
        {
            var c = eng;
            switch (eng.ToString().ToLower())
            {
                case "q":
                    c = 'й';
                    break;
                case "w":
                    c = 'ц';
                    break;
                case "e":
                    c = 'у';
                    break;
                case "r":
                    c = 'к';
                    break;
                case "t":
                    c = 'е';
                    break;
                case "y":
                    c = 'н';
                    break;
                case "u":
                    c = 'г';
                    break;
                case "i":
                    c = 'ш';
                    break;
                case "o":
                    c = 'щ';
                    break;
                case "p":
                    c = 'з';
                    break;
                case "a":
                    c = 'ф';
                    break;
                case "s":
                    c = 'ы';
                    break;
                case "d":
                    c = 'в';
                    break;
                case "f":
                    c = 'а';
                    break;
                case "g":
                    c = 'п';
                    break;
                case "h":
                    c = 'р';
                    break;
                case "j":
                    c = 'о';
                    break;
                case "k":
                    c = 'л';
                    break;
                case "l":
                    c = 'д';
                    break;
                case "z":
                    c = 'я';
                    break;
                case "x":
                    c = 'ч';
                    break;
                case "c":
                    c = 'с';
                    break;
                case "v":
                    c = 'м';
                    break;
                case "b":
                    c = 'и';
                    break;
                case "n":
                    c = 'т';
                    break;
                case "m":
                    c = 'ь';
                    break;
                case ",":
                    c = 'б';
                    break;
                case ".":
                    c = 'ю';
                    break;
                case "[":
                    c = 'х';
                    break;
                case "]":
                    c = 'ъ';
                    break;
                case ";":
                    c = 'ж';
                    break;
                case "/":
                    c = 'э';
                    break;
            }

            return c;
        }

        private static string Convert(string eng)
        {
            var str = new StringBuilder(eng.Length);
            foreach (var letter in eng)
            {
                str.Append(Swap_letters(letter));
            }

            return str.ToString();
        }

        private void Z_Dictionary()
        {
            var mainStream = new FileStream(Path.Combine(RootDirectory, "MyDictionary.txt"), FileMode.Open,
                FileAccess.Read);
            TextReader reader = new StreamReader(mainStream);
            var line = reader.ReadLine();
            while (line != null && !line.Contains("ящурка"))
            {
                line = reader.ReadLine();
                var massLine = line?.Split('|', ' ');
                var mainWord = massLine?[0];
                if (massLine == null) continue;
                foreach (var k in massLine)
                {
                    if (k == mainWord) continue;
                    if (!Dict.ContainsKey(k))
                        Dict.Add(k, mainWord);
                }
            }
        }

        private void Input(string input)
        {
            if (!input.Contains("\""))
            {
                var requestWords = input?.Split(Symbols1, StringSplitOptions.RemoveEmptyEntries);
                foreach (var k in requestWords)
                {
                    var s = k.ToLower();
                    if (Dict.ContainsKey(s))
                        NewRequestWords.Add(Dict[s]);
                    else
                    {
                        if (Letters.Any(x => s.Contains(x))) s = Convert(s);
                        NewRequestWords.Add(Dict.ContainsKey(s) ? Dict[s] : s);
                    }
                }
            }
            else
                _contains = true;
        }

        private void Paste(string file, int count)
        {
            var line = File.ReadAllText(file);
            var mass = line.Split(Symbols, StringSplitOptions.RemoveEmptyEntries);
            var hashSet = new HashSet<string>();
            Res.Add(0);
            var i = 0;
            foreach (var k in mass)
            {
                var ooo = false;
                foreach (var t in _requestWords)
                {
                    var n = t.Split(Symbols1, StringSplitOptions.RemoveEmptyEntries);
                    var any = n.Where((t1, j) => t1 != mass[i + j]).Any();

                    if (any != false) continue;
                    Res[count]++;
                    ooo = true;
                }

                if (ooo == false)
                {
                    var s = k.ToLower();
                    s = Dict.ContainsKey(s) ? Dict[s] : s;
                    hashSet.Add(s);
                    if (NewRequestWords.Contains(s)) Res[count]++;
                }

                i++;
            }

            HashList.Add(hashSet);
        }

        private void exact_search(string input)
        {
            var count = 0;
            var first = input[0] == '\"';
            var w = input.Split(new char[] {'\"'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var k in w)
            {
                if ((first && count % 2 == 1) || !first && count % 2 == 0) Ordinary.Add(k);
                else Exact.Add(k);
                count++;
            }
        }

        private void Normalize()
        {
            foreach (var k in string.Join("", Ordinary).Split(Symbols, StringSplitOptions.RemoveEmptyEntries))
                NewRequestWords.Add(Dict.ContainsKey(k) ? Dict[k].ToLower() : k.ToLower());
            foreach (var k in Exact) _requestWords.Add(k.ToLower());
        }

        public void Initialize()
        {
            Z_Dictionary();
        }

        public void AddFileToIndex(string filePath)
        {
            FileList.Add(filePath);
        }

        public List<SearchResult> DoSearch(string input)
        {
            while (true)
            {
                Input(input);
                if (_contains)
                {
                    exact_search(input);
                    Normalize();
                }

                var m = 0;
                var s = new List<string>();
                foreach (var l in FileList)
                {
                    s.Add(l);
                    Paste(l, m);
                    m++;
                }

                var list = Res.Select((t, index) =>
                {
                    var inner = new Inner();
                    inner.S = s[index];
                    inner.A = t;
                    return inner;
                }).ToList();
                list.Sort();
                list.Reverse();

                var sr = new List<SearchResult>();
                foreach (var k in list)
                {
                    var q = new SearchResult
                    {
                        FilePath = k.S,
                        BestContentExtract = "",
                        Score = (uint) k.A
                    };
                    sr.Add(q);
                }

                Res.Clear();
                return sr;
            }
        }
    }
}