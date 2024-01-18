using System;
using System.Xml;

namespace NPSiteGenerator
{
    public class ParamFactory
    {
        public static IParam Create(XmlNode node)
        {
            var elem = node as XmlElement;

            if(!elem.Name.Equals("param"))
            {
                throw new Exception(
                    string.Format("Unexpected node type (expected 'param'): {0}", elem.Name));
            }
            if(!elem.HasAttribute("name"))
            {
                throw new Exception(
                    string.Format("Missing name for parameter: {0}", elem.Attributes));
            }
            if(!elem.HasAttribute("type"))
            {
                throw new Exception(string.Format(
                    "Missing type for parameter: {0}", elem.Attributes));
            }

            string name = elem.Attributes["name"].Value;
            string[] type = elem.Attributes["type"].Value
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            bool required = true;
            if(elem.HasAttribute("required"))
            {
                var b = elem.Attributes["required"].Value;
                if (!bool.TryParse(b, out required))
                {
                    Console.WriteLine($"WARNING: bad boolean for parameter {name}: {b}");
                }
            }
            
            if (type[0].Equals("xml"))
            {
                if (type.Length > 1)
                {
                    return new XmlParam(name, required, type[1]);
                }
                else
                {
                    return new XmlParam(name, required);
                }
            }
            else if (type[0].Equals("text"))
            {
                if (type.Length > 1)
                {
                    return new TextParam(name, required, type[1]);
                }
                else
                {
                    return new TextParam(name, required);
                }
            }
            else if (type[0].Equals("list"))
            {
                int count = 0;
                ListParam param = null;
                foreach(XmlNode sub in elem)
                {
                    if(sub.NodeType != XmlNodeType.Comment)
                    {
                        if(count != 0)
                        {
                            throw new Exception(
                                string.Format("Unexpected nodes (expected only one child for list type): {0}", elem.OuterXml));
                        }
                        param = new ListParam(name, required, Create(sub));
                        count += 1;
                    }
                }
                if(param == null)
                {
                    throw new Exception(
                        string.Format("No child type for list: {0}", elem.OuterXml));
                }
                return param;
            }
            else if(type[0].Equals("struct"))
            {
                StructParam param = new StructParam(name, required, elem.ChildNodes.Count);
                foreach(XmlNode fieldNode in elem.ChildNodes)
                {
                    if(fieldNode.NodeType != XmlNodeType.Comment)
                    {
                        param.AddField(Create(fieldNode));
                    }
                }
                return param;
            }
            else
            {
                throw new Exception(string.Format("Unrecognized param type: {0}", type));
            }
        }
    }
}
