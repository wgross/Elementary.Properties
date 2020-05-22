using System;
using System.Collections.Generic;
using System.Linq;

namespace Elementary.Properties.Selectors
{
    public class ValuePropertyPair<L, R>
    {
        public static ValuePropertyPairCollection<L, R> Join(IEnumerable<IValuePropertyCollectionItem> leftProperties, IEnumerable<IValuePropertyCollectionItem> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null, Action<IValuePropertyPairCollectionConfiguration> configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(InnerJoin(leftProperties, rightProperties));
            configure?.Invoke(collection);
            return collection;
        }

        private static IEnumerable<ValuePropertySymmetricPair> JoinImpl(IEnumerable<IValuePropertyCollectionItem> leftProperties, IEnumerable<IValuePropertyCollectionItem> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null)
        {
            onError ??= delegate { };

            var rightProperyMap = rightProperties.ToDictionary(pi => pi.PropertyName);

            foreach (var lpi in leftProperties)
            {
                var exists = rightProperyMap.TryGetValue(lpi.PropertyName, out var rpi);
                if (!exists)
                {
                    onError(JoinError.RightPropertyMissing, (lpi.PropertyName, lpi.PropertyType));
                }
                else if (lpi.PropertyType != rpi.PropertyType)
                {
                    rightProperyMap.Remove(rpi.Info.Name);
                    onError(JoinError.RightPropertyType, (lpi.Info.Name, lpi.Info.PropertyType));
                }
                else
                {
                    rightProperyMap.Remove(rpi.Info.Name);
                    yield return new ValuePropertySymmetricPair(lpi.Info, rpi.Info);
                }
            }
            foreach (var rpi in rightProperyMap.Values)
            {
                onError(JoinError.LeftPropertyMissing, (rpi.PropertyName, rpi.PropertyType));
            }
            yield break;
        }

        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> All(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(InnerJoin(ValueProperty<L>.All(), ValueProperty<R>.All()));
            configure?.Invoke(collection);
            return collection;
        }

        private static IEnumerable<IValuePropertyPair> InnerJoin(IEnumerable<IValuePropertyCollectionItem> left, IEnumerable<IValuePropertyCollectionItem> right)
        {
            return left.Join(
                inner: right,
                outerKeySelector: l => (l.PropertyName, l.PropertyType),
                innerKeySelector: r => (r.PropertyName, r.PropertyType),
                resultSelector: (r, l) => new ValuePropertySymmetricPair(l.Info, r.Info));
        }
    }
}