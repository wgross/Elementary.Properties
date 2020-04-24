﻿using Elementary.Properties.Getters;
using Xunit;

namespace Elementary.Properties.Test.Getters
{
    public class ExpressionGetterFactoryTest
    {
        private class Data
        {
            public int IntegerPublicGetter { get; set; }

            public int IntegerProtectedGetter { protected get; set; }
        }

        [Fact]
        public void Getter_retrieves_public_property_value()
        {
            // ARRANGE

            var data = new Data { IntegerPublicGetter = 1 };
            var getter = ExpressionGetterFactory.Of<Data, int>(o => o.IntegerPublicGetter).Compile();

            // ACT

            var result = getter(data);

            // ASSERT

            Assert.Equal(1, result);
        }
    }
}