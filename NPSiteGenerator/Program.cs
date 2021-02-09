using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NPSiteGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new TemplateEngine("www", "src");
            engine.ReadTemplates();
            engine.GeneratePages();
        }
    }
}
