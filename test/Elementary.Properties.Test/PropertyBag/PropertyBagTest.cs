using Elementary.Properties.PropertyBags;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elementary.Properties.Test.PropertyBags
{
    public class PropertyBagTest
    {
        public class Data
        {
            private int nonePublic = 4;

            public int Public { get; set; } = 1;

            private int Private { get; set; } = 2;

            public int PrivatePublic { private get; set; } = 3;

            public int NonePublic { set => this.nonePublic = value; }

            public int PublicPrivate { get; private set; } = 5;

            public int PublicNone { get; } = 6;
        }

        public static IEnumerable<object[]> SamplePropertyBags()
        {
            var data = new Data();
            yield return new object[] { PropertyBag.Of<Data>(data) };

            var pBag = PropertyBag.Of<Data>();
            pBag.SetInstance(data);
            yield return new object[] { pBag };
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_supports_reading_and_writing(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result = propertyBag.IsReadOnly;

            // ASSERT

            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_doesnt_support_adding(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result1 = Assert.Throws<NotSupportedException>(() => propertyBag.Add("p", 1));
            var result2 = Assert.Throws<NotSupportedException>(() => propertyBag.Add(new KeyValuePair<string, object>("p", 1)));
            var result3 = Assert.Throws<NotSupportedException>(() => propertyBag.TryAdd("p", 2));

            // ASSERT

            Assert.Equal("PropertyBag doesn't support adding of properties", result1.Message);
            Assert.Equal("PropertyBag doesn't support adding of properties", result2.Message);
            Assert.Equal("PropertyBag doesn't support adding of properties", result3.Message);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_doesnt_support_removing(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result1 = Assert.Throws<NotSupportedException>(() => propertyBag.Remove("p"));
            var result2 = Assert.Throws<NotSupportedException>(() => propertyBag.Remove(new KeyValuePair<string, object>("p", 1)));

            // ASSERT

            Assert.Equal("PropertyBag doesn't support removing of properties", result1.Message);
            Assert.Equal("PropertyBag doesn't support removing of properties", result2.Message);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_doesnt_support_clearing(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => propertyBag.Clear());

            // ASSERT

            Assert.Equal("PropertyBag doesn't support clearing of properties", result.Message);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_enumerates_all_public_properties_as_keys(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result1 = propertyBag.Keys.ToArray();
            var result2 = propertyBag.Count;

            // ASSERT

            Assert.Equal(new[] { nameof(Data.Public), "Private", nameof(Data.PrivatePublic), nameof(Data.PublicPrivate) }, result1);
            Assert.Equal(4, result2);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_enumerates_all_public_properties_as_values(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result1 = propertyBag.Values.ToArray();

            // ASSERT

            Assert.Equal(new object[] { 1, 2, 3, 5 }, result1);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_contains_all_public_properties_as_keys(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result = propertyBag.ContainsKey(nameof(Data.Public));

            // ASSERT

            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_reads_property_value(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result1 = (int)propertyBag[(nameof(Data.Public))];
            var result2 = propertyBag.TryGetValue(nameof(Data.Public), out var result3);

            // ASSERT

            Assert.Equal(1, result1);
            Assert.True(result2);
            Assert.Equal(1, result3);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_reading_property_value_fails_graciously_on_missing_property(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result = propertyBag.TryGetValue("missing", out var _);

            // ASSERT

            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_reading_property_value_fails_graciously_on_missing_property_getter(IDictionary<string, object> propertyBag)
        {
            // ACT

            var result = propertyBag.TryGetValue("NonePublic", out var _);

            // ASSERT

            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_sets_property_value(PropertyBagBase<Data> propertyBag)
        {
            // ACT

            propertyBag[(nameof(Data.Public))] = 2;

            // ASSERT

            Assert.Equal(2, propertyBag.Instance.Public);
        }

        [Theory]
        [MemberData(nameof(SamplePropertyBags))]
        public void PropertyBag_copies_property_values_to_array(PropertyBagBase<Data> propertyBag)
        {
            // ACT

            propertyBag[(nameof(Data.Public))] = 2;

            // ASSERT

            Assert.Equal(2, propertyBag.Instance.Public);
        }
    }
}