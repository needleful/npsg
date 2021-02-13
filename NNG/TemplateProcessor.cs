using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NNG
{
    class TemplateProcessor : IProcessor
    {
        public string Name { get; private set; }

        public XmlReader Reader => throw new NotImplementedException();

        public IProcessor Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TemplateProcessor(Generator generator, XmlValidatingReader reader)
        {
            Stack<string> elems = new Stack<string>(16);
            while(reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if(elems.Count == 0)
                        {
                            if(reader.Name != "template")
                            {
                                throw new TemplateParseException(
                                    reader.LineNumber, reader.LinePosition,
                                    string.Format("Expected 'template' as top level element, got {0}", reader.Name));
                            }
                            Name = reader.GetAttribute("name");
                        }
                        else
                        {
                            elems.Push(reader.Name);
                        }
                        if(reader.Name == "param")
                        {
                            GetParam(reader);
                        }
                        break;
                }
            }
        }
        
        private void GetParam(XmlReader reader)
        {

        }

        public bool Read()
        {
            throw new NotImplementedException();
        }
    }

    public class TemplateParseException : Exception
    {
        public TemplateParseException(int line, int col, string context)
            : base(string.Format("{0}. Line {1}, Column {2}", context, line, col))
        {
        }
    }
}
