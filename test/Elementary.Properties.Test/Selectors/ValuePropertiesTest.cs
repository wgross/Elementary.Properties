using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;
using static Elementary.Properties.Selectors.PropertyInfos;

namespace Elementary.Properties.Test.Selectors
{
    public class ValuePropertiesTest
    {
        #region Verify Type Archetypes

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
        public void ValueProperties_accepts_valueProperties_and_strings()
        {
            // ACT

            var result = ValueProperties.All<PropertyTypeArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String" }, result.Select(pi => pi.Name));
        }

        #endregion Verify Type Archetypes

        #region Verify Accessor Archetypes

        public class AccessorArchetypes
        {
            private string missingGetter;

            public int Public { get; set; }

            protected Guid Protected { get; set; }

            private DateTime? Private { get; set; }

            public string MissingGetter { set => this.missingGetter = value; }

            public string MissingSetter => "getter";
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_read()
        {
            // ACT

            var result = ValueProperties.AllCanRead<AccessorArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingSetter" }, result.Select(pi => pi.Name));
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_write()
        {
            // ACT

            var result = ValueProperties.AllCanWrite<AccessorArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingGetter" }, result.Select(pi => pi.Name));
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_read_and_write()
        {
            // ACT

            var result = ValueProperties.AllCanReadAndWrite<AccessorArchetypes>();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private" }, result.Select(pi => pi.Name));
        }

        [Fact]
        public void ValueProperties_joins_compatible_properties()
        {
            // ACT

            var result = ValueProperties.Join(
                ValueProperties.All<PropertyTypeArchetypes>(),
                ValueProperties.All<PropertyTypeArchetypes>()
            ).ToArray();

            // ASSERT

            Assert.Equal(4, result.Length);
        }

        #endregion Verify Accessor Archetypes

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
                ValueProperties.All<PropertyTypeArchetypes>(),
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
                ValueProperties.All<PropertyTypeArchetypes>(),
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
                ValueProperties.All<PropertyTypeArchetypes>(),
                ValueProperties.All<DifferentType>(),
                (error, p) => recordedError = (error, p)
            ).ToArray();

            // ASSERT

            Assert.Equal(JoinError.RightPropertyType, recordedError.error);
            Assert.Equal("Integer", recordedError.p.name);
            Assert.Equal(typeof(int), recordedError.p.propertyType);
        }

     
    }
}