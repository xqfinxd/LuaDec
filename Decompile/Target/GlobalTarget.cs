using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Target
{
    public class GlobalTarget : ITarget
    {

        private readonly IExpression name;

        public GlobalTarget(ConstantExpression name)
        {
            this.name = name;
        }

        public override void walk(Walker w)
        {
            name.walk(w);
        }

        public override void print(Decompiler d, Output output, bool declare)
        {
            output.WriteString(name.asName());
        }

        public override void printMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }

    }

}
