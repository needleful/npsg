using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public delegate void TemplateAction(
        TemplateEngine.TContext context,
        Template template,
        XmlNode parent, XmlNode node,
        IDictionary<string, ITemplateValue> values);

    public class TemplateActions
    {
        public static IReadOnlyDictionary<string, TemplateAction> Actions = new Dictionary<string, TemplateAction>
        {
            {"ForEach", ForEach },
            {"Match", Match }
        };

        public static bool Contains(string name)
        {
            return Actions.ContainsKey(name);
        }

        public static void DoAction(TemplateEngine.TContext context, Template template,
            XmlNode parent, XmlNode node,
            IDictionary<string, ITemplateValue> values)
        {
            Actions[node.Name](context, template, parent, node, values);
        }

        public static void ForEach(TemplateEngine.TContext context, Template template,
            XmlNode parent, XmlNode node,
            IDictionary<string, ITemplateValue> values)
        {
            string name = node.Attributes["list"].Value;
            if (values.ContainsKey(name) && values[name] is ListValue list)
            {
                if (template.Params[name] is ListParam paramList)
                {
                    XmlElement listElem = parent.OwnerDocument.CreateElement("div");
                    if((node as XmlElement).HasAttribute("html-class"))
                    {
                        listElem.SetAttribute("class", node.Attributes["html-class"].Value);
                    }
                    int iter_value = 1;
                    string iter_name = "__iter";
                    if (node.Attributes["i"] != null)
                    {
                        iter_name = node.Attributes["i"].Value;
                    }

                    foreach (var value in list.Values)
                    {
                        values[iter_name] = new TextValue(iter_value.ToString());
                        values[paramList.SubParam.Name] = value;
                        XmlNode applied = template.ApplyValues(node.CloneNode(true), values);
                        foreach(XmlNode c2 in applied.ChildNodes)
                        {
                            listElem.AppendChild(c2);
                        }
                        ++iter_value;
                    }
                    parent.ReplaceChild(listElem, node);

                    values.Remove(iter_name);
                    values.Remove(paramList.SubParam.Name);
                }
                else
                {
                    throw new ParamException(template.Params[name],
                        string.Format("Content was formated as a list, but the param is {0}\n{1}",
                            template.Params[name].TypeName, node.OuterXml));
                }
            }
        }

        public static void Match(TemplateEngine.TContext context, Template template,
           XmlNode parent, XmlNode node,
           IDictionary<string, ITemplateValue> values)
        {
            string func = node.Attributes["f"].Value.Trim();
            string x = node.Attributes["x"].Value.Trim();

            Console.WriteLine("MATCHING: {0}({1})", func, x);

            string to_match = null;

            switch(func)
            {
                case "src-file-exists":
                    to_match = File.Exists(Path.Combine(context.SourceRoot, x)).ToString();
                    break;
                case "file-exists":
                    to_match = File.Exists(Path.Combine(context.FileRoot, x)).ToString();
                    break;
                case "get":
                    to_match = x;
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unknown match function '{0}'", func));
            }

            XmlNode found = null;
            foreach(XmlNode c in node.ChildNodes)
            {
                if(c.Name.Equals(to_match))
                {
                    if(found != null)
                    {
                        throw new Exception(string.Format("Duplicate matching elements within Match action: {0}\n{1}", to_match, node.OuterXml));
                    }
                    found = c;
                    foreach(XmlNode ch in c.ChildNodes)
                    {
                        parent.InsertBefore(ch, node);
                    }
                    parent.RemoveChild(node);
                }
            }

            if(found is null)
            {
                parent.RemoveChild(node);
            }
        }
    }
}
