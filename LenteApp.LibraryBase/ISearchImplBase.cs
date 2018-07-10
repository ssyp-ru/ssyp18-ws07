using System.Collections.Generic;

namespace LenteApp.LibraryBase
{
    public interface ISearchImplBase
    {
        void Initialize();
        void AddFileToIndex(string filePath);
        List<SearchResult> DoSearch(string request);
    }
}