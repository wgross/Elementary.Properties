using Elementary.Properties.Assertions;
using Xunit;

namespace Elementary.Properties.Test.Assertions
{
    public class DynamicAssertInitializedFactoryTest
    {
        public class Data1
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data1 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void DynamicAssertInitializedFactory_accepts_initialized_values_of_class()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>();

            // ACT

            var result = assertInitialized(new Data1());

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_rejects_defaultOfT()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>();

            // ACT

            var result = assertInitialized(new Data1());

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_rejects_defaultOfT_if_included_reference()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = string.Empty,
                Reference = null // <-- default
            });

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_accepts_initialized_values_in_nested_class()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = string.Empty,
                Reference = new Data1
                {
                    Integer = 1,
                    String = string.Empty
                }
            });

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_rejects_defaultOfT_in_nested_class()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = string.Empty,
                Reference = new Data1
                {
                    Integer = 0, // <-- default
                    String = string.Empty
                }
            });

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_rejects_defaultOfT_if_included_reference_2_level_nested()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference, i => i.IncludeValuesOf(i => i.Reference)));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = "1",
                Reference = new Data1
                {
                    Integer = 2,
                    String = "2",
                    Reference = null // <-- default
                }
            });

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_accepts_defaultOfT_if_excluded_in_nested_class()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference, i => i.Exclude(nameof(Data1.String))));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = "1",
                Reference = new Data1
                {
                    Integer = 2,
                    String = null, // <-- default but excluded
                    Reference = null // <-- default, but not included,
                }
            });

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void DynamicAssertInitializedFactory_accepts_included_reference_2_level_nested()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>(c => c.IncludeValuesOf(d => d.Reference, i => i.IncludeValuesOf(i => i.Reference)));

            // ACT

            var result = assertInitialized(new Data1
            {
                Integer = 1,
                String = "1",
                Reference = new Data1
                {
                    Integer = 2,
                    String = "2",
                    Reference = new Data1
                    {
                        Integer = 3,
                        String = "3",
                        Reference = null // <-- default but not included
                    }
                }
            });

            // ASSERT

            Assert.True(result);
        }
    }
}