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
        public void DynamicAssertInitializedFactory_rejects_defaultOfT()
        {
            // ARRANGE

            var assertInitialized = DynamicAssertInitializedFactory.Of<Data1>();

            // ACT

            var result = assertInitialized(new Data1());

            // ASSERT

            Assert.False(result);
        }
    }
}