using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public interface ITemplateValue
    {
        string ToString();
    }

    public class TextValue : ITemplateValue
    {
        protected readonly string _text;
        public TextValue(string text)
        {
            _text = text;
        }

        public override string ToString()
        {
            return _text;
        }
    }

    public class XmlValue : ITemplateValue
    {
        protected readonly XmlNode _node;
        public XmlValue(XmlNode node)
        {
            _node = node;
        }

        public override string ToString()
        {
            return _node.InnerXml;
        }
    }

    public class MarkdownValue : ITemplateValue
    {
        protected readonly string _xml;
        public MarkdownValue(XmlNode preprocessed)
        {
            _xml = Markdown.ToHTML(preprocessed.InnerXml);
        }

        public override string ToString()
        {
            return _xml;
        }
    }

    public class ListValue : ITemplateValue
    {
        public IReadOnlyList<ITemplateValue> Values => _values;

        protected readonly List<ITemplateValue> _values;

        public ListValue(int reserved = 8)
        {
            _values = new List<ITemplateValue>(reserved);
        }

        public void AddValue(ITemplateValue value)
        {
            _values.Add(value);
        }
    }

    public class StructValue : ITemplateValue
    {
        public IReadOnlyDictionary<string, ITemplateValue> Values => _values;
        protected readonly Dictionary<string, ITemplateValue> _values;

        public StructValue(int reserved = 8)
        {
            _values = new Dictionary<string, ITemplateValue>(reserved);
        }

        public StructValue(DateTime date)
        {
            _values = new Dictionary<string, ITemplateValue>(){
                { "text", new TextValue(date.ToString())},
                { "year", new TextValue(date.ToString("yyyy")) },
                { "month", new TextValue(date.ToString("MMMM")) },
                {"day", new TextValue(date.ToString("dd")) }
            };
        }

        public void AddField(string name, ITemplateValue value)
        {
            if(_values.ContainsKey(name))
            {
                throw new Exception(string.Format("Pre-existing struct field: {0}", name));
            }
            _values[name] = value;
        }
    }
}
