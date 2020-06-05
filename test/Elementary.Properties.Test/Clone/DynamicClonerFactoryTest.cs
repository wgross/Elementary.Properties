using Elementary.Properties.Assertions;
using Elementary.Properties.Cloners;
using Xunit;

namespace Elementary.Properties.Test.Clone
{
    public class DynamicClonerFactoryTest
    {
        public class Data1
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Data2
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data1 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void DynamicClonerFactory_creates_shallow_copy()
        {
            // ARRANGE

            var assertEquals = DynamicAssertEqualityFactory.Of<Data1, Data1>();
            var clone = DynamicClonerFactory.Of<Data1>();
            var source = new Data1
            {
                Integer = 1,
                String = "2",
                Reference = new Data2()
            };

            // ACT

            var result = clone(source);

            // ASSERT

            Assert.NotNull(result);
            Assert.NotSame(source, result);
            Assert.True(assertEquals(source, result));
            Assert.Null(result.Reference);
        }

        [Fact]
        public void DynamicClonerFactory_creates_nested_copy()
        {
            // ARRANGE

            var assertEquals = DynamicAssertEqualityFactory.Of<Data1, Data1>(c => c.IncludeNested(d => d.Reference));
            var clone = DynamicClonerFactory.Of<Data1>(c => c.IncludeNested(d => d.Reference));
            var source = new Data1
            {
                Integer = 1,
                String = "2",
                Reference = new Data2
                {
                    Integer = 3,
                    String = "4"
                }
            };

            // ACT

            var result = clone(source);

            // ASSERT

            Assert.NotNull(result);
            Assert.NotSame(source, result);
            Assert.NotSame(result.Reference, source.Reference);
            Assert.True(assertEquals(source, result));
        }

        [Fact]
        public void DynamicClonerFactory_creates_nested_copy_with_null()
        {
            // ARRANGE

            var assertEquals = DynamicAssertEqualityFactory.Of<Data1, Data1>(c => c.IncludeNested(d => d.Reference));
            var clone = DynamicClonerFactory.Of<Data1>(c => c.IncludeNested(d => d.Reference));
            var source = new Data1
            {
                Integer = 1,
                String = "2",
                Reference = null
            };

            // ACT

            var result = clone(source);

            // ASSERT

            Assert.NotNull(result);
            Assert.NotSame(source, result);
            Assert.Null(result.Reference);
            Assert.True(assertEquals(source, result));
        }
    }
}