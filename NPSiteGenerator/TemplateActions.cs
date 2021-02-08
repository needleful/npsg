using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public delegate void TemplateAction(
        Template template,
        XmlNode parent, XmlNode node,
        IDictionary<string, ITemplateValue> values);

    public class TemplateActions
    {
        public static IReadOnlyDictionary<string, TemplateAction> Actions = new Dictionary<string, TemplateAction>
        {
            {"foreach", ForEach },
            {"cond", Cond }
        };

        public static bool Contains(string name)
        {
            return Actions.ContainsKey(name);
        }

        public static void DoAction(string name, Template template,
            XmlNode parent, XmlNode node,
            IDictionary<string, ITemplateValue> values)
        {
            Actions[name](template, parent, node, values);
        }

        public static void ForEach(Template template,
            XmlNode parent, XmlNode node,
            IDictionary<string, ITemplateValue> values)
        {
            string name = node.Attributes["list"].Value;
            if (values.ContainsKey(name) && values[name] is ListValue list)
            {
                if (template.Params[name] is ListParam paramList)
                {
                    XmlElement listElem = parent.OwnerDocument.CreateElement("div");
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
                        listElem.InnerXml += applied.InnerXml;
                        ++iter_value;
                    }
                    Console.WriteLine(">> {0} -> {1}", node.OuterXml, listElem.OuterXml);
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

        public static void Cond(Template template,
           XmlNode parent, XmlNode node,
           IDictionary<string, ITemplateValue> values)
        {
            throw new NotImplementedException();
        }
    }
}
