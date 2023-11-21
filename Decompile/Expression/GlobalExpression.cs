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

        public override void walk(Walker w)
        {
            w.visitExpression(this);
            name.walk(w);
        }

        public override int getConstantIndex()
        {
            return index;
        }

        public override bool isDotChain()
        {
            return true;
        }

        public override void print(Decompiler d, Output output)
        {
            output.WriteString(name.asName());
        }

        public override bool isBrief()
        {
            return true;
        }

    }

}
