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

        private readonly Action<Data, int> nativeSetter = (i, v) => i.Property = v;

        private readonly Action<Data, int> reflectionSetter = ReflectionSetterFactory.Of<Data, int>(i => i.Property);

        private readonly Action<Data, int> expressionSetter = ExpressionSetterFactory.Of<Data, int>(i => i.Property).Compile();

        private readonly Action<Data, int> dynamicSetter = DynamicMethodSetterFactory.Of<Data, int>(i => i.Property);

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
        public void Set_property_value_with_dynamic_method()
        {
            this.dynamicSetter(data, 1);
        }

        [Benchmark]
        public void Set_property_value_directly()
        {
            this.nativeSetter(this.data, 1);
        }
    }
}