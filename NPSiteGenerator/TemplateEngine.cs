using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NPSiteGenerator
{
    public class TemplateEngine
    {
        public struct Context
        {
            public string FileRoot
            {
                get;
                private set;
            }

            public Context(string root)
            {
                FileRoot = root;
            }
        }

        readonly IDictionary<string, Template> templates;
        readonly Context context;

        public TemplateEngine(string fileRoot)
        {
            if (!Directory.Exists(fileRoot))
            {
                Directory.CreateDirectory(fileRoot);
            }
            context = new Context(fileRoot);
            templates = new Dictionary<string, Template>();
        }

        public void ReadTemplates(string sourceDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new ArgumentException(string.Format("Template directory does not exist: {0}", sourceDir));
            }
            foreach (string template in Directory.EnumerateFiles(sourceDir, "*.template.xml"))
            {
                Console.WriteLine("Template file: {0}", template);
                var xml = new XmlDocument();
                try
                {
                    xml.Load(template);
                    foreach (XmlNode node in xml.ChildNodes)
                    {
                        if (node.Name.Equals("templates"))
                        {
                            foreach (XmlNode sub in node.ChildNodes)
                            {
                                AddTemplate(sub);
                            }
                        }
                        else
                        {
                            AddTemplate(node);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing '{0}':{1}\n {2}", template, e.Message, e);
                }
            }
        }

        public void GeneratePages(string sourceDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new ArgumentException(string.Format("Source directory does not exist: {0}", sourceDir));
            }
            foreach (string dir in Directory.EnumerateDirectories(sourceDir))
            {
                GeneratePages(dir);
            }
            foreach (string file in Directory.EnumerateFiles(sourceDir, "*.page.xml"))
            {
                try
                {
                    Console.WriteLine("Reading page {0}", file);
                    XmlReader reader = XmlReader.Create(file);
                    XmlDocument doc = new XmlDocument();
                    while (reader.Read())
                    {
                        if(reader.IsStartElement())
                        {
                            string name = reader.Name;
                            Console.WriteLine("<{0}>", name);

                            if(templates.ContainsKey(name))
                            {
                                doc.LoadXml(reader.ReadOuterXml());

                                templates[name].ReadTemplateUse(doc.FirstChild, context);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while reading {0}: {1}", file, e);
                }
            }
        }

        protected void AddTemplate(XmlNode xml)
        {
            if (xml.NodeType == XmlNodeType.Comment)
            {
                return;
            }
            if (!xml.Name.Equals("template"))
            {
                throw new ArgumentException(
                    string.Format("Unexpected XML element ('template' expected): {0}", xml.Name));
            }
            else
            {
                Template t = new Template(xml);
                templates[t.Name] = t;
                Console.WriteLine("\tNew Template: {0}", t.Name);
            }
        }
    }
}
