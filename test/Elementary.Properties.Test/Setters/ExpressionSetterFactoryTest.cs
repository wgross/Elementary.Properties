﻿using Elementary.Properties.Setters;
using Xunit;

namespace Elementary.Properties.Test.Setters
{
    public class ExpressionSetterFactoryTest
    {
        private class Data
        {
            public int PublicIntegerPublicSetter { get; set; }

            public int PublicIntegerProtectedSetter { get; protected set; }

            public int PublicIntegerPrivateSetter { get; private set; }
        }

        [Fact]
        public void Setter_writes_public_property_value()
        {
            // ARRANGE

            var data = new Data { PublicIntegerPublicSetter = 0 };
            var setter = ExpressionSetterFactory.Of<Data, int>(o => o.PublicIntegerPublicSetter).Compile();

            // ACT

            setter(data, 1);

            // ASSERT

            Assert.Equal(1, data.PublicIntegerPublicSetter);
        }

        [Fact]
        public void Setter_writes_protected_property_value()
        {
            // ARRANGE

            var data = new Data { PublicIntegerPublicSetter = 0 };
            var setter = ExpressionSetterFactory.Of<Data, int>(o => o.PublicIntegerProtectedSetter).Compile();

            // ACT

            setter(data, 1);

            // ASSERT

            Assert.Equal(1, data.PublicIntegerProtectedSetter);
        }

        [Fact]
        public void Setter_writes_private_property_value()
        {
            // ARRANGE

            var data = new Data { PublicIntegerPublicSetter = 0 };
            var setter = ExpressionSetterFactory.Of<Data, int>(o => o.PublicIntegerPrivateSetter).Compile();

            // ACT

            setter(data, 1);

            // ASSERT

            Assert.Equal(1, data.PublicIntegerPrivateSetter);
        }
    }
}