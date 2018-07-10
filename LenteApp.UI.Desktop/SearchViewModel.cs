using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using LenteApp.Impl.Artyom;
using LenteApp.Impl.Nikita;
using LenteApp.Impl.Sergey;
using LenteApp.LibraryBase;

namespace LenteApp.UI.Desktop
{
    public class SearchViewModel
    {
        private IEnumerable<string> Names { get; } =
            new[]
            {
                "Sergey", "Nikita", "Artyom"
            };

        private ObservableCollection<string> Answer { get; set; }
            = new ObservableCollection<string>();

        public string SearchQuery { get; set; } = "";
        public string SelectedName { get; set; } = "Artyom";

        public ICommand ResetCommand { get; }

        public SearchViewModel()
        {
            ResetCommand = new DelegateCommand(Reset);
            Reset();
        }

        private void Reset()
        {
            Answer.Clear();
            Console.WriteLine(SearchQuery);
            ISearchImplBase search = new ArtyomSearch();
            switch (SelectedName)
            {
                case "Artyom":
                    break;
                case "Sergey":
                    search = new SergeySearch();
                    break;
                case "Nikita":
                    search = new NikitaSearch();
                    break;
            }

            search.Initialize();
            foreach (var k in Directory.EnumerateFiles(ArtyomSearch.RootDirectory, "*.txt"))
            {
                if (k != "MyDictionary.txt") search.AddFileToIndex(k);
            }

            if (SearchQuery == "") return;
            foreach (var inner in search.DoSearch(SearchQuery))
                Answer.Add($"{Path.GetFileName(inner.FilePath)} : {inner.Score}");
        }
    }
}