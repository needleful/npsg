using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNG
{
    class GeneratorException : Exception
    {
        public GeneratorException(string file, string context)
            : base(string.Format("Error in {0}: {1}", file, context))
        { }
    }
}
