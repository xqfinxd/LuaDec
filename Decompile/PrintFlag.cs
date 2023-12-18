using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class PrintFlag
    {
        public static readonly int DISASSEMBLER = 0x00000001;
        public static readonly int SHORT = 0x00000002;

        public static bool test(int flags, int flag)
        {
            return (flags & flag) != 0;
        }
    }
}
