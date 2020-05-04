using System;
using System.Collections.Generic;

namespace Elementary.Properties.Comparers
{
    public class DynamicEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equalityComparer;
        private readonly Func<T, int> getHashCode;

        public DynamicEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this.equalityComparer = equals;
            this.getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            return this.equalityComparer(x, y);
        }

        public int GetHashCode(T obj) => this.getHashCode(obj);
    }
}