using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPSiteGenerator
{
    public class TextFormulaParser
    {
        public static ITextFormula Parse(string text)
        {
            throw new NotImplementedException();
        }
    }

    public interface ITextFormula
    {
        bool CanCompute(IDictionary<string, ITemplateValue> values);
        string Compute(IDictionary<string, ITemplateValue> values);
    }
}
