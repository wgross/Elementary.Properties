using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elementary.Properties.PropertyBags
{
    public abstract class PropertyBagBase<T> : IDictionary<string, object?>
        where T : class
    {
        private readonly Dictionary<string, (Func<T, object?> getter, Action<T, object?> setter)> accessors = new Dictionary<string, (Func<T, object?> getter, Action<T, object?> setter)>();

        /// <summary>
        /// The instance shin currenty edieted by the PropertyBag
        /// </summary>
        public T? Instance { get; protected set; }

        #region Access accessor delegates

        internal void Init(IEnumerable<(string name, Func<T, object?> getter, Action<T, object?> setter)> accessors)
        {
            foreach (var a in accessors)
            {
                this.accessors[a.name] = (a.getter, a.setter);
            }
        }

        private bool TrySetValueAtInstance(string key, object? value)
        {
            if (this.accessors.TryGetValue(key, out var gs))
                if (gs.setter is { })
                {
                    gs.setter(this.Instance!, value);
                    return true;
                }
            return false;
        }

        private bool TryGetValueFromInstance(string key, out object? value)
        {
            value = null;
            if (this.accessors.TryGetValue(key, out var gs))
            {
                value = gs.getter(this.Instance!);
                return true;
            }
            return false;
        }

        #endregion Access accessor delegates

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object? this[string key]
        {
            get => this.TryGetValueFromInstance(key, out var value)
                ? value
                : throw new KeyNotFoundException(key);

            set => this.TrySetValueAtInstance(key, value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICollection<string> Keys => this.accessors.Keys;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICollection<object?> Values => this.accessors.Values.Where(v => v.getter is { }).Select(v => v.getter!(this.Instance!)).ToList();

#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Count => this.accessors.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsReadOnly => false;

        public void Add(string key, object? value) => throw new NotSupportedException("PropertyBag doesn't support adding of properties");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, object?> item) => throw new NotSupportedException("PropertyBag doesn't support adding of properties");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Clear() => throw new NotSupportedException("PropertyBag doesn't support clearing of properties");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Contains(KeyValuePair<string, object?> item)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool ContainsKey(string key) => this.accessors.ContainsKey(key);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return this.accessors
                .Where(kv => kv.Value.getter is { })
                .Select(kv => new KeyValuePair<string, object?>(kv.Key, kv.Value.getter?.Invoke(this.Instance!)))
                .GetEnumerator();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Remove(string key) => throw new NotSupportedException("PropertyBag doesn't support removing of properties");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException("PropertyBag doesn't support removing of properties");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryGetValue(string key, out object? value) => this.TryGetValueFromInstance(key, out value);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}