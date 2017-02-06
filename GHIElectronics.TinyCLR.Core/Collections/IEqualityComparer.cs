namespace System.Collections
{
    using System;
    public interface IEqualityComparer
    {
        bool Equals(object x, object y);
        int GetHashCode(object obj);
    }
}