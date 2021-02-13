using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NNG
{
    class Generator
    {
        public string SourceRoot { get; private set; }
        public string OutputRoot { get; private set; }

        public IReadOnlyDictionary<string, IProcessor> Processors => _processors;

        readonly Dictionary<string, IProcessor> _processors = new Dictionary<string, IProcessor>();

        public Generator(string input, string output)
        {
            SourceRoot = input;
            OutputRoot = output;
            LoadBuiltInProcessors();
        }

        public void GenerateAllFiles()
        {
            ReadTemplates();
            GeneratePages(SourceRoot, OutputRoot);
        }

        private void LoadBuiltInProcessors()
        {
            // TODO
        }
        
        private void ReadTemplates()
        {
            foreach (string tFile in Directory.EnumerateFiles("*.template.xml"))
            {
                try
                {
                    TemplateProcessor tp = new TemplateProcessor(XmlReader.Create(tFile));
                    if(_processors.ContainsKey(tp.Name))
                    {
                        throw new GeneratorException(tFile, "Processor already exists: " + tp.Name);
                    }
                    _processors[tp.Name] = tp;
                    Console.WriteLine("Template generated: {0} -> {1}", tFile, tp.Name);
                }
                catch(Exception e)
                {
                    Console.WriteLine("{0} ERROR: {1}", tFile, e);
                }
            }
        }

        private void GeneratePages(string source, string output)
        {
            Console.WriteLine("{0} -> {1}", source, output);
            if(!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            foreach (string file in Directory.EnumerateFiles(source, "*.page.xml"))
            {
                ProcessFile(file);
            }

            foreach (string srcNext in Directory.EnumerateDirectories(source))
            {
                string head = Path.GetFileName(srcNext);
                string outNext = Path.Combine(output, head);
                GeneratePages(srcNext, outNext);
            }
        }

        private void ProcessFile(string file)
        {
            // TODO: implement
        }
    }
}
