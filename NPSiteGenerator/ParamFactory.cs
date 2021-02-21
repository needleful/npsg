using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public class ParamFactory
    {
        public static IParam Create(XmlNode node)
        {
            if(!node.Name.Equals("param"))
            {
                throw new Exception(
                    string.Format("Unexpected node type (expected 'param'): {0}", node.Name));
            }

            string name = node.Attributes["name"].Value;
            string[] type = node.Attributes["type"].Value
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (type[0].Equals("xml"))
            {
                return new XmlParam(name);
            }
            else if (type[0].Equals("text"))
            {
                if (type.Length > 1)
                {
                    return new TextParam(name, type[1]);
                }
                else
                {
                    return new TextParam(name);
                }
            }
            else if (type[0].Equals("list"))
            {
                int count = 0;
                ListParam param = null;
                foreach(XmlNode elem in node)
                {
                    if(elem.NodeType != XmlNodeType.Comment)
                    {
                        if(count != 0)
                        {
                            throw new Exception(
                                string.Format("Unexpected nodes (expected only one child for list type): {0}", node.OuterXml));
                        }
                        param = new ListParam(name, Create(elem));
                        count += 1;
                    }
                }
                if(param == null)
                {
                    throw new Exception(
                        string.Format("No child type for list: {0}", node.OuterXml));
                }
                return param;
            }
            else if(type[0].Equals("struct"))
            {
                StructParam param = new StructParam(name, node.ChildNodes.Count);
                foreach(XmlNode fieldNode in node.ChildNodes)
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
