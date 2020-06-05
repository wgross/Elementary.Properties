using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;

namespace Elementary.Properties.Test.Selectors
{
    public class ValuePropertyPairCollectionTest
    {
        #region Properties of same type and name

        public class PropertyTypeArchetypes_Left
        {
            public int Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_Left2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class PropertyTypeArchetypes_Left2
        {
            public int Integer2 { get; set; }

            public Guid Struct2 { get; set; }

            public DateTime? Nullable2 { get; set; }

            public string String2 { get; set; }

            public PropertyTypeArchetypes_Left Reference2 { set; get; }

            public int[] Collection2 { get; set; }
        }

        public class PropertyTypeArchetypes_Right
        {
            public int Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_Right2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class PropertyTypeArchetypes_Right2
        {
            public int Integer2 { get; set; }

            public Guid Struct2 { get; set; }

            public DateTime? Nullable2 { get; set; }

            public string String2 { get; set; }

            public PropertyTypeArchetypes_Right Reference2 { set; get; }

            public int[] Collection2 { get; set; }
        }

        [Fact]
        public void ValuePropertyPairCollection_contains_matching_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.Integer),
                nameof(PropertyTypeArchetypes_Left.Struct),
                nameof(PropertyTypeArchetypes_Left.Nullable),
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_excludes_property()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection(configure: c =>
            {
                c.ExcludeLeftValue(o => o.Integer);
            }).ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.Struct),
                nameof(PropertyTypeArchetypes_Left.Nullable),
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_includes_nested_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection(configure: c =>
            {
                c.IncludeNested(l => l.Reference);
            }).ToArray();

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.Left.Name));

            var result_level1 = result
                .OfType<ValuePropertyPairNested>()
                .Single(p => p.Left.Name == nameof(PropertyTypeArchetypes_Left.Reference))
                .NestedPropertyPairs;

            Assert.Equal(new[] { "Integer2", "Struct2", "Nullable2", "String2" }, result_level1.Select(pi => pi.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_excludes_nested_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection(configure: c =>
            {
                c.IncludeNested(l => l.Reference);
                c.ExcludeLeftValue(l => l.Reference.Integer2);
            }).ToArray();

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.Left.Name));

            var result_level1 = result
                .OfType<ValuePropertyPairNested>()
                .Single(p => p.Left.Name == nameof(PropertyTypeArchetypes_Left.Reference))
                .NestedPropertyPairs;

            Assert.Equal(new[] { "Struct2", "Nullable2", "String2" }, result_level1.Select(pi => pi.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_excluding_nested_property_rejects_unknown_nested()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection(configure: c =>
              {
                  //missing//c.IncludeNested(l => l.Reference);
                  c.ExcludeLeftValue(l => l.Reference.Integer2);
              }));

            // ASSERT

            Assert.Equal($"Exclude property(name='Integer2') failed: Nested property(name='Reference') isn't included", result.Message);
        }

        [Fact]
        public void ValuePropertyPairCollection_includes_nested2_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right>.MappableCollection(configure: c =>
            {
                c.IncludeNested(l => l.Reference);
                c.IncludeNested(l => l.Reference.Reference2);
            }).ToArray();

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference" }, result.Select(pi => pi.Left.Name));

            var result_level1 = result
                .OfType<ValuePropertyPairNested>()
                .Single(p => p.Left.Name == nameof(PropertyTypeArchetypes_Left.Reference))
                .NestedPropertyPairs;

            Assert.Equal(new[] { "Integer2", "Struct2", "Nullable2", "String2", "Reference2" }, result_level1.Select(pi => pi.Left.Name));

            var result_level2 = result_level1
                 .OfType<ValuePropertyPairNested>()
                 .Single(p => p.Left.Name == nameof(PropertyTypeArchetypes_Left2.Reference2))
                 .NestedPropertyPairs;

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String" }, result_level2.Select(pi => pi.Left.Name));
        }

        #endregion Properties of same type and name

        #region Handling of different names

        public class PropertyTypeArchetypes_Right_DifferentName
        {
            public int Integer { get; set; }

            public Guid Struct_Different { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_Left Reference_Different { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValuePropertyPairCollection_including_nested_properties_fails_for_missing_right_side()
        {
            // ACT & ASSERT

            var result = Assert.Throws<InvalidOperationException>(() => ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right_DifferentName>.MappableCollection(configure: c =>
            {
                c.IncludeNested(l => l.Reference);
            }).ToArray());

            Assert.Equal($"Property(name='Reference') wasn't found in type(name='PropertyTypeArchetypes_Right_DifferentName')", result.Message);
        }

        [Fact]
        public void ValuePropertyPairCollection_ignores_missing_property_name()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Right_DifferentName>.MappableCollection().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.Integer),
                nameof(PropertyTypeArchetypes_Left.Nullable),
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        #endregion Handling of different names

        public class PropertyTypeArchetypes_DifferentTypes
        {
            public long Integer { get; set; }

            public DateTime Struct { get; set; }

            public DateTime Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_DifferentTypes Reference { set; get; }

            public long[] Collection { get; set; }
        }

        public class PropertyTypeArchetypes_MissingProperty
        {
            public int Integer { get; set; }

            public Guid Struct_Different { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_Left Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValuePropertyPairCollection_ignores_different_types_having_same_names()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_DifferentTypes>.MappableCollection().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        public class PropertyTypeArchetypes_Nullable
        {
            public int? Integer { get; set; }

            public Guid? Struct { get; set; }

            public string String { get; set; }

            public PropertyTypeArchetypes_Left Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void MappableValuePropertyPairCollection_contains_matching_right_nullable_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Left, PropertyTypeArchetypes_Nullable>.MappableCollection().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.Integer),
                nameof(PropertyTypeArchetypes_Left.Struct),
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void MappableValuePropertyPairCollection_rejects_matching_left_nullable_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes_Nullable, PropertyTypeArchetypes_Left>.MappableCollection().ToArray();

            // ASSERT

            Nullable<int> x;
            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes_Left.String)
            },
            result.Select(pp => pp.Left.Name));
        }
    }
}