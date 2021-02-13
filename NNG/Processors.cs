using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NNG
{
    public interface IProcessor
    {
        string Name { get; }
        XmlReader Reader { get; }
        IProcessor Source { get; set; }

        bool Read();
    }

    public class FileProcessor : IProcessor
    {
        public string Name { get; private set; }
        public XmlReader Reader { get; private set; }
        public IProcessor Source
        {
            get => null;
            set => throw new NotImplementedException();
        }

        public FileProcessor(string file)
        {
            Name = "file://" + file;
            Reader = XmlReader.Create(file);
        }

        public bool Read()
        {
            return Reader.Read();
        }
    }
}
