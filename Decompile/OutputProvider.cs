using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public interface OutputProvider
    {

        void print(string s);

        void print(byte b);

        void println();

    }
}
