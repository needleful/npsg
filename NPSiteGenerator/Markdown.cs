
using System;
using System.Text;

namespace NPSiteGenerator
{
    public class Markdown
    {
        private enum Parsing
        {
            Free,
            StartingParagraph,
            InParagraph,
            EndingParagraph,
            StartingUList,
            InUList,
            ParagraphInUList,
            EndingUList
        }

        public static string ToHTML(string markdown)
        {
            var str = new StringBuilder();
            var p = Parsing.Free;

            foreach(string lineWithWhitespace in markdown.Split('\n'))
            {
                string line = lineWithWhitespace.Trim();
                string nextLine;
                if(string.IsNullOrEmpty(line))
                {
                    if(p == Parsing.InParagraph)
                    {
                        p = Parsing.EndingParagraph;
                    }
                    else if (p == Parsing.ParagraphInUList || p == Parsing.InUList)
                    {
                        p = Parsing.EndingUList;
                    }
                    nextLine = "";
                }
                else if(line[0] == '#')
                {
                    var level = 1;
                    while(line.Length > level && line[level] == '#')
                    {
                        level += 1;
                    }
                    nextLine = string.Format("<h{0} class='markdown'>{1}</h{0}>", level, line.Substring(level));
                    p = Parsing.Free;
                }
                else if(line[0] == '-' || line[0] == '*')
                {
                    nextLine = $"<li class='markdown'>{line.Substring(1)}";

                    if(p == Parsing.InParagraph)
                    {
                        str.AppendLine("</p>");
                        p = Parsing.StartingUList;
                    }
                    else if(p == Parsing.InUList || p == Parsing.ParagraphInUList)
                    {
                        str.Append("</li>");
                        p = Parsing.InUList;
                    }
                    else
                    {
                        p = Parsing.StartingUList;
                    }
                }
                else
                {
                    nextLine = line;
                    if(p == Parsing.Free)
                    {
                        p = Parsing.StartingParagraph;
                        str.Append("<p class='markdown'>");
                    }
                    if(p == Parsing.InUList)
                    {
                        p = Parsing.ParagraphInUList;
                    }
                }

                if (p == Parsing.StartingParagraph)
                {
                    p = Parsing.InParagraph;
                }
                else if (p == Parsing.StartingUList)
                {
                    str.AppendLine("<ul class='markdown'>");
                    p = Parsing.InUList;
                }
                else if (p == Parsing.InParagraph || p == Parsing.ParagraphInUList && nextLine.Length > 0)
                {
                    str.AppendLine("<br/>");
                }
                else if(p == Parsing.EndingParagraph)
                {
                    str.AppendLine("</p>");
                    p = Parsing.Free;
                }
                else if (p == Parsing.EndingUList)
                {
                    str.Append("</li></ul>");
                    p = Parsing.Free;
                }

                if (nextLine.Length > 0)
                {
                    str.AppendLine(nextLine);
                }
            }
            return str.ToString();
        }
    }
}
