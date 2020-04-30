using Elementary.Properties.Mappers;
using Xunit;

namespace Elementary.Properties.Test.Mappers
{
    public class DynamicMethodMapperFactoryTest
    {
        private class Source
        {
            public int IntegerPublic { get; set; }

            public int IntegerProtected { protected get; set; }

            public int IntegerPrivate { private get; set; }
        }

        private class Destination
        {
            public int IntegerPublic { get; set; }

            public int IntegerProtected { get; protected set; }

            public int IntegerPrivate { get; private set; }
        }

        [Fact]
        public void Map_public_property_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source { IntegerPublic = 1 };
            var destination = new Destination();
            var mapper = DynamicMethodMapperFactory.Of<Source, Destination>(new[] { nameof(Source.IntegerPublic) });

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.Equal(1, destination.IntegerPublic);
        }

        [Fact]
        public void Map_protected_property_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source { IntegerProtected = 1 };
            var destination = new Destination();
            var mapper = DynamicMethodMapperFactory.Of<Source, Destination>(new[] { nameof(Source.IntegerProtected) });

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.Equal(1, destination.IntegerProtected);
        }

        [Fact]
        public void Map_private_property_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source { IntegerPrivate = 1 };
            var destination = new Destination();
            var mapper = DynamicMethodMapperFactory.Of<Source, Destination>(new[] { nameof(Source.IntegerPrivate) });

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.Equal(1, destination.IntegerPrivate);
        }

        [Fact]
        public void Map_multiple_properties_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source
            {
                IntegerPublic = 1,
                IntegerProtected = 2,
                IntegerPrivate = 3
            };
            var destination = new Destination();
            var mapper = DynamicMethodMapperFactory.Of<Source, Destination>(new[] { nameof(Source.IntegerPublic), nameof(Source.IntegerProtected), nameof(Source.IntegerPrivate) });

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.Equal(1, destination.IntegerPublic);
            Assert.Equal(2, destination.IntegerProtected);
            Assert.Equal(3, destination.IntegerPrivate);
        }
    }
}