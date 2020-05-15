using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;
using static Elementary.Properties.Selectors.PropertyInfos;

namespace Elementary.Properties.Test.Selectors
{
    public class ValuePropertyPairCollectionTest
    {
        public class PropertyTypeArchetypes
        {
            public int Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_excludes_properties()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyTypeArchetypes>(),
                ValueProperties.All<PropertyTypeArchetypes>(),
            configure: cfg => cfg.ExcludeLeft(nameof(PropertyTypeArchetypes.Integer), nameof(PropertyTypeArchetypes.Nullable))).ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
            Assert.Equal(new[]
                {
                    PropertyFromMemberAccess<PropertyTypeArchetypes>(p => p.Struct),
                    PropertyFromMemberAccess<PropertyTypeArchetypes>(p => p.String)
                },
                result.Select(pp => pp.Left).ToArray());
        }

        public class DifferentNames
        {
            public int AlsoInteger { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_overrides_property_pair()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyTypeArchetypes>(),
                ValueProperties.All<DifferentNames>(),
                configure: cfg => cfg.OverridePair(
                    ValueProperties.Single<PropertyTypeArchetypes>(p => p.Integer),
                    ValueProperties.Single<DifferentNames>(p => p.AlsoInteger)))
                .ToArray();

            // ASSERT

            Assert.Equal(4, result.Length);
        }
    }
}