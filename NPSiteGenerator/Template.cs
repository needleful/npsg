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

        public IDictionary<string, ITextFormula> Formulas
        {
            get;
        } = new Dictionary<string, ITextFormula>();

        public TemplateEngine Engine
        {
            get;
            private set;
        }

        readonly XmlNode _content;
        readonly static Regex textReplace = new Regex(@"\{\{\s*(?<formula>[^\{\}]+)\s*\}\}");

        public Template(TemplateEngine engine, XmlNode xml)
        {
            Engine = engine;
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
        
        public XmlNode Apply(XmlNode instance, TemplateEngine.TContext context)
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
                string f = m.Groups["formula"].Value;
                var form = GetOrParseFormula(f);
                if (form.CanCompute(values))
                {
                    return form.Compute(values);
                }
                else
                {
                    return m.Value;
                }
            }
            content.InnerXml = textReplace.Replace(content.InnerXml, _EvalMatch);

            return XmlReplace(content, values);
        }

        private ITextFormula GetOrParseFormula(string formula)
        {
            if(!Formulas.ContainsKey(formula))
            {
                Formulas[formula] = TextFormulaParser.Parse(formula);
            }

            return Formulas[formula];
        }

        private XmlNode XmlReplace(XmlNode content, IDictionary<string, ITemplateValue> values)
        {
            var originalNodes = new List<XmlNode>(content.ChildNodes.Count);

            foreach (XmlNode c in content.ChildNodes)
            {
                originalNodes.Add(c);
            }
            foreach(var node in originalNodes)
            {
                if (TemplateActions.Contains(node.Name))
                {
                    TemplateActions.DoAction(Engine.Context, this, content, node, values);
                }
                else if (node.NodeType == XmlNodeType.Element && node.ChildNodes.Count > 0)
                {
                    content.ReplaceChild(XmlReplace(node, values), node);
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
