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

        public void GeneratePages(string sourceDir, string outDir = "")
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new ArgumentException(string.Format("Source directory does not exist: {0}", sourceDir));
            }
            if (string.IsNullOrEmpty(outDir))
            {
                outDir = context.FileRoot;
            }
            if(!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            Console.WriteLine("{0} -> {1}", sourceDir, outDir);
            foreach (string dir in Directory.EnumerateDirectories(sourceDir))
            {
                string dirHead = Path.GetFileName(dir);
                string newOut = Path.Combine(outDir, dirHead);
                GeneratePages(dir, newOut);
            }
            foreach (string file in Directory.EnumerateFiles(sourceDir, "*.page.xml"))
            {
                try
                {
                    Console.WriteLine("Reading page {0}", file);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    var newDoc = Process(doc);
                    string outFile = Path.Combine(outDir, 
                        Path.GetFileName(file).Replace(".page.xml", ".html"));

                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        OmitXmlDeclaration = true,
                    };

                    if(File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }

                    var f = new FileStream(outFile, FileMode.OpenOrCreate);
                    var writer = XmlWriter.Create(f, settings);

                    doc.FirstChild.WriteContentTo(writer);
                    writer.Close();
                    f.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while reading {0}: {1}", file, e);
                }
            }
        }

        protected XmlNode Process(XmlNode page)
        {
            foreach(XmlNode n in page.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if(templates.ContainsKey(n.Name))
                    {
                        XmlNode applied = templates[n.Name].Apply(n, context);
                        XmlNode replacement = page.OwnerDocument.ImportNode(applied, true);
                        page.ReplaceChild(replacement, n);
                    }
                    else
                    {
                        XmlNode processed = Process(n);
                        page.ReplaceChild(processed, n);
                    }
                }
            }
            return page;
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
