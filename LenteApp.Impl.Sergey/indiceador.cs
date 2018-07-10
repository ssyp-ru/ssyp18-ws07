using System;
using System.Collections.Generic;
using System.IO;

namespace LenteApp.Impl.Sergey
{
    public class indiceador
    {
        public HashSet<string> indiceado_Pagina = new HashSet<string>();
        public string nombre_del_file;

        private static readonly char[] Separador = new char[]
        {
            ' ', ',', '.', '!', '?', ';', ':', '+', '=', '-', '/', '\r', '\n', '\t', '\'', '\"', '(', ')', '[',
            ']', '«', '»', '„', '“', '…', '`' , '#', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public indiceador(string nombre_del_file)
        {
            this.nombre_del_file = nombre_del_file;
            if (!File.Exists(CachePath()))
            {
                Сrear_el_set();
                Salvar_el_set();
            }
            else
            {
                Leer_set_file();
            }
        }

        public string CachePath()
        {
            var sets = Path.Combine(SergeySearch.RootPath, "Sets");
            if (!Directory.Exists(sets))
                Directory.CreateDirectory(sets);
            return Path.Combine(sets, Path.GetFileNameWithoutExtension(nombre_del_file) + "_set.txt");
        }

        public void Сrear_el_set()
        {
            Zaliznyak dicParaTrabajo = new Zaliznyak(Path.Combine(SergeySearch.RootPath, "vortaro.txt"));
            FileStream rioDePagina = new FileStream(Path.Combine(SergeySearch.RootPath, nombre_del_file), FileMode.Open, FileAccess.Read);
            TextReader leerLaPagina = new StreamReader(rioDePagina);
            string pagina = leerLaPagina.ReadLine();
            while (pagina != null)
            {
                pagina = pagina.ToLower();
                var todasPalabras = pagina.Split(
                    Separador,
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (var palabra in todasPalabras)
                {
                    var FormaNormala = dicParaTrabajo.FormaNormala(palabra);
                    if (FormaNormala != null && FormaNormala.Length > 2)
                    {
                        indiceado_Pagina.Add(FormaNormala);
                    }
                    else
                    {
                        //Разблокируйте, коли хотите обработку текстов на опечатки (предупрежу: она ОЧЕНЬ долгая)
                        /*else if (dic_para_trabajo.sub_dicts.TryGetValue(palabra[0], out var value))
                        {
                            Console.WriteLine(palabra);
                            int i = 0;
                            foreach (var elm in value)
                            {
                                i = LevenshteinDistance(elm, palabra);
                                if (i < 2)
                                {
                                    indiceado_Pagina.Add(dic_para_trabajo.FormaNormala(elm));
                                    break;
                                }
                            }
    
                            if (i >= 2)
                            {
                                indiceado_Pagina.Add(palabra);
                            }
                        }*/
                        if (palabra.Length > 2)
                        {
                            indiceado_Pagina.Add(palabra);
                        }
                    }
                }

                pagina = leerLaPagina.ReadLine();
            }
        }

        public void Salvar_el_set()
        {
            
            FileStream file_flujo = new FileStream(CachePath(), FileMode.Create, FileAccess.Write);
            TextWriter texto_escribiendo = new StreamWriter(file_flujo);
            texto_escribiendo.Write(String.Join(" ", indiceado_Pagina));
            texto_escribiendo.Close();
            file_flujo.Close();
        }

        public void Leer_set_file()
        {
            FileStream file_flujo = new FileStream(CachePath(), FileMode.Open, FileAccess.Read);
            TextReader texto_leyendo = new StreamReader(file_flujo);
            string linea = texto_leyendo.ReadLine();
            while (linea != null)
            {
                string[] lineas = linea.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var linnea in lineas)
                {
                    indiceado_Pagina.Add(linnea);
                }

                linea = texto_leyendo.ReadLine();
            }

            texto_leyendo.Close();
            file_flujo.Close();
        }

        public int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }

            if (String.IsNullOrEmpty(target)) return source.Length;
            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            for (var j = 1; j <= m; j++) distance[0, j] = j;
            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                            distance[previousRow, j] + 1,
                            distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
                }
            }

            return distance[currentRow, m];
        }
    }
}