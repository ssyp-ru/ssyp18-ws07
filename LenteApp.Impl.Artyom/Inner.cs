using System;

namespace LenteApp.Impl.Artyom
{
    public class Inner : IComparable<Inner>
    {
        internal string S;
        internal int A;

        public int CompareTo(Inner other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var aComparison = A.CompareTo(other.A);
            return aComparison != 0 ? aComparison : String.Compare(S, other.S, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return $"{S} : {A}";
        }
    }
}