﻿using BenchmarkDotNet.Attributes;
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

        private readonly Func<Data, int> nativeGetter = d => d.Property;

        private readonly Func<Data, int> reflectionGetter = ReflectionGetterFactory.Of<Data, int>(i => i.Property);

        private readonly Func<Data, int> expressionGetter = ExpressionGetterFactory.Of<Data, int>(i => i.Property).Compile();

        private readonly Func<Data, int> dynamicGetter = DynamicMethodGetterFactory.Of<Data, int>(i => i.Property);

        [Benchmark]
        public void Get_property_value_with_reflection()
        {
            var result = this.reflectionGetter(this.data);
        }

        [Benchmark]
        public void Get_property_value_with_compiled_expression()
        {
            var result = this.expressionGetter(this.data);
        }

        [Benchmark]
        public void Get_property_value_with_dynamic_method()
        {
            var result = this.dynamicGetter(this.data);
        }

        [Benchmark]
        public void Get_property_value_directly()
        {
            var result = this.nativeGetter(this.data);
        }
    }
}