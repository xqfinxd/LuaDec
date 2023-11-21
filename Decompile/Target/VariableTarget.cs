using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Target
{
    public class VariableTarget : ITarget
    {

        public readonly Declaration decl;

        public VariableTarget(Declaration decl)
        {
            this.decl = decl;
        }

        public override void walk(Walker w) { }

        public override void print(Decompiler d, Output output, bool declare)
        {
            output.WriteString(decl.name);
            if (declare && decl.tbc)
            {
                output.WriteString(" <close>");
            }
        }

        public override void printMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }

        public override bool isDeclaration(Declaration decl)
        {
            return this.decl == decl;
        }

        public override bool isLocal()
        {
            return true;
        }

        public override int getIndex()
        {
            return decl.register;
        }

        public bool equals(Object obj)
        {
            if (obj is VariableTarget)
            {
                VariableTarget t = (VariableTarget)obj;
                return decl == t.decl;
            }
            else
            {
                return false;
            }
        }

    }

}
