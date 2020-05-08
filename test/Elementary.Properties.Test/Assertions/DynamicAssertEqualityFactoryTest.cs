using Elementary.Properties.Assertions;
using Xunit;

namespace Elementary.Properties.Test.Assertions
{
    public class DynamicAssertEqualityFactoryTest
    {
        public class Data1
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data1 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Data2
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void AssertValuesAreEqual_accepts_equal_values()
        {
            // ARRANGE

            var data1 = new Data1();
            var data2 = new Data2();
            var areEqual = DynamicAssertEqualityFactory.Of<Data1, Data2>();

            // ACT

            var result = areEqual(data1, data2);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void AssertValuesAreEqual_rejects_different_values()
        {
            // ARRANGE

            var data1 = new Data1 { Integer = 1 };
            var data2 = new Data2 { Integer = 2 };
            var areEqual = DynamicAssertEqualityFactory.Of<Data1, Data2>();

            // ACT

            var result = areEqual(data1, data2);

            // ASSERT

            Assert.False(result);
        }
    }
}