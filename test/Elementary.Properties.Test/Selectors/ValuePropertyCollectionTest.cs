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

            public PropertyTypeArchetypes2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class PropertyTypeArchetypes2
        {
            public int Integer2 { get; set; }

            public Guid Struct2 { get; set; }

            public DateTime? Nullable2 { get; set; }

            public string String2 { get; set; }

            public PropertyTypeArchetypes Reference2 { set; get; }

            public int[] Collection2 { get; set; }
        }

        [Fact]
        public void ValueProperties_excludes_property()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c => c.ExcludeValue(p => p.Integer));

            // ASSERT

            Assert.Equal(new[] { "Struct", "Nullable", "String" }, result.Select(pi => pi.PropertyName).ToArray());
        }

        [Fact]
        public void ValueProperties_includes_nested_class()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c =>
            {
                c.IncludeNested(p => p.Reference);
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.PropertyName));

            var result_level1 = result
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes.Reference))
                .NestedProperties
                .Select(pi => pi.PropertyName).ToArray();

            Assert.Equal(new[] { "Integer2", "Struct2", "Nullable2", "String2" }, result_level1);
        }

        [Fact]
        public void ValueProperties_excludes_property_in_nested_class()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c =>
            {
                c.IncludeNested(p => p.Reference);
                c.ExcludeValue(p => p.Reference.Integer2);
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.PropertyName));

            var result_level1 = result
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes.Reference))
                .NestedProperties
                .Select(pi => pi.PropertyName).ToArray();

            Assert.Equal(new[] { "Struct2", "Nullable2", "String2" }, result_level1);
        }

        [Fact]
        public void ValueProperties_excluding_property_in_nested_class_rejects_unkown_nested()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c =>
             {
                 //missing//c.IncludeNested(p => p.Reference);
                 c.ExcludeValue(p => p.Reference.Integer2);
             }));

            // ASSERT

            Assert.Equal($"Exclude property(name='Integer2') failed: Nested property(name='Reference') isn't included", result.Message);
        }

        [Fact]
        public void ValueProperties_includes_nested2_class()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c =>
            {
                c.IncludeNested(p => p.Reference);
                c.IncludeNested(p => p.Reference.Reference2);
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.PropertyName).ToArray());

            var result_level1 = result
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes.Reference))
                .NestedProperties;

            Assert.Equal(new[] { "Integer2", "Struct2", "Nullable2", "String2", "Reference2" }, result_level1.Select(pi => pi.PropertyName));

            var result_level2 = result_level1
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes2.Reference2))
                .NestedProperties;

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String" }, result_level2.Select(pi => pi.PropertyName));
        }

        [Fact]
        public void ValueProperties_excludes_property_in_nested2_class()
        {
            // ACT

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite(c =>
            {
                c.IncludeNested(p => p.Reference);
                c.IncludeNested(p => p.Reference.Reference2);
                c.ExcludeValue(p => p.Reference.Reference2.Integer);
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.PropertyName).ToArray());

            var result_level1 = result
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes.Reference))
                .NestedProperties;

            Assert.Equal(new[] { "Integer2", "Struct2", "Nullable2", "String2", "Reference2" }, result_level1.Select(pi => pi.PropertyName));

            var result_level2 = result_level1
                .OfType<ValuePropertyNested>()
                .Single(p => p.PropertyName == nameof(PropertyTypeArchetypes2.Reference2))
                .NestedProperties;

            Assert.Equal(new[] { "Struct", "Nullable", "String" }, result_level2.Select(pi => pi.PropertyName));
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
                c.IncludeNested(p => p.Reference);
            });

            // ASSERT

            var referenceProperty = (ValuePropertyNested)(result.Single(p => p.PropertyName == "Reference"));

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingSetter" }, referenceProperty.NestedProperties.Select(pi => pi.PropertyName).ToArray());
        }
    }
}