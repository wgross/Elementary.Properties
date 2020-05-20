using System;
using System.Collections.Generic;

namespace Elementary.Properties.Comparers
{
    /// <summary>
    /// a generic equality comparer using generated methods to compare two instances of <typeparamref name="T"/>
    /// and calculating their hashcodes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private readonly Func<T, T, bool> equals;
        private readonly Func<T, int> getHashCode;

        internal DynamicEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this.equals = equals;
            this.getHashCode = getHashCode;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Equals(T x, T y) => this.equals(x, y);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int GetHashCode(T obj) => this.getHashCode(obj);
    }
}