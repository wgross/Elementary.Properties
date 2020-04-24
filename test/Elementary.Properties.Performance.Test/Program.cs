using BenchmarkDotNet.Running;

namespace Elementary.Properties.Performance.Test
{
    internal class Program
    {
        public static void Main(string[] args) => BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}