using System;
using System.Collections.Generic;
using System.IO;

namespace LenteApp.Impl.Sergey
{
    public class Zaliznyak
    {
        public Dictionary<string, string> DiccionarioOperativo = new Dictionary<string, string>();
        public readonly Dictionary<char, List<string>> SubDiccionario = new Dictionary<char, List<string>>();

        public Zaliznyak(string dicc)
        {
            FileStream LeerAlFile = new FileStream(dicc, FileMode.Open, FileAccess.Read);
            TextReader LeerAlTexto = new StreamReader(LeerAlFile);
            string LeerRenglon = LeerAlTexto.ReadLine();
            string alfabeto = "абвгдеёжзийклмнопрстуфхцчшщэюя";
            for (int i = 0; i < 30; i++)
            {
                SubDiccionario.Add(alfabeto[i], new List<string>());
            }

            while (LeerRenglon != null)
            {
                string[] palabras = LeerRenglon.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                char litera = palabras[0][0];
                for (var contador = 0; contador < palabras.Length; contador++)
                {
                    if (!DiccionarioOperativo.ContainsKey(palabras[contador]))
                    {
                        DiccionarioOperativo.Add(palabras[contador], palabras[0]);
                    }

                    SubDiccionario[litera].Add(palabras[contador]);
                }

                LeerRenglon = LeerAlTexto.ReadLine();
            }
        }

        public string FormaNormala(string entrada)
        {
            if (DiccionarioOperativo.ContainsKey(entrada))
            {
                return DiccionarioOperativo[entrada];
            }
            else
            {
                return null;
            }
        }
    }
}