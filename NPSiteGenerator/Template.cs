using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NPSiteGenerator
{
    public class Template
    {
        public string Name
        {
            get;
            private set;
        }

        readonly XmlNode _content;
        readonly IDictionary<string, IParam> _params;

        public Template(XmlNode xml)
        {
            Name = xml.Attributes["name"].Value;
            _params = new Dictionary<string, IParam>();
            foreach (XmlNode child in xml)
            {
                if (child.Name.Equals("param"))
                {
                    string name = child.Attributes["name"].Value.ToLower().Trim();
                    string type = child.Attributes["type"].Value.ToLower().Trim();
                    string subtype = "generic";
                    // Split into types
                    if (type.IndexOf(',') != -1)
                    {
                        int i = type.IndexOf(',');
                        string c = type;
                        type = c.Substring(0, i).Trim();
                        subtype = c.Substring(i + 1).Trim();
                    }

                    IParam param;
                    if (type.Equals("xml"))
                    {
                        param = new XmlParam();
                    }
                    else if (type.Equals("text"))
                    {
                        param = new TextParam(subtype);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unrecognized param type: {0}", type));
                    }

                    _params[name] = param;
                }
            }
        }

        // TODO: actually write to a file
        public void ReadTemplateUse(XmlNode instance, TemplateEngine.Context context)
        {
            var dict = new Dictionary<string, XmlNode>();
            foreach (XmlNode node in instance.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                string name = node.Name;

                if (!_params.ContainsKey(name))
                {
                    throw new Exception(string.Format("Unknown element: {0}", name));
                }
                else if (!_params[name].Validate(node, context))
                {
                    throw new Exception(
                        string.Format("Bad element: node '{0}' does not match parameter {1}\n{2}",
                        name, _params[name], node.InnerText));
                }
                dict[name] = node;
            }

            foreach(var pair in dict)
            {
                Console.WriteLine("{0} = {1}", pair.Key, pair.Value.InnerXml);
            }
        }
    }
}
