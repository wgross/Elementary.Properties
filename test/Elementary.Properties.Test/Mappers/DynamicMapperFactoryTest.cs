using Elementary.Properties.Assertions;
using Elementary.Properties.Mappers;
using System;
using Xunit;

namespace Elementary.Properties.Test.Mappers
{
    public class DynamicMapperFactoryTest
    {
        public class Source
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Source2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Source2
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Source3 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Source3
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Source Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Destination
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Destination2 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Destination2
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Destination3 Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Destination3
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Destination Reference { set; get; }

            public int[] Collection { get; set; }
        }

        private readonly Func<Source, Destination, bool> assertEquals = DynamicAssertEqualityFactory.Of<Source, Destination>(c =>
        {
            c.IncludeNested(l => l.Reference);
        });

        [Fact]
        public void Map_properties_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2"
            };
            var destination = new Destination();
            var mapper = DynamicMapperFactory.Of<Source, Destination>();

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);

            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);
        }

        [Fact]
        public void Map_nested_properties_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4"
                }
            };
            var destination = new Destination
            {
                Reference = new Destination2()
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Equal(3, source.Reference.Integer);
            Assert.Equal("4", source.Reference.String);
            Assert.Equal(3, destination.Reference.Integer);
            Assert.Equal("4", destination.Reference.String);
        }

        [Fact]
        public void Map_nested_properties_both_null()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = null
            };
            var destination = new Destination
            {
                Reference = null
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);

            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Null(source.Reference);
            Assert.Null(destination.Reference);
        }

        [Fact]
        public void Map_nested_left_null_instance()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2"
            };
            var destination = new Destination
            {
                Reference = new Destination2()
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Null(source.Reference);
            Assert.Null(destination.Reference);
        }

        [Fact]
        public void Map_nested_right_null_instance()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4"
                }
            };
            var destination = new Destination
            {
                Reference = null
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Equal(3, source.Reference.Integer);
            Assert.Equal("4", source.Reference.String);

            Assert.Equal(3, destination.Reference.Integer);
            Assert.Equal("4", destination.Reference.String);
        }

        [Fact]
        public void Map_nested2_properties_of_same_name_and_type()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4",
                    Reference = new Source3
                    {
                        Integer = 5,
                        String = "6"
                    }
                }
            };
            var destination = new Destination
            {
                Reference = new Destination2
                {
                    Reference = new Destination3()
                }
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Equal(3, source.Reference.Integer);
            Assert.Equal("4", source.Reference.String);
            Assert.Equal(3, destination.Reference.Integer);
            Assert.Equal("4", destination.Reference.String);

            Assert.Equal(5, source.Reference.Reference.Integer);
            Assert.Equal("6", source.Reference.Reference.String);
            Assert.Equal(5, destination.Reference.Reference.Integer);
            Assert.Equal("6", destination.Reference.Reference.String);
        }

        [Fact]
        public void Map_nested2_right_null_instance()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4",
                    Reference = new Source3
                    {
                        Integer = 5,
                        String = "6"
                    }
                }
            };
            var destination = new Destination
            {
                Reference = new Destination2
                {
                    Reference = null
                }
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Equal(3, source.Reference.Integer);
            Assert.Equal("4", source.Reference.String);
            Assert.Equal(3, destination.Reference.Integer);
            Assert.Equal("4", destination.Reference.String);

            Assert.Equal(5, source.Reference.Reference.Integer);
            Assert.Equal("6", source.Reference.Reference.String);
            Assert.Equal(5, destination.Reference.Reference.Integer);
            Assert.Equal("6", destination.Reference.Reference.String);
        }

        [Fact]
        public void Map_nested2_left_null_instance()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4",
                    Reference = null
                }
            };
            var destination = new Destination
            {
                Reference = new Destination2
                {
                    Reference = new Destination3()
                }
            };
            var mapper = DynamicMapperFactory.Of<Source, Destination>(c => c.IncludeNested(l => l.Reference.Reference));

            // ACT

            mapper(source, destination);

            // ASSERT

            Assert.True(assertEquals(source, destination));

            Assert.Equal(1, source.Integer);
            Assert.Equal("2", source.String);
            Assert.Equal(1, destination.Integer);
            Assert.Equal("2", destination.String);

            Assert.Equal(3, source.Reference.Integer);
            Assert.Equal("4", source.Reference.String);
            Assert.Equal(3, destination.Reference.Integer);
            Assert.Equal("4", destination.Reference.String);

            Assert.Null(source.Reference.Reference);
            Assert.Null(destination.Reference.Reference);
        }

        public class Destination_NoCtor
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Destination2_NoCtor Reference { set; get; }

            public int[] Collection { get; set; }
        }

        public class Destination2_NoCtor
        {
            public Destination2_NoCtor(int i)
            {
                this.Integer = i;
            }

            public int Integer { get; set; }

            public string String { get; set; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void Map_nested_right_null_instance_without_default_ctor()
        {
            // ARRANGE

            var source = new Source
            {
                Integer = 1,
                String = "2",
                Reference = new Source2
                {
                    Integer = 3,
                    String = "4"
                }
            };
            var destination = new Destination_NoCtor
            {
                Reference = null
            };

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => DynamicMapperFactory.Of<Source, Destination_NoCtor>(c => c.IncludeNested(l => l.Reference)));

            // ASSERT

            Assert.Equal("Right property type(name='Destination2_NoCtor' requires default ctor", result.Message);
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