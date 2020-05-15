using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;
using static Elementary.Properties.Selectors.PropertyInfos;

namespace Elementary.Properties.Test.Selectors
{
    public class ValuePropertiesTest
    {
        public class PropertyArchetypes
        {
            public int Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyArchetypes Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class AccessorArchetypes
        {
            private string missingGetter;

            public int Public { get; set; }

            protected Guid Protected { get; set; }

            private DateTime? Private { get; set; }

            public string MissingGetter { set => this.missingGetter = value; }
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings()
        {
            // ACT

            var result = ValueProperties.All<PropertyArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String" }, result.Select(pi => pi.Name));
        }

        [Fact]
        public void ValuePorperties_excludes_property_by_name()
        {
            // ACT

            var result = ValueProperties.All<PropertyArchetypes>(c => c.Exclude(nameof(PropertyArchetypes.Integer)));

            // ASSERT

            Assert.Equal(new[] { "Struct", "Nullable", "String" }, result.Select(pi => pi.Name).ToArray());
        }

        [Fact]
        public void ValuePorperties_includes_references_and_collections_by_name()
        {
            // ACT

            var result = ValueProperties.All<PropertyArchetypes>(c =>
            {
                c.Include(PropertyFromMemberAccess<PropertyArchetypes>(p => p.Reference));
                c.Include(PropertyFromMemberAccess<PropertyArchetypes>(p => p.Collection));
            });

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String", "Reference", "Collection" }, result.Select(pi => pi.Name).ToArray());
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_read()
        {
            // ACT

            var result = ValueProperties.AllCanRead<AccessorArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private" }, result.Select(pi => pi.Name));
        }

        [Fact]
        public void ValueProperties_joins_compatible_properties()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyArchetypes>(),
                ValueProperties.All<PropertyArchetypes>()
            ).ToArray();

            // ASSERT

            Assert.Equal(4, result.Length);
        }

        public class MissingProperty
        {
            public int Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_compatible_properties_notifies_on_missing_right_property()
        {
            // ACT

            (JoinError error, (string name, Type propertyType) p) recordedError = (JoinError.RightPropertyMissing, (null, null));
            var result = ValueProperties.Join(
                ValueProperties.All<PropertyArchetypes>(),
                ValueProperties.All<MissingProperty>(),
                (error, p) => recordedError = (error, p)
            ).ToArray();

            // ASSERT

            Assert.Equal(JoinError.RightPropertyMissing, recordedError.error);
            Assert.Equal("String", recordedError.p.name);
            Assert.Equal(typeof(string), recordedError.p.propertyType);
        }

        [Fact]
        public void ValueProperties_joining_compatible_properties_notifies_on_missing_left_property()
        {
            // ACT

            (JoinError error, (string name, Type propertyType) p) recordedError = (JoinError.RightPropertyMissing, (null, null));
            var result = ValueProperties.Join(
                ValueProperties.All<MissingProperty>(),
                ValueProperties.All<PropertyArchetypes>(),
                (error, p) => recordedError = (error, p)
            ).ToArray();

            // ASSERT

            Assert.Equal(JoinError.LeftPropertyMissing, recordedError.error);
            Assert.Equal("String", recordedError.p.name);
            Assert.Equal(typeof(string), recordedError.p.propertyType);
        }

        public class DifferentType
        {
            public long Integer { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_compatible_properties_notifies_on_different_right_property_type()
        {
            // ACT

            (JoinError error, (string name, Type propertyType) p) recordedError = (JoinError.RightPropertyMissing, (null, null));
            var result = ValueProperties.Join(
                ValueProperties.All<PropertyArchetypes>(),
                ValueProperties.All<DifferentType>(),
                (error, p) => recordedError = (error, p)
            ).ToArray();

            // ASSERT

            Assert.Equal(JoinError.RightPropertyType, recordedError.error);
            Assert.Equal("Integer", recordedError.p.name);
            Assert.Equal(typeof(int), recordedError.p.propertyType);
        }

        [Fact]
        public void ValueProperties_joining_excludes_properties()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyArchetypes>(),
                ValueProperties.All<PropertyArchetypes>(),
            configure: cfg => cfg.ExcludeLeft(nameof(PropertyArchetypes.Integer), nameof(PropertyArchetypes.Nullable))).ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
            Assert.Equal(new[]
                {
                    PropertyFromMemberAccess<PropertyArchetypes>(p => p.Struct),
                    PropertyFromMemberAccess<PropertyArchetypes>(p => p.String)
                },
                result.Select(pp => pp.Left).ToArray());
        }

        public class DifferentNames
        {
            public int AlsoInteger { get; set; }

            public Guid Struct { get; set; }

            public DateTime? Nullable { get; set; }

            public string String { get; set; }

            public PropertyArchetypes Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void ValueProperties_joining_overrides_property_pair()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyArchetypes>(),
                ValueProperties.All<DifferentNames>(),
                configure: cfg => cfg.OverridePair(
                    ValueProperties.Single<PropertyArchetypes>(p => p.Integer),
                    ValueProperties.Single<DifferentNames>(p => p.AlsoInteger)))
                .ToArray();

            // ASSERT

            Assert.Equal(4, result.Length);
        }
    }
}