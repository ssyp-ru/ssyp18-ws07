using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO;
using LenteApp.LibraryBase;

namespace LenteApp.Impl.Sergey
{
    public class SergeySearch : ISearchImplBase
    {
        List<indiceador> pagina_de_setas = new List<indiceador>();
        private Zaliznyak dicc;

        public static string RootPath
        {
            get
            {
                var currentDirectory = Environment.CurrentDirectory;
                if (Directory.Exists(Path.Combine(currentDirectory, "Data")))
                    return Path.Combine(currentDirectory, "Data", "Sergey");
                return Path.Combine(currentDirectory, "..", "Data", "Sergey");
            }
        }

        private static string Convertidor(string demanda)
        {
            Dictionary<char, char> traductor = new Dictionary<char, char>
            {
                ['q'] = 'й',
                ['w'] = 'ц',
                ['e'] = 'у',
                ['r'] = 'к',
                ['t'] = 'е',
                ['y'] = 'н',
                ['u'] = 'г',
                ['i'] = 'ш',
                ['o'] = 'щ',
                ['p'] = 'з',
                ['['] = 'х',
                [']'] = 'ъ',
                ['a'] = 'ф',
                ['s'] = 'ы',
                ['d'] = 'в',
                ['f'] = 'а',
                ['g'] = 'п',
                ['h'] = 'р',
                ['j'] = 'о',
                ['k'] = 'л',
                ['l'] = 'д',
                [';'] = 'ж',
                ['\''] = 'э',
                ['z'] = 'я',
                ['x'] = 'ч',
                ['c'] = 'с',
                ['v'] = 'м',
                ['b'] = 'и',
                ['n'] = 'т',
                ['m'] = 'ь',
                [','] = 'б',
                ['.'] = 'ю',
                ['`'] = 'ё'
            };
            char[] red = demanda.ToCharArray();
            for (int i = 0; i < red.Length; i++)
            {
                if (traductor.ContainsKey(red[i]))
                {
                    red[i] = traductor[red[i]];
                }
            }

            demanda = String.Join("", red).ToLower();
            return demanda;
        }

        public List<SearchResult> Busqueda(string asunto)
        {
            int[] contador = new int[pagina_de_setas.Count];
            string[] asuntos = asunto.Split(
                new[]
                {
                    ' ', ',', '.', '!', '?', ';', ':', '+', '=', '-', '\r', '\n', '\t', '\'', '\"', '(', ')', '[',
                    ']', '«', '»', '„', '“', '…',
                    '#', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
                },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var pregunta in asuntos)
            {
                for (int indice = 0; indice < pagina_de_setas.Count; indice++)
                {
                    var arr = pagina_de_setas[indice];
                    if (arr.indiceado_Pagina.Contains(pregunta))
                    {
                        contador[indice]++;
                    }
                    else if (arr.indiceado_Pagina.Contains(Convertidor(pregunta)))
                    {
                        contador[indice]++;
                    }
                    else
                    {
                        if (dicc.SubDiccionario.TryGetValue(pregunta[0], out var value))
                        {
                            foreach (var elm in value)
                            {
                                var i = arr.LevenshteinDistance(elm, pregunta);
                                if (i <= 1)
                                {
                                    if (arr.indiceado_Pagina.Contains(elm))
                                    {
                                        contador[indice]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SearchResult max = new SearchResult();
            int maximal = 0;
            for (int i = 0; i < pagina_de_setas.Count; i++)
            {
                if (contador[i] > maximal && contador[i] != 0)
                {
                    maximal = contador[i];
                    var paginaDeSeta = pagina_de_setas[i];
                    max.FilePath = paginaDeSeta.nombre_del_file;
                    max.Score = (uint) contador[i];
                    max.BestContentExtract = RegioneImportante(max.FilePath, asunto);
                }
            }

            var list = new List<SearchResult>();
            list.Add(max);
            return list;
        }

        public string RegioneImportante(string filenombre, string pregunta)
        {
            if (filenombre == "")
                return "";

            FileStream file_flujo = new FileStream(filenombre, FileMode.Open, FileAccess.Read);
            TextReader texto_leyendo = new StreamReader(file_flujo);
            string[] campo_de_busqueda = texto_leyendo.ReadToEnd().Split(
                new[]
                {
                    ' ', ',', '.', '!', '?', ';', ':', '+', '=', '-', '\r', '\n', '\t', '\'', '\"', '(', ')', '[',
                    ']',
                    '#', '*'
                },
                StringSplitOptions.RemoveEmptyEntries);
            string[] preguntas = pregunta.Split(
                new[]
                {
                    ' ', ',', '.', '!', '?', ';', ':', '+', '=', '-', '\r', '\n', '\t', '\'', '\"', '(', ')', '[',
                    ']', '«', '»', '„', '“', '…',
                    '#', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
                },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < preguntas.Length; i++)
            {
                string normal = dicc.FormaNormala(preguntas[i]);
                if (normal != null)
                {
                    preguntas[i] = normal;
                }
            }

            string[] file_nomalizado = new string[campo_de_busqueda.Length];
            for (int i = 0; i < file_nomalizado.Length; i++)
            {
                string normal = dicc.FormaNormala(campo_de_busqueda[i]);
                if (normal != null)
                {
                    file_nomalizado[i] = normal;
                }
                else
                {
                    file_nomalizado[i] = campo_de_busqueda[i];
                }
            }

            int[] significados = new int[file_nomalizado.Length];
            int indice_de_maximal = 0;
            for (int i = file_nomalizado.Length - 1; i > 0; i--)
            {
                int posicion;
                if (i + 20 > file_nomalizado.Length)
                {
                    posicion = file_nomalizado.Length - 1;
                }
                else
                {
                    posicion = i + 20;
                }

                int numero = 0;
                for (int j = posicion - 1; j > i; j--)
                {
                    if (preguntas.Contains(file_nomalizado[j]))
                    {
                        numero++;
                    }
                }

                if (preguntas.Contains(file_nomalizado[i]))
                {
                    significados[i] = 1 + numero;
                }
                else
                {
                    significados[i] = numero;
                }

                if (significados[i] == significados.Max())
                {
                    indice_de_maximal = i;
                }
            }

            string regionimportante = "";
            for (int i = indice_de_maximal; i < indice_de_maximal + 20; i++)
            {
                regionimportante = regionimportante + campo_de_busqueda[i] + " ";
            }

            return regionimportante;
        }

        public void Initialize()
        {
            dicc = new Zaliznyak(Path.Combine(RootPath, "vortaro.txt"));
        }

        public void AddFileToIndex(string filePath)
        {
            indiceador indiceado_file = new indiceador(filePath);
            pagina_de_setas.Add(indiceado_file);
        }

        public List<SearchResult> DoSearch(string request)
        {
            return Busqueda(request);
        }
    }
}