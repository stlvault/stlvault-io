using System;
using System.IO;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using StlVault.IO;
using StlVault.IO.Stl;

namespace Benchmarks
{
    [SimpleJob(RunStrategy.ColdStart, targetCount: 6, invocationCount: 15)]
    [HtmlExporter]
    [MemoryDiagnoser]
    public class ParserBenchmarks
    {
        private ArrayPoolBufferFactory BufferFactory { get; set; }

        [Params("bin-030mb", "bin-350mb", "txt-020mb", "txt-220mb")]
        public string File { get; set; }
        
        [Benchmark]
        public IMeshBuffer ArrayBuffer()
        {
            var file = GetFileName();
            var buffer = StlImporter.ImportMesh(file, BufferFactory);
            buffer.Dispose();
            
            return buffer;
        }

        private string GetFileName([CallerFilePath] string filePath = null)
        {
            var fileDir = Path.GetDirectoryName(filePath);
            return Path.Combine(fileDir, "data", File + ".stl");
        }

        [GlobalSetup]
        public void Setup()
        {
            BufferFactory = new ArrayPoolBufferFactory();
        }
    }
}
