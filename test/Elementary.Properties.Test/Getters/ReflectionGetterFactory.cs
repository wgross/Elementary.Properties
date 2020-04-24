using System;
using Xunit;

namespace Elementary.Properties.Test
{
    public class ReflectionGetterFactoryTest
    {
        class Data
        {
            public int IntegerGetter { get; set; }
        }

        [Fact]
        public void Getter_retrieves_public_property_value()
        {
            // ARRANGE

            var getter = ReflectionGetterFactory.Of<Data>(o => o.IntegerGetter)
            // ACT



        }

    }
}
