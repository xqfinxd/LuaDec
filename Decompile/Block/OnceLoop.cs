using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class OnceLoop : ContainerBlock
    {
        public OnceLoop(LFunction function, int begin, int end)
             : base(function, begin, end, CloseType.None, -1, 0)
        {
        }

        public override bool Breakable()
        {
            return true;
        }

        public override int GetLoopback()
        {
            return begin;
        }

        public override bool HasHeader()
        {
            return false;
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override int ScopeEnd()
        {
            return end - 1;
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteLine("repeat");
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.Write("until true");
        }
    }
}