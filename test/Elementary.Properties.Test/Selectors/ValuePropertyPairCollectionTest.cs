using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;

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

            var result = ValueProperty<PropertyTypeArchetypes>.Join<PropertyTypeArchetypes>(
                ValueProperty<PropertyTypeArchetypes>.All(),
                ValueProperty<PropertyTypeArchetypes>.All(),
            configure: cfg => cfg.ExcludeLeft(nameof(PropertyTypeArchetypes.Integer), nameof(PropertyTypeArchetypes.Nullable))).ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
            Assert.Equal(new[]
                {
                    Property<PropertyTypeArchetypes>.Info(p => p.Struct),
                    Property<PropertyTypeArchetypes>.Info(p => p.String)
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

            var result = ValueProperty<PropertyTypeArchetypes>.Join<DifferentNames>(
                ValueProperty<PropertyTypeArchetypes>.All(),
                ValueProperty<DifferentNames>.All(),
                configure: cfg => cfg.OverridePair(
                    ValueProperty<PropertyTypeArchetypes>.Info(p => p.Integer),
                    ValueProperty<DifferentNames>.Info(p => p.AlsoInteger)))
                .ToArray();

            // ASSERT

            Assert.Equal(4, result.Length);
        }
    }
}