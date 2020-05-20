using Elementary.Properties.Selectors;
using System;
using System.Linq;
using Xunit;

namespace Elementary.Properties.Test.Selectors
{
    public class PropertyTest
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
        public void Property_retrieves_nested_info()
        {
            // ACT

            var result = Property<PropertyTypeArchetypes>.InfoPath(o => o.Reference.Integer).ToArray();

            // ASSERT

            Assert.Equal(2, result.Count());
            Assert.Equal(nameof(PropertyTypeArchetypes.Reference), result.First().Name);
            Assert.Equal(nameof(PropertyTypeArchetypes.Integer), result.Last().Name);
        }

        [Fact]
        public void Property_rejects_nested_info_if_only_single_info_is_expected()
        {
            // ACT & ASSERT

            var result = Assert.Throws<InvalidOperationException>(() => Property<PropertyTypeArchetypes>.Info(o => o.Reference.Integer));
        }
    }
}