using System;
using System.Collections.Generic;

namespace LenteApp.LibraryBase
{
    public sealed class SearchResult : IComparable<SearchResult>, IComparable
    {
        public string FilePath { get; set; }
        public string BestContentExtract { get; set; }
        public uint Score { get; set; }

        /// <inheritdoc />
        public int CompareTo(SearchResult other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var scoreComparison = Score.CompareTo(other.Score);
            if (scoreComparison != 0) return scoreComparison;
            return string.Compare(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is SearchResult)) throw new ArgumentException($"Object must be of type {nameof(SearchResult)}");
            return CompareTo((SearchResult) obj);
        }

        public static bool operator <(SearchResult left, SearchResult right)
        {
            return Comparer<SearchResult>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(SearchResult left, SearchResult right)
        {
            return Comparer<SearchResult>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(SearchResult left, SearchResult right)
        {
            return Comparer<SearchResult>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(SearchResult left, SearchResult right)
        {
            return Comparer<SearchResult>.Default.Compare(left, right) >= 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(FilePath)}: {FilePath}, {nameof(BestContentExtract)}: {BestContentExtract}, {nameof(Score)}: {Score}";
        }
    }
}