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
}
