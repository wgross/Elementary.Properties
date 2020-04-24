using BenchmarkDotNet.Attributes;
using Elementary.Properties.Setters;
using System;

namespace Elementary.Properties.Performance.Test
{
    public class SetterFactoryBenchmark
    {
        public class Data
        {
            public int Property { get; set; }
        }

        private readonly Data data = new Data { Property = 0 };

        private readonly Action<Data, int> reflectionSetter = ReflectionSetterFactory.Of<Data, int>(i => i.Property);

        private readonly Action<Data, int> expressionSetter = ExpressionSetterFactory.Of<Data, int>(i => i.Property).Compile();

        [Benchmark]
        public void Set_property_value_with_reflection()
        {
            this.reflectionSetter(data, 1);
        }

        [Benchmark]
        public void Set_property_value_with_compiled_expression()
        {
            this.expressionSetter(data, 1);
        }

        [Benchmark]
        public void Set_property_value_directly()
        {
            this.data.Property = 1;
        }
    }
}