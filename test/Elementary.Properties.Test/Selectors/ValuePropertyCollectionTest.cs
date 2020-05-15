using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;
using static Elementary.Properties.Selectors.PropertyInfos;

namespace Elementary.Properties.Test.Selectors
{
    public class ValuePropertyCollectionTest
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
        public void ValuePorperties_excludes_property_by_name()
        {
            // ACT

            var result = ValueProperties.All<PropertyTypeArchetypes>(c => c.Exclude(nameof(PropertyTypeArchetypes.Integer)));

            // ASSERT

            Assert.Equal(new[] { "Struct", "Nullable", "String" }, result.Select(pi => pi.Name).ToArray());
        }

        [Fact]
        public void ValuePorperties_includes_references_and_collections_by_name()
        {
            // ACT

            var result = ValueProperties.All<PropertyTypeArchetypes>(c =>
            {
                c.Include(PropertyFromMemberAccess<PropertyTypeArchetypes>(p => p.Reference));
                c.Include(PropertyFromMemberAccess<PropertyTypeArchetypes>(p => p.Collection));
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference", "Collection" }, result.Select(pi => pi.Name).ToArray());
        }
    }
}