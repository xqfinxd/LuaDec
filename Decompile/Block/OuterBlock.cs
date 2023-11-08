using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class OuterBlock : ContainerBlock
    {

        public OuterBlock(LFunction function, int length)
            : base(function, 0, length + 1, CloseType.NONE, -1, -2)
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

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override int scopeEnd()
        {
            return (end - 1) + function.header.version.outerBlockScopeAdjustment.Value;
        }

        public override void print(Decompiler d, Output output)
        {
            /* extra return statement */
            int last = statements.Count - 1;
            if (last < 0 || !(statements[last] is Return))
            {
                throw new System.InvalidOperationException(statements[last].ToString());
            }
            statements.RemoveAt(last);
            printSequence(d, output, statements);
        }

    }

}
