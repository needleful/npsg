using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NPSiteGenerator
{
    public interface IParam
    {
        string Name { get; }
        string TypeName { get; }

        ITemplateValue Process(XmlNode node, TemplateEngine.Context context);
    }

    public class TextParam : IParam
    {
        public enum DataType
        {
            generic,
            file,
            integer,
            @float
        }
        public string Name
        {
            get;
            private set;
        }

        public string TypeName => "text," + SubType.ToString();

        public TextParam(string name, string p_type = "generic")
        {
            Name = name;
            switch (p_type.ToLower())
            {
                case "generic":
                case "text":
                case "none":
                    SubType = DataType.generic;
                    break;
                case "integer":
                case "int":
                case "uint":
                    SubType = DataType.integer;
                    break;
                case "float":
                case "double":
                    SubType = DataType.@float;
                    break;
                case "file":
                    SubType = DataType.file;
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid subtype: {0}", p_type));
            }
        }
        public readonly DataType SubType;

        public ITemplateValue Process(XmlNode outerNode, TemplateEngine.Context context)
        {
            if (outerNode.ChildNodes.Count != 1)
            {
                throw new ParamException(this, 
                    string.Format("Expected one child, got {0}\n{1}", outerNode.ChildNodes.Count, outerNode.OuterXml));
            }

            XmlNode node = outerNode.FirstChild;
            if (node.NodeType != XmlNodeType.Text)
            {
                throw new ParamException(this, 
                    string.Format("Expected plain text, got {0}\n{1}", node.NodeType, node.OuterXml));
            }

            string text = node.InnerText;
            switch (SubType)
            {
                case DataType.generic:
                    break;
                case DataType.integer:
                    if(!long.TryParse(text, out _))
                    {
                        throw new ParamException(this,
                            string.Format("Not a valid integer: {0}", text));
                    }
                    break;
                case DataType.@float:
                    if (!double.TryParse(text, out _))
                    {
                        throw new ParamException(this,
                            string.Format("Not a valid float: {0}", text));
                    }
                    break;
                case DataType.file:
                    if(!File.Exists(Path.Combine(context.FileRoot, text)))
                    {
                        throw new ParamException(this,
                            string.Format("File does not exist: {0}", text));
                    }
                    text = "/" + text;
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unknown type: {0}", SubType));
            }
            return new TextValue(text);
        }
    }

    public class XmlParam : IParam
    {
        public string Name
        {
            get;
            private set;
        }

        public string TypeName => "xml";

        public XmlParam(string name)
        {
            Name = name;
        }

        public ITemplateValue Process(XmlNode node, TemplateEngine.Context context)
        {
            return new XmlValue(node);
        }
    }

    public class ListParam : IParam
    {
        public string Name
        {
            get;
            private set;
        }

        public string TypeName => "list," + SubParam.TypeName;

        public IParam SubParam;

        public ListParam(string name, IParam subParam)
        {
            Name = name;
            SubParam = subParam;
        }

        public ITemplateValue Process(XmlNode node, TemplateEngine.Context context)
        {
            ListValue list = new ListValue(node.ChildNodes.Count);
            foreach(XmlNode c in node.ChildNodes)
            {
                if(c.NodeType != XmlNodeType.Comment)
                {
                    if(!c.Name.Equals(SubParam.Name))
                    {
                        throw new ParamException(this, 
                            string.Format("Unexpected node (expected '{0}'): {1}\n{2}", 
                                SubParam.Name, c.Name, node.OuterXml));
                    }
                    list.AddValue(SubParam.Process(c, context));
                }
            }
            return list;
        }
    }

    public class ParamException : Exception
    {
        public IParam Param
        {
            get;
            private set;
        }

        public ParamException(IParam param, string context)
            : base(string.Format("Failed to parse parameter '{0}/{1}' : {2}", param.Name, param.TypeName, context))
        {
            Param = param;
        }
    }
}
