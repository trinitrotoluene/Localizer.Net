using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Localizer.Net.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    public class ScriptingBenchmarks
    {
        private ILocalization _localization;

        [GlobalSetup]
        public void BuildLocalization()
        {
            _localization = new LocalizationBuilder()
                .WithDefaultLocale("en-US")
                .WithPathSeparator(".")
                .UseJsonFiles("locales")
                .Build();
        }

        [Benchmark(Description = "Plaintext")]
        public string ReturnPlaintext()
        {
            return _localization.Resolve("en-US", "hello");
        }

        [Benchmark(Description = "Simple replacement")]
        public string ReturnSimpleReplacement()
        {
            return _localization.Resolve("en-US", "context-tests.test1", ("drill", "test"));
        }

        [Benchmark(Description = "Complex replacement")]
        public string ReturnComplexReplacement()
        {
            return _localization.Resolve("en-US", "context-tests.test3", ("isDrill", false), ("drill", "test"));
        }
    }
}
