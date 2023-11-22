using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
        }

        public override void addStatement(IStatement statement)
        {
            throw new System.InvalidOperationException();
        }

        public override bool isContainer()
        {
            return false;
        }

        public override bool isEmpty()
        {
            return true;
        }

        public override bool breakable()
        {
            return false;
        }

        public override bool isUnprotected()
        {
            //Actually, it is unprotected, but not really a block
            return false;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("goto lbl_" + target);
        }

    }

}
