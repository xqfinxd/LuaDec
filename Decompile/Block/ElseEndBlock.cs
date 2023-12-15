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

        public override bool Breakable()
        {
            return false;
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

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool HasHeader()
        {
            return true;
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType == CloseType.Close ? closeLine - 1 : base.ScopeEnd();
        }

        public override void Write(Decompiler d, Output output)
        {
            if (statements.Count == 1 && statements[0] is IfThenEndBlock)
            {
                output.Write("else");
                statements[0].Write(d, output);
            }
            else if (statements.Count == 2 && statements[0] is IfThenElseBlock && statements[1] is ElseEndBlock)
            {
                output.Write("else");
                statements[0].Write(d, output);
                statements[1].Write(d, output);
            }
            else
            {
                output.Write("else");
                output.WriteLine();
                output.Indent();
                WriteSequence(d, output, statements);
                output.Dedent();
                output.Write("end");
            }
        }
    }
}