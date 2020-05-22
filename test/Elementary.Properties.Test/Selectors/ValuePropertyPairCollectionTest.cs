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

            public PropertyTypeArchetypes Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_excludes_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes>.Join(
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

        //[Fact]
        //public void ValueProperties_joining_overrides_property_pair()
        //{
        //    // ACT

        //    var result = ValuePropertyPair<PropertyTypeArchetypes, DifferentNames>.Join(
        //        ValueProperty<PropertyTypeArchetypes>.All(),
        //        ValueProperty<DifferentNames>.All(),
        //        configure: cfg => cfg.(
        //            ValueProperty<PropertyTypeArchetypes>.Info(p => p.Integer),
        //            ValueProperty<DifferentNames>.Info(p => p.AlsoInteger)))
        //        .ToArray();

        //    // ASSERT

        //    Assert.Equal(4, result.Length);
        //}

        [Fact]
        public void ValuePropertyPairCollection_contains_matching_properties()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes>.All().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_ignores_different_types_having_same_names()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes_DifferentTypes>.All().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_ignores_missing_property_name()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes_MissingProperty>.All().ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_excludes_property()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes>.All(configure: c =>
             {
                 c.ExcludeLeft(nameof(PropertyTypeArchetypes.Integer));
             }).ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String)
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_includes_property_with_different_names()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes_MissingProperty>.All(
                configure: c => c.IncludePair(
                     nameof(PropertyTypeArchetypes.Struct),
                     nameof(PropertyTypeArchetypes_MissingProperty.Struct_Different))).ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String),
                nameof(PropertyTypeArchetypes.Struct),
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_includes_property_with_different_types()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes_DifferentTypes>.All(
                configure: c => c.IncludePair(
                     nameof(PropertyTypeArchetypes.Struct),
                     nameof(PropertyTypeArchetypes_DifferentTypes.Struct))).ToArray();

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.String),
                nameof(PropertyTypeArchetypes.Struct),
            },
            result.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_includes_matching_properties_of_nested_class()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes>.All(
                configure: c => c.IncludeNested(n => n.Reference)).ToArray();

            // ASSERT

            var referenceProperty = result.OfType<ValuePropertyPairNested>().Single(p => p.Left.Name == nameof(PropertyTypeArchetypes.Reference));

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String),
                nameof(PropertyTypeArchetypes.Reference)
            },
            result.Select(pp => pp.Left.Name));

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String)
            },
            referenceProperty.NestedPropertyPairs.Select(pp => pp.Left.Name));
        }

        [Fact]
        public void ValuePropertyPairCollection_excludes_property_of_nested_class()
        {
            // ACT

            var result = ValuePropertyPair<PropertyTypeArchetypes, PropertyTypeArchetypes>.All(
                configure: c => c.IncludeNested(
                    propertyAccess: n => n.Reference,
                    configure: c => c.ExcludeLeft(nameof(PropertyTypeArchetypes.Integer)))).ToArray();

            // ASSERT

            var referenceProperty = result.OfType<ValuePropertyPairNested>().Single(p => p.Left.Name == nameof(PropertyTypeArchetypes.Reference));

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Integer),
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String),
                nameof(PropertyTypeArchetypes.Reference)
            },
            result.Select(pp => pp.Left.Name));

            Assert.Equal(new[]
            {
                nameof(PropertyTypeArchetypes.Struct),
                nameof(PropertyTypeArchetypes.Nullable),
                nameof(PropertyTypeArchetypes.String)
            },
            referenceProperty.NestedPropertyPairs.Select(pp => pp.Left.Name));
        }
    }
}