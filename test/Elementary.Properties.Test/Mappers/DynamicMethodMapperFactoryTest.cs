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
        public void Map_public_properties_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source
            {
                IntegerPublic = 1,
                IntegerProtected = 2,
                IntegerPrivate = 3
            };
            var destination = new Destination();
            var mapper = DynamicMapperFactory.Of<Source, Destination>();

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.Equal(1, destination.IntegerPublic);
            Assert.Equal(2, destination.IntegerProtected);
            Assert.Equal(3, destination.IntegerPrivate);
        }
    }
}