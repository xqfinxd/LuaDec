using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{

    public class Vararg : IExpression
    {

        private readonly bool multiple;

        public Vararg(int length, bool multiple)
            : base(PRECEDENCE_ATOMIC)
        {
            this.multiple = multiple;
        }

        public override void walk(Walker w)
        {
            w.visitExpression(this);
        }

        public override int getConstantIndex()
        {
            return -1;
        }

        public override void print(Decompiler d, Output output)
        {
            //output.print("...");
            output.WriteString(multiple ? "..." : "(...)");
        }

        public override void printMultiple(Decompiler d, Output output)
        {
            output.WriteString(multiple ? "..." : "(...)");
        }

        public override bool isMultiple()
        {
            return multiple;
        }

    }

}
