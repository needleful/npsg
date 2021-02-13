using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNG
{
    class Program
    {
        static void Main(string[] args)
        {
            Generator gen = new Generator("src", "www");
            gen.GenerateAllFiles();
        }
    }
}
