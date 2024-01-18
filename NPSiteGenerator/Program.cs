
namespace NPSiteGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            var engine = new TemplateEngine("www", "src");
            engine.ReadTemplates();
            engine.GeneratePages();
        }
    }
}
