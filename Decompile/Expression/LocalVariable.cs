using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class LocalVariable : IExpression
    {

        private readonly Declaration decl;

        public LocalVariable(Declaration decl)
            : base(PRECEDENCE_ATOMIC)
        {
            this.decl = decl;
        }

        public override void walk(Walker w)
        {
            w.visitExpression(this);
        }

        public override int getConstantIndex()
        {
            return -1;
        }

        public override bool isDotChain()
        {
            return true;
        }

        public override void print(Decompiler d, Output output)
        {
            output.WriteString(decl.name);
        }

        public override bool isBrief()
        {
            return true;
        }

    }

}
