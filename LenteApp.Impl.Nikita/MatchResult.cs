using System;

namespace LenteApp.Impl.Nikita
{
    public class MatchResult : IComparable<MatchResult>, IComparable
    {
        public string FileName;
        public string MatchContent;
        public int MatchHighestScore;
        public int MatchScore;

        public int CompareTo(MatchResult other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var matchHighestScoreComparison = MatchHighestScore.CompareTo(other.MatchHighestScore);
            if (matchHighestScoreComparison != 0)
            {
                return matchHighestScoreComparison;
            }

            var matchScoreComparison = MatchScore.CompareTo(other.MatchScore);
            if (matchScoreComparison != 0)
            {
                return matchScoreComparison;
            }

            var fileNameComparison = String.Compare(FileName, other.FileName, StringComparison.Ordinal);
            if (fileNameComparison != 0)
            {
                return fileNameComparison;
            }

            return String.Compare(MatchContent, other.MatchContent, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (!(obj is MatchResult))
            {
                throw new ArgumentException($"Object must be of type {nameof(MatchResult)}");
            }

            return CompareTo((MatchResult)obj);
        }
    }
}