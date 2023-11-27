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

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override int GetConstantIndex()
        {
            return -1;
        }

        public override void Write(Decompiler d, Output output)
        {
            //output.print("...");
            output.WriteString(multiple ? "..." : "(...)");
        }

        public override void WriteMultiple(Decompiler d, Output output)
        {
            output.WriteString(multiple ? "..." : "(...)");
        }

        public override bool IsMultiple()
        {
            return multiple;
        }

    }

}
