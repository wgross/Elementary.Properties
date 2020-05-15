using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elementary.Properties.PropertyBags
{
    public abstract class PropertyBagBase<T> : IDictionary<string, object?>
        where T : class
    {
        private readonly Dictionary<string, (Func<T, object?>? getter, Action<T, object?>? setter)> accessors = new Dictionary<string, (Func<T, object?>? getter, Action<T, object?>? setter)>();

        public T? Instance { get; protected set; }

        #region Access accessor delegates

        internal void Init(IEnumerable<(string name, Func<T, object?> getter)> getters, IEnumerable<(string name, Action<T, object?> setter)> setters)
        {
            foreach (var g in getters)
            {
                this.accessors[g.name] = (g.getter, null);
            }
            foreach (var s in setters)
            {
                if (this.accessors.TryGetValue(s.name, out var g))
                    this.accessors[s.name] = (g.getter, s.setter);
                else
                    this.accessors[s.name] = (null, s.setter);
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

        protected bool TryGetValueFromInstance(string key, out object? value)
        {
            value = null;
            if (this.accessors.TryGetValue(key, out var gs))
                if (gs.getter is { })
                {
                    value = gs.getter(this.Instance!);
                    return true;
                }

            return false;
        }

        #endregion Access accessor delegates

        public object? this[string key]
        {
            get => this.TryGetValueFromInstance(key, out var value)
                ? value
                : throw new KeyNotFoundException(key);

            set => this.TrySetValueAtInstance(key, value);
        }

        public ICollection<string> Keys => this.accessors.Keys;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        public ICollection<object?> Values => this.accessors.Values.Where(v => v.getter is { }).Select(v => v.getter!(this.Instance!)).ToList();
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        public int Count => this.accessors.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object? value) => throw new NotSupportedException("PropertyBag doesn't support adding of properties");

        public void Add(KeyValuePair<string, object?> item) => throw new NotSupportedException("PropertyBag doesn't support adding of properties");

        public void Clear() => throw new NotSupportedException("PropertyBag doesn't support clearing of properties");

        public bool Contains(KeyValuePair<string, object?> item)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsKey(string key) => this.accessors.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return this.accessors
                .Where(kv => kv.Value.getter is { })
                .Select(kv => new KeyValuePair<string, object?>(kv.Key, kv.Value.getter?.Invoke(this.Instance!)))
                .GetEnumerator();
        }

        public bool Remove(string key) => throw new NotSupportedException("PropertyBag doesn't support removing of properties");

        public bool Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException("PropertyBag doesn't support removing of properties");

        public bool TryGetValue(string key, out object? value) => this.TryGetValueFromInstance(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}