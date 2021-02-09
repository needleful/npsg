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
        public struct TContext
        {
            public string FileRoot
            {
                get;
                private set;
            }

            public string SourceRoot
            {
                get;
                private set;
            }

            public TContext(string froot, string srcRoot)
            {
                FileRoot = froot;
                SourceRoot = srcRoot;
            }
        }

        readonly IDictionary<string, Template> templates;
        public TContext Context
        {
            get;
            private set;
        }

        public TemplateEngine(string fileRoot, string sourceDir)
        {
            Context = new TContext(fileRoot, sourceDir);
            templates = new Dictionary<string, Template>();
        }

        public void ReadTemplates()
        {
            ReadTemplates(Context.SourceRoot);
        }

        public void ReadTemplates(string sourceDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new ArgumentException(string.Format("Template directory does not exist: {0}", sourceDir));
            }
            foreach (string template in Directory.EnumerateFiles(sourceDir, "*.template.xml"))
            {
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
                                AddTemplate(template, sub);
                            }
                        }
                        else
                        {
                            AddTemplate(template, node);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing '{0}':{1}\n {2}", template, e.Message, e);
                }
            }
        }

        public void GeneratePages()
        {
            GeneratePages(Context.SourceRoot, Context.FileRoot);
        }

        public void GeneratePages(string sourceDir, string outDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new ArgumentException(string.Format("Source directory does not exist: {0}", sourceDir));
            }
            if (!Directory.Exists(outDir))
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
                Console.WriteLine("Reading page {0}", file);
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    XmlNode page = null;
                    foreach (XmlNode n in doc)
                    {
                        if (n.Name.Equals("page"))
                        {
                            page = Process(n);
                            break;
                        }
                    }
                    if (page == null)
                    {
                        throw new Exception("No <page> node");
                    }

                    XmlDocument newDoc = new XmlDocument();
                    XmlNode doctype = newDoc.CreateDocumentType("html", null, null, null);
                    newDoc.AppendChild(doctype);

                    foreach (XmlNode child in page.ChildNodes)
                    {
                        var n = newDoc.ImportNode(child, true);
                        newDoc.AppendChild(n);
                    }

                    string outFile = Path.Combine(outDir,
                    Path.GetFileName(file).Replace(".page.xml", ".html"));

                    if (File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }
                    var f = new FileStream(outFile, FileMode.OpenOrCreate);

                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        OmitXmlDeclaration = true,
                    };
                    var writer = XmlWriter.Create(f, settings);
                    newDoc.WriteContentTo(writer);
                    writer.Flush();
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
            foreach (XmlNode n in page.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if (templates.ContainsKey(n.Name))
                    {
                        XmlNode applied = templates[n.Name].Apply(n, Context);
                        XmlNode replacement = page.OwnerDocument.ImportNode(applied, true);
                        foreach (XmlNode c in replacement.ChildNodes)
                        {
                            page.InsertBefore(c, n);
                        }
                        page.RemoveChild(n);
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

        protected void AddTemplate(string filename, XmlNode xml)
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
                Template t = new Template(this, xml);
                templates[t.Name] = t;
                Console.WriteLine("* {0} :: {1}", filename, t);
            }
        }
    }
}
