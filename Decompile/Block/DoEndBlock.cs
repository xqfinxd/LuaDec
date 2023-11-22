using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class DoEndBlock : ContainerBlock
    {

        public DoEndBlock(LFunction function, int begin, int end)
             : base(function, begin, end, CloseType.None, -1, 1)
        {
        }

        public override bool breakable()
        {
            return false;
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override bool allowsPreDeclare()
        {
            return true;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteLine("do");
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }

    }

}
