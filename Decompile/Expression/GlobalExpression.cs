using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class GlobalExpression : IExpression
    {

        private readonly ConstantExpression name;
        private readonly int index;

        public GlobalExpression(ConstantExpression name, int index)
            : base(PRECEDENCE_ATOMIC)
        {
            this.name = name;
            this.index = index;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            name.Walk(w);
        }

        public override int GetConstantIndex()
        {
            return index;
        }

        public override bool IsDotChain()
        {
            return true;
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString(name.AsName());
        }

        public override bool IsBrief()
        {
            return true;
        }

    }

}
