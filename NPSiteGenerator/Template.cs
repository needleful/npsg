using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        readonly string _content;
        readonly IDictionary<string, IParam> _params;
        readonly static Regex textReplace = new Regex(@"\{\{\s*(?<variable>\w+)\s*\}\}");

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
                    // Split into subtype
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
                        param = new XmlParam(name);
                    }
                    else if (type.Equals("text"))
                    {
                        param = new TextParam(name, subtype);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unrecognized param type: {0}", type));
                    }

                    _params[name] = param;
                }
                else if (child.Name.Equals("content"))
                {
                    _content = child.InnerXml;
                }
            }
        }
        
        public XmlNode Apply(XmlNode instance, TemplateEngine.Context context)
        {
            var values = new Dictionary<string, string>();
            foreach (XmlNode node in instance.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                string name = node.Name;

                if (!_params.ContainsKey(name))
                {
                    throw new Exception(string.Format("Unknown element: {0}", node.OuterXml));
                }
                values[name] = _params[name].Process(node, context);
            }

            foreach(var pair in values)
            {
                Console.WriteLine("{0} = {1}", pair.Key, pair.Value);
            }

            foreach(var param in _params.Keys)
            {
                if(!values.ContainsKey(param))
                {
                    throw new Exception(
                        string.Format("Missing required parameter: {0}", param));
                }
            }

            string newXml = TextReplace(values);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(newXml);

            return doc.DocumentElement;
        }

        private string TextReplace(IDictionary<string, string> values)
        {
            string _EvalMatch(Match m)
            {
                string var = m.Groups["variable"].Value;
                if(values.ContainsKey(var))
                {
                    return values[var];
                }
                else
                {
                    return m.Value;
                }
            }

            return textReplace.Replace(_content, _EvalMatch);
        }
    }
}
