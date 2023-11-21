using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Statement
{
    public class Declare : IStatement
    {
        private readonly List<Declaration> decls;
        public Declare(List<Declaration> decls)
        {
            this.decls = decls;
        }
        public override void walk(Walker w)
        {
            w.visitStatement(this);
        }
        public override void print(Decompiler d, Output output)
        {
            output.WriteString("local ");
            output.WriteString(decls[0].name);
            for (int i = 1; i < decls.Count; i++)
            {
                output.WriteString(", ");
                output.WriteString(decls[i].name);
            }
        }
    }
}
