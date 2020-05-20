using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;

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
        public void ValueProperties_excludes_property_by_name()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.All(c => c.Exclude(nameof(PropertyTypeArchetypes.Integer)));

            // ASSERT

            Assert.Equal(new[] { "Struct", "Nullable", "String" }, result.Select(pi => pi.Property.Name).ToArray());
        }

        //[Fact]
        //public void ValueProperties_rejects_include_from_wrong_class()
        //{
        //    // ACT

        //    var result = ValueProperty<PropertyTypeArchetypes>.All(c => c.Exclude(nameof(PropertyTypeArchetypes.Integer)));

        //    // ASSERT

        //    Assert.Equal(new[] { "Struct", "Nullable", "String" }, result.Select(pi => pi.Property.Name).ToArray());
        //}

        [Fact]
        public void ValueProperties_includes_nested_class()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.All(c =>
            {
                c.IncludeValuesOf(p => p.Reference);
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.Property.Name).ToArray());
        }

        public class AccessorArchetypes
        {
            private string missingGetter;

            public int Public { get; set; }

            protected Guid Protected { get; set; }

            private DateTime? Private { get; set; }

            public string MissingGetter { set => this.missingGetter = value; }

            public string MissingSetter => "getter";

            public AccessorArchetypes Reference { set; get; }
        }

        //[Fact]
        //public void ValueProperties_rejects_nested_class_unreadable_property()
        //{
        //    // ACT

        //    var result = ValueProperty<AccessorArchetypes>.All(c =>
        //    {
        //        // ASSERT

        //        Assert.Throws<ArgumentException>(() => c.IncludeValuesOf(c=>c.MissingSetter)));
        //    });
        //}

        [Fact]
        public void ValueProperties_includes_nested_class_property_matching_parents_accessor_archetype()
        {
            // ACT

            var result = ValueProperty<AccessorArchetypes>.AllCanRead(c =>
            {
                c.IncludeValuesOf(p => p.Reference);
            });

            // ASSERT

            var referenceProperty = (ValuePropertyCollectionInnerNode)(result.Single(p => p.Property.Name == "Reference"));

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingSetter" }, referenceProperty.ValueProperties.Select(pi => pi.Property.Name).ToArray());
        }
    }
}