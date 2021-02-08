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

        public IDictionary<string, IParam> Params
        {
            get;
            set;
        }


        private IDictionary<string, ITextFormula> _formulas = new Dictionary<string, ITextFormula>();

        readonly XmlNode _content;
        readonly static Regex textReplace = new Regex(@"\{\{\s*(?<variable>[^\{\}]+)\s*\}\}");

        public Template(XmlNode xml)
        {
            Name = xml.Attributes["name"].Value;
            Params = new Dictionary<string, IParam>();
            foreach (XmlNode child in xml)
            {
                if (child.Name.Equals("param"))
                {
                    IParam p = ParamFactory.Create(child);
                    Params[p.Name] = p;
                }
                else if (child.Name.Equals("content"))
                {
                    _content = child;
                }
            }
        }
        
        public XmlNode Apply(XmlNode instance, TemplateEngine.Context context)
        {
            var values = new Dictionary<string, ITemplateValue>();
            foreach (XmlNode node in instance.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                string name = node.Name;

                if (!Params.ContainsKey(name))
                {
                    throw new Exception(string.Format("Unknown element: {0}", node.OuterXml));
                }
                values[name] = Params[name].Process(node, context);
            }

            foreach(var pair in values)
            {
                Console.WriteLine("{0} = {1}", pair.Key, pair.Value);
            }

            foreach(var param in Params.Keys)
            {
                if(!values.ContainsKey(param))
                {
                    throw new Exception(
                        string.Format("Missing required parameter: {0}", param));
                }
            }

            return ApplyValues(_content.CloneNode(true), values);
        }

        public XmlNode ApplyValues(XmlNode content, IDictionary<string, ITemplateValue> values)
        {
            // Text replacement
            string _EvalMatch(Match m)
            {
                string var = m.Groups["variable"].Value;
                if(values.ContainsKey(var))
                {
                    return values[var].ToString();
                }
                else
                {
                    return m.Value;
                }
            }
            content.InnerXml = textReplace.Replace(content.InnerXml, _EvalMatch);

            return XmlReplace(content, values);
        }

        private XmlNode XmlReplace(XmlNode content, IDictionary<string, ITemplateValue> values)
        {
            foreach (XmlNode node in content.ChildNodes)
            {
                if (TemplateActions.Contains(node.Name))
                {
                    TemplateActions.DoAction(node.Name, this, content, node, values);
                }
                else if (node.NodeType == XmlNodeType.Element && node.ChildNodes.Count > 0)
                {
                    XmlReplace(node, values);
                }
            }
            return content;
        }

        public override string ToString()
        {
            string s = "Template:";
            bool first = true;
            foreach(var p in Params.Values)
            {
                if(first)
                {
                    first = false;
                }
                else
                {
                    s += ";";
                }
                s += string.Format(" {0}/{1}", p.Name, p.TypeName);
            }
            return s;
        }
    }
}
