using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class OuterBlock : ContainerBlock
    {
        public OuterBlock(LFunction function, int length)
            : base(function, 0, length + 1, CloseType.None, -1, -2)
        {
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override int ScopeEnd()
        {
            return (end - 1) + function.header.version.outerBlockScopeAdjustment.Value;
        }

        public override void Write(Decompiler d, Output output)
        {
            /* extra return statement */
            int last = statements.Count - 1;
            if (last < 0 || !(statements[last] is Return))
            {
                throw new System.InvalidOperationException(statements[last].ToString());
            }
            statements.RemoveAt(last);
            WriteSequence(d, output, statements);
        }
    }
}