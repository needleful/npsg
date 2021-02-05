using System;
using System.IO;
using System.Xml;

namespace NPSiteGenerator
{
    public interface IParam
    {
        bool Validate(XmlNode node, TemplateEngine.Context context);
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

        public TextParam(string p_type = "generic")
        {
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

        public bool Validate(XmlNode node, TemplateEngine.Context context)
        {
            if (node.ChildNodes.Count != 1)
            {
                return false;
            }

            node = node.FirstChild;
            if (node.NodeType != XmlNodeType.Text)
            {
                return false;
            }

            string text = node.InnerText;
            switch (_type)
            {
                case DataType.Generic:
                    return true;
                case DataType.Integer:
                    return long.TryParse(text, out _);
                case DataType.FloatingPoint:
                    return double.TryParse(text, out _);
                case DataType.File:
                    return File.Exists(Path.Combine(context.FileRoot, text));
                default:
                    throw new NotImplementedException(string.Format("Unknown type: {0}", _type));
            }
        }

        public override string ToString()
        {
            return string.Format("text,{0}", _type);
        }
    }

    public class XmlParam : IParam
    {
        public bool Validate(XmlNode node, TemplateEngine.Context context)
        {
            return true;
        }

        public override string ToString()
        {
            return "xml";
        }
    }
}
