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
            string sourceDirectory = "src";

            var engine = new TemplateEngine("www");
            engine.ReadTemplates(sourceDirectory);
            engine.GeneratePages("src");
        }
    }
}
