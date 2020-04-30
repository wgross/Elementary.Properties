using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elementary.Properties.Test.Mappers
{
    public class MappingBuilderTest
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

            public int IntegerProtected { protected get; set; }

            public int IntegerPrivate { private get; set; }
        }

        public static IEnumerable<object[]> ArrangeIncludeProperty()
        {
            yield return new object[]
            {
                new MappingBuilder<Source, Destination>()
                    .Include(s => s.IntegerPublic)
                    .Build()
                    .ToArray()
            };
            yield return new object[]
            {
                new MappingBuilder<Source, Destination>()
                    .Include(nameof(Source.IntegerPublic))
                    .Build()
                    .ToArray()
            };
        }

        [Theory]
        [MemberData(nameof(ArrangeIncludeProperty))]
        public void MappingBuilder_fills_in_Mapping_data_for_single_property(IEnumerable<Mapping> mapping)
        {
            // ASSERT

            Assert.Single(mapping);
            Assert.Equal(typeof(Source).GetProperty(nameof(Source.IntegerPublic)).GetGetMethod(), mapping.Single().SourceGetter);
            Assert.Equal(typeof(Destination).GetProperty(nameof(Source.IntegerPublic)).GetSetMethod(), mapping.Single().DestinationSetter);
        }

        [Fact]
        public void MapppingBuilder_excludes_duplicate_includes()
        {
            // ARRANGE

            var builder = new MappingBuilder<Source, Destination>()
                .IncludeValueProperties()
                .Include(s => s.IntegerPublic)
                .Include(nameof(Source.IntegerPublic));

            // ACT

            var result = builder.Build().ToArray();

            // ASSERT

            Assert.Equal(3, result.Length);
        }

        private class DifferentMemberTypes
        {
            public int ValueType { get; set; }

            public int[] CollectionType { get; set; }

            public string StringType { get; set; }

            public Source ReferenceType { get; set; }
        }

        [Fact]
        public void MappingData_selects_ValueType_properties()
        {
            // ACT

            var result = new MappingBuilder<DifferentMemberTypes, DifferentMemberTypes>().IncludeValueProperties().Build().ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.ValueType)).GetGetMethod(), result.First().SourceGetter);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.ValueType)).GetSetMethod(), result.First().DestinationSetter);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.StringType)).GetGetMethod(), result.Last().SourceGetter);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.StringType)).GetSetMethod(), result.Last().DestinationSetter);
        }

        public static IEnumerable<object[]> ArrangeExcludeProperty()
        {
            yield return new object[]
            {
                new MappingBuilder<DifferentMemberTypes, DifferentMemberTypes>()
                    .IncludeValueProperties()
                    .Exclude(s => s.ValueType)
                    .Build()
                    .ToArray()
            };
            yield return new object[]
            {
                new MappingBuilder<DifferentMemberTypes, DifferentMemberTypes>()
                    .IncludeValueProperties()
                    .Exclude(s => s.ValueType)
                    .Build()
                    .ToArray()
            };
        }

        [Theory]
        [MemberData(nameof(ArrangeExcludeProperty))]
        public void MappingBuilder_excludes_specified_property(IEnumerable<Mapping> mapping)
        {
            // ASSERT

            Assert.Single(mapping);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.StringType)).GetGetMethod(), mapping.Single().SourceGetter);
            Assert.Equal(typeof(DifferentMemberTypes).GetProperty(nameof(DifferentMemberTypes.StringType)).GetSetMethod(), mapping.Single().DestinationSetter);
        }
    }
}