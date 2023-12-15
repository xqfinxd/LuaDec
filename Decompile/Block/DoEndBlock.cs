using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class DoEndBlock : ContainerBlock
    {
        public DoEndBlock(LFunction function, int begin, int end)
             : base(function, begin, end, CloseType.None, -1, 1)
        {
        }

        public override bool AllowsPreDeclare()
        {
            return true;
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool HasHeader()
        {
            return false;
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteLine("do");
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.Write("end");
        }
    }
}