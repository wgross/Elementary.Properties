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

        //private readonly Func<Data, int> expressionGetter = ExpressionGetterFactory.Of<Data, int>(i => i.Property).Compile();

        [Benchmark]
        public void Set_property_value_with_reflection()
        {
            this.reflectionSetter(data, 1);
        }

        //[Benchmark]
        //public void Get_property_value_with_compiled_expression()
        //{
        //    var result = this.expressionGetter(data);
        //}

        [Benchmark]
        public void Set_property_value_directly()
        {
            this.data.Property = 1;
        }
    }
}