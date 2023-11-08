using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class ElseEndBlock : ContainerBlock
    {

        public IfThenElseBlock partner;

        public ElseEndBlock(LFunction function, int begin, int end, CloseType closeType, int closeLine)
             : base(function, begin, end, closeType, closeLine, -1)
        {
        }

        public override int CompareTo(IBlock block)
        {
            if (block == partner)
            {
                return 1;
            }
            else
            {
                int result = base.CompareTo(block);
                return result;
            }
        }

        public override bool breakable()
        {
            return false;
        }

        public override int scopeEnd()
        {
            return usingClose && closeType == CloseType.CLOSE ? closeLine - 1 : base.scopeEnd();
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void print(Decompiler d, Output output)
        {
            if (statements.Count == 1 && statements[0] is IfThenEndBlock) {
                output.print("else");
                statements[0].print(d, output);
            } else if (statements.Count == 2 && statements[0] is IfThenElseBlock && statements[1] is ElseEndBlock) {
                output.print("else");
                statements[0].print(d, output);
                statements[1].print(d, output);
            } else
            {
                output.print("else");
                output.println();
                output.indent();
                printSequence(d, output, statements);
                output.dedent();
                output.print("end");
            }
        }

    }

}