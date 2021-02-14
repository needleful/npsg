using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public static class XmlExtensions
    {
        public static void ReplaceChildWithChildren(this XmlNode parent, XmlNode source, XmlNode reference)
        {
            parent.InsertChildrenAfter(source, reference);
            parent.RemoveChild(reference);
        }

        public static void InsertChildrenAfter(this XmlNode parent, XmlNode source, XmlNode reference)
        {
            List<XmlNode> nodes = new List<XmlNode>(source.ChildNodes.Count);
            foreach (XmlNode c in source.ChildNodes)
            {
                nodes.Add(c);
            }
            XmlNode prev = reference;
            foreach (XmlNode c in nodes)
            {
                parent.InsertAfter(c, prev);
            }
        }
    }
}
