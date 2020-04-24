using BenchmarkDotNet.Attributes;
using Elementary.Properties.Getters;
using System;

namespace Elementary.Properties.Performance.Test
{
    [CsvExporter, PlainExporter]
    public class GetterFactoryBenchmark
    {
        public class Data
        {
            public int Property { get; set; }
        }

        private readonly Data data = new Data { Property = 1 };

        private readonly Func<Data, int> reflectionGetter = ReflectionGetterFactory.Of<Data, int>(i => i.Property);

        [Benchmark]
        public void Get_property_value_with_reflection()
        {
            var result = reflectionGetter(data);
        }

        [Benchmark]
        public void Get_property_value_directly()
        {
            var result = this.data.Property;
        }
    }
}