using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class OnceLoop : ContainerBlock
    {

        public OnceLoop(LFunction function, int begin, int end)
             : base(function, begin, end, CloseType.NONE, -1, 0)
        {
        }

        public override int scopeEnd()
        {
            return end - 1;
        }

        public override bool breakable()
        {
            return true;
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override int getLoopback()
        {
            return begin;
        }

        public override void print(Decompiler d, Output output)
        {
            output.WriteLine("repeat");
            output.Indent();
            printSequence(d, output, statements);
            output.Dedent();
            output.WriteString("until true");
        }

    }

}
