using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using HandlebarsDotNet;

namespace HandlebarsNet.Benchmark
{
    public class LargeArray
    {
        private object _data;
        private HandlebarsTemplate<TextWriter, object, object> _default;

        [Params(20000, 40000, 80000)]
        public int N { get; set; }
        
        [GlobalSetup]
        public void Setup()
        {
            const string template = @"{{#each this}}{{this}}{{/each}}";

            var handlebars = Handlebars.Create();

            using (var reader = new StringReader(template))
            {
                _default = handlebars.Compile(reader);
            }

            _data = Enumerable.Range(0, N).ToList();
        }
        
        [Benchmark]
        public void Default() => _default(TextWriter.Null, _data);
    }
}