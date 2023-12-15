using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class Goto : IBlock
    {
        public readonly int target;

        public Goto(LFunction function, int line, int target)
            : base(function, line, line, 2)
        {
            this.target = target;
        }

        public override void AddStatement(IStatement statement)
        {
            throw new System.NotImplementedException();
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsContainer()
        {
            return false;
        }

        public override bool IsEmpty()
        {
            return true;
        }

        public override bool HasHeader()
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
            output.Write("goto lbl_" + target);
        }
    }
}