using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TXLSTool
{
    //Psycho-Pass PS Vita
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            if (args[0] == "e")
            {
                new TblFile().Extract(args[1]);
                return;
            }
            else if (args[0] == "c")
            {
                new TblFile().Create(args[1]);
                return;
            }
        }
    }
}
