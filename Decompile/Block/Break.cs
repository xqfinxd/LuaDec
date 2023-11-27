using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class Break : IBlock
    {
        public readonly int target;
        public string breakComment;

        public Break(LFunction function, int line, int target)
            : base(function, line, line, 2)
        {
            this.target = target;
        }

        public override void AddStatement(IStatement statement)
        {
            throw new System.InvalidOperationException();
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool IsContainer()
        {
            return false;
        }

        public override bool IsEmpty()
        {
            return true;
        }

        public override bool IsUnprotected()
        {
            //Actually, it is unprotected, but not really a block
            return false;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("do break end");
            if (breakComment != null) output.WriteString(" -- " + breakComment);
        }

        public override void WriteTail(Decompiler d, Output output)
        {
            output.WriteString("break");
            if (breakComment != null) output.WriteString(" -- " + breakComment);
        }
    }
}