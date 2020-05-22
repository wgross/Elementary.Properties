using Elementary.Properties.Mappers;
using Elementary.Properties.Selectors;
using Xunit;

namespace Elementary.Properties.Test.Mappers
{
    public class DynamicMapperFactoryTest
    {
        private class Source
        {
            public int IntegerPublic { get; set; }

            public int IntegerProtected { protected get; set; }

            public int IntegerPrivate { private get; set; }

            public Source ReferencePublic { get; set; }
        }

        private class Destination
        {
            public int IntegerPublic { get; set; }

            public int IntegerProtected { get; protected set; }

            public int IntegerPrivate { get; private set; }

            public Destination ReferencePublic { get; set; }
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

        //[Fact]
        //public void Map_public_property_with_custom_right_setter()
        //{
        //    // ARRANGE

        //    var source = new Source
        //    {
        //        IntegerPublic = 1,
        //        IntegerProtected = 2,
        //        IntegerPrivate = 3
        //    };
        //    var destination = new Destination();
        //    var mapperLeaf = DynamicMapperFactory.Of<Source, Destination>();
        //    var mapperInner = DynamicMapperFactory.Of<Source, Destination>(configure: c =>
        //    {
        //        c.IncludePair(Property<Source>.Info(s => s.ReferencePublic), (d, v) =>
        //        {
        //            d.ReferencePublic = new Destination();
        //            mapperLeaf((Source)v, d.ReferencePublic);
        //        });
        //    });

        //    // ACT

        //    mapperInner(source, destination);

        //    // ASSERT

        //    Assert.Equal(1, destination.IntegerPublic);
        //    Assert.Equal(2, destination.IntegerProtected);
        //    Assert.Equal(3, destination.IntegerPrivate);
        //}
    }
}