using System;
using System.Collections.Generic;
using System.IO;
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
            XmlNode parent, XmlNode feNode,
            IDictionary<string, ITemplateValue> values)
        {
            string name = feNode.Attributes["list"].Value;
            if (values.ContainsKey(name) && values[name] is ListValue list)
            {
                if (template.Params[name] is ListParam paramList)
                {
                    int iter_value = 1;
                    string iter_name = "__iter";
                    if (feNode.Attributes["i"] != null)
                    {
                        iter_name = feNode.Attributes["i"].Value;
                    }

                    foreach (var value in list.Values)
                    {
                        values[iter_name] = new TextValue(iter_value.ToString());
                        values[paramList.SubParam.Name] = value;
                        XmlNode applied = template.ApplyValues(feNode.CloneNode(true), values);

                        parent.InsertChildrenBefore(applied, feNode);
                        ++iter_value;
                    }
                    parent.RemoveChild(feNode);

                    values.Remove(iter_name);
                    values.Remove(paramList.SubParam.Name);
                }
                else
                {
                    throw new ParamException(template.Params[name],
                        string.Format("Content was formated as a list, but the param is {0}\n{1}",
                            template.Params[name].TypeName, feNode.OuterXml));
                }
            }
            else
            {
                parent.RemoveChild(feNode);
            }
        }

        public static void Match(TemplateEngine.TContext context, Template template,
           XmlNode parent, XmlNode matchNode,
           IDictionary<string, ITemplateValue> values)
        {
            XmlElement matchElem = matchNode as XmlElement;

            string to_match = null;

            if (matchElem.HasAttribute("f") && matchElem.HasAttribute("x"))
            {
                string func = matchNode.Attributes["f"].Value.Trim();
                string x = matchNode.Attributes["x"].Value.Trim();
                switch (func)
                {
                    case "src-file-exists":
                        to_match = File.Exists(Path.Combine(context.SourceRoot, x)).ToString();
                        break;
                    case "src-dir-exists":
                        to_match = Directory.Exists(Path.Combine(context.SourceRoot, x)).ToString();
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
            }
            else
            {
                to_match = matchElem.GetAttribute("value");
            }

            
            foreach(XmlNode c in matchNode.ChildNodes)
            {
                if(c.Name.Equals(to_match))
                {
                    parent.InsertChildrenBefore(c, matchNode);
                }
            }
            parent.RemoveChild(matchNode);
        }
    }
}
