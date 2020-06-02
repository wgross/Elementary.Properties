using AutoMapper;
using BenchmarkDotNet.Attributes;
using Elementary.Properties.Mappers;
using System;

namespace Elementary.Properties.Performance.Test
{
    [CsvExporter, PlainExporter]
    public class DynamicMapperBenchmark
    {
        #region Classes

        public class Flat50PropertiesSource
        {
            public int Property1 { get; set; } = 1;
            public int Property2 { get; set; } = 2;
            public int Property3 { get; set; } = 3;
            public int Property4 { get; set; } = 4;
            public int Property5 { get; set; } = 5;
            public int Property6 { get; set; } = 6;
            public int Property7 { get; set; } = 7;
            public int Property8 { get; set; } = 8;
            public int Property9 { get; set; } = 9;
            public int Property10 { get; set; } = 10;
            public int Property11 { get; set; } = 11;
            public int Property12 { get; set; } = 12;
            public int Property13 { get; set; } = 13;
            public int Property14 { get; set; } = 14;
            public int Property15 { get; set; } = 15;
            public int Property16 { get; set; } = 16;
            public int Property17 { get; set; } = 17;
            public int Property18 { get; set; } = 18;
            public int Property19 { get; set; } = 19;
            public int Property20 { get; set; } = 20;
            public int Property21 { get; set; } = 21;
            public int Property22 { get; set; } = 22;
            public int Property23 { get; set; } = 23;
            public int Property24 { get; set; } = 24;
            public int Property25 { get; set; } = 25;
            public int Property26 { get; set; } = 26;
            public int Property27 { get; set; } = 27;
            public int Property28 { get; set; } = 28;
            public int Property29 { get; set; } = 29;
            public int Property30 { get; set; } = 30;
            public int Property31 { get; set; } = 31;
            public int Property32 { get; set; } = 32;
            public int Property33 { get; set; } = 33;
            public int Property34 { get; set; } = 34;
            public int Property35 { get; set; } = 35;
            public int Property36 { get; set; } = 36;
            public int Property37 { get; set; } = 37;
            public int Property38 { get; set; } = 38;
            public int Property39 { get; set; } = 39;
            public int Property40 { get; set; } = 40;
            public int Property41 { get; set; } = 41;
            public int Property42 { get; set; } = 42;
            public int Property43 { get; set; } = 43;
            public int Property44 { get; set; } = 44;
            public int Property45 { get; set; } = 45;
            public int Property46 { get; set; } = 46;
            public int Property47 { get; set; } = 47;
            public int Property48 { get; set; } = 48;
            public int Property49 { get; set; } = 49;
            public int Property50 { get; set; } = 50;
        }

        public class Flat50PropertiesDestination
        {
            public int Property1 { get; set; }
            public int Property2 { get; set; }
            public int Property3 { get; set; }
            public int Property4 { get; set; }
            public int Property5 { get; set; }
            public int Property6 { get; set; }
            public int Property7 { get; set; }
            public int Property8 { get; set; }
            public int Property9 { get; set; }
            public int Property10 { get; set; }
            public int Property11 { get; set; }
            public int Property12 { get; set; }
            public int Property13 { get; set; }
            public int Property14 { get; set; }
            public int Property15 { get; set; }
            public int Property16 { get; set; }
            public int Property17 { get; set; }
            public int Property18 { get; set; }
            public int Property19 { get; set; }
            public int Property20 { get; set; }
            public int Property21 { get; set; }
            public int Property22 { get; set; }
            public int Property23 { get; set; }
            public int Property24 { get; set; }
            public int Property25 { get; set; }
            public int Property26 { get; set; }
            public int Property27 { get; set; }
            public int Property28 { get; set; }
            public int Property29 { get; set; }
            public int Property30 { get; set; }
            public int Property31 { get; set; }
            public int Property32 { get; set; }
            public int Property33 { get; set; }
            public int Property34 { get; set; }
            public int Property35 { get; set; }
            public int Property36 { get; set; }
            public int Property37 { get; set; }
            public int Property38 { get; set; }
            public int Property39 { get; set; }
            public int Property40 { get; set; }
            public int Property41 { get; set; }
            public int Property42 { get; set; }
            public int Property43 { get; set; }
            public int Property44 { get; set; }
            public int Property45 { get; set; }
            public int Property46 { get; set; }
            public int Property47 { get; set; }
            public int Property48 { get; set; }
            public int Property49 { get; set; }
            public int Property50 { get; set; }
        }

        #endregion Classes

        private Action<Flat50PropertiesSource, Flat50PropertiesDestination> dynamicMapper = DynamicMapperFactory.Of<Flat50PropertiesSource, Flat50PropertiesDestination>();
        private IMapper autoMapper;
        private readonly Flat50PropertiesSource source = new Flat50PropertiesSource();

        private readonly Flat50PropertiesDestination destination = new Flat50PropertiesDestination();

        [GlobalSetup]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Flat50PropertiesSource, Flat50PropertiesDestination>();
            });

            this.autoMapper = config.CreateMapper();
        }

        [Benchmark]
        public void Map_with_DynamicMapper()
        {
            this.dynamicMapper(this.source, this.destination);
        }

        [Benchmark]
        public void Map_with_AutoMapper()
        {
            this.autoMapper.Map(this.source, this.destination);
        }
    }
}