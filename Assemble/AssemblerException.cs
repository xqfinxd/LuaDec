using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Assemble
{
    public class AssemblerException : Exception
    {

        public AssemblerException(string msg) : base(msg)
        {
            
        }

    }
}
