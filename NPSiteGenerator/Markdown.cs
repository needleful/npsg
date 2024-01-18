
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                    nextLine = string.Format("<h{0} class='markdown'>{1}</h{0}>", 
                        level, ParseInlineMarkup(line.Substring(level)));
                    p = Parsing.Free;
                }
                else if(line[0] == '-' || line[0] == '*')
                {
                    nextLine = $"<li class='markdown'>{ParseInlineMarkup(line.Substring(1))}";

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
                    nextLine = ParseInlineMarkup(line);
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

        private static readonly string[] symbols =
        {
            "__",
            "**",
            "_",
            "*",
            "~",
            "`"
        };

        private static readonly Dictionary<string, string> symToTag = new Dictionary<string, string>
        {
            { "*", "em" },
            { "_", "em" },
            {"**", "strong" },
            {"__", "strong" },
            {"`", "code" },
            {"~", "s" }
        };
        
        private struct TagStart
        {
            public string originalText;
            public string tag;
            public int offset;

            public TagStart(string text, string tag, int start)
            {
                originalText = text;
                this.tag = tag;
                offset = start;
            }

            public void RemoveFrom(StringBuilder builder)
            {
                builder.Remove(offset, tag.Length + 2);
                builder.Insert(offset, originalText);
            }
        }

        static string ParseInlineMarkup(string line)
        {
            var needsParsing = false;
            foreach(string sym in symbols)
            {
                if(line.Contains(sym))
                {
                    needsParsing = true;
                    break;
                }
            }
            if (!needsParsing)
            {
                return line;
            }

            var editedLine = new StringBuilder(line.Length + 10);
            var tags = new List<TagStart>();
            var orphanedTags = new List<TagStart>();
            for(var i = 0; i < line.Length; i++)
            {
                var match = false;
                foreach (string sym in symbols)
                {
                    if(ContainsAt(line, sym, i))
                    {
                        match = true;
                        i += sym.Length - 1;
                        var tag = symToTag[sym];
                        if (tags.Count > 0)
                        {
                            var start_index = -1;
                            // Check if we opened this tag
                            for (int t = tags.Count; t --> 0;)
                            {
                                var tt = tags[t];
                                if(tt.originalText == sym)
                                {
                                    start_index = t;
                                    break;
                                }
                            }
                            if(start_index > -1)
                            {
                                // Tags started after this one are orphaned now that it's closed.
                                for (int t = tags.Count; t --> start_index;)
                                {
                                    if (t != start_index)
                                    {
                                        orphanedTags.Add(tags[t]);
                                    }
                                    tags.RemoveAt(t);
                                }
                                editedLine.Append($"</{tag}>");
                                break;
                            }
                        }
                        tags.Add(new TagStart(sym, tag, editedLine.Length));
                        editedLine.Append($"<{tag}>");
                        break;
                    }
                }
                if(!match)
                {
                    editedLine.Append(line[i]);
                }
            }

            // We remove the orphaned tags, starting from the last ones to not mess with offsets
            orphanedTags.AddRange(tags);
            orphanedTags.OrderBy((tag) => tag.offset);
            foreach (TagStart t in orphanedTags)
            {
                t.RemoveFrom(editedLine);
            }
            return editedLine.ToString();
        }

        private static bool ContainsAt(string text, string match, int start)
        {
            if (text.Length - start < match.Length)
            {
                return false;
            }
            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] != text[i + start])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
