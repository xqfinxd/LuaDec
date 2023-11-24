using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class UpvalueExpression : IExpression
    {

        private readonly string name;

        public UpvalueExpression(string name)
            : base(PRECEDENCE_ATOMIC)
        {
            this.name = name;
        }

        public override void walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override int getConstantIndex()
        {
            return -1;
        }

        public override bool isDotChain()
        {
            return true;
        }

        public override void print(Decompiler d, Output output) {
            output.WriteString(name);
        }

        public override bool isBrief()
        {
            return true;
        }

        public override bool isEnvironmentTable(Decompiler d)
        {
            return d.GetVersion().IsEnvTable(name);
        }

    }

}
