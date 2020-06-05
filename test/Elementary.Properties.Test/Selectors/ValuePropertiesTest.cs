using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;

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

            var result = ValueProperty<PropertyTypeArchetypes>.AllCanReadAndWrite();

            // ASSERT

            Assert.Equal(new[] { "Integer", "Struct", "Nullable", "String" }, result.Select(pi => pi.Info.Name));
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

            var result = ValueProperty<AccessorArchetypes>.AllCanRead();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingSetter" }, result.Select(pi => pi.Info.Name));
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_write()
        {
            // ACT

            var result = ValueProperty<AccessorArchetypes>.AllCanWrite();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private", "MissingGetter" }, result.Select(pi => pi.Info.Name));
        }

        [Fact]
        public void ValueProperties_accepts_valueProperties_and_strings_which_can_read_and_write()
        {
            // ACT

            var result = ValueProperty<AccessorArchetypes>.AllCanReadAndWrite();

            // ASSERT

            Assert.Equal(new[] { "Public", "Protected", "Private" }, result.Select(pi => pi.Info.Name));
        }

        #endregion Verify Accessor Archetypes
    }
}