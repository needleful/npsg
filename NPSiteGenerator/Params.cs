using System;
using System.IO;
using System.Xml;

namespace NPSiteGenerator
{
    public interface IParam
    {
        string Name { get; }

        string Process(XmlNode node, TemplateEngine.Context context);
    }

    public class TextParam : IParam
    {
        enum DataType
        {
            Generic,
            File,
            Integer,
            FloatingPoint
        }
        public string Name
        {
            get;
            private set;
        }

        public TextParam(string name, string p_type = "generic")
        {
            Name = name;
            switch (p_type.ToLower())
            {
                case "generic":
                case "text":
                case "none":
                    _type = DataType.Generic;
                    break;
                case "integer":
                case "int":
                case "uint":
                    _type = DataType.Integer;
                    break;
                case "float":
                case "double":
                    _type = DataType.FloatingPoint;
                    break;
                case "file":
                    _type = DataType.File;
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid subtype: {0}", p_type));
            }
        }
        readonly DataType _type;

        public string Process(XmlNode outerNode, TemplateEngine.Context context)
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
            switch (_type)
            {
                case DataType.Generic:
                    return text;
                case DataType.Integer:
                    if(!long.TryParse(text, out _))
                    {
                        throw new ParamException(this,
                            string.Format("Not a valid integer: {0}", text));
                    }
                    break;
                case DataType.FloatingPoint:
                    if (!double.TryParse(text, out _))
                    {
                        throw new ParamException(this,
                            string.Format("Not a valid float: {0}", text));
                    }
                    break;
                case DataType.File:
                    if(!File.Exists(Path.Combine(context.FileRoot, text)))
                    {
                        throw new ParamException(this,
                            string.Format("File does not exist: {0}", text));
                    }
                    return "/" + text;
                default:
                    throw new NotImplementedException(string.Format("Unknown type: {0}", _type));
            }
            return text;
        }

        public override string ToString()
        {
            return string.Format("text,{0}", _type);
        }
    }

    public class XmlParam : IParam
    {
        public string Name
        {
            get;
            private set;
        }

        public XmlParam(string name)
        {
            Name = name;
        }

        public string Process(XmlNode node, TemplateEngine.Context context)
        {
            return node.InnerXml;
        }

        public override string ToString()
        {
            return "xml";
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
            : base(string.Format("Failed to parse parameter '{0}' : {1}", param.Name, context))
        {
            Param = param;
        }
    }
}
