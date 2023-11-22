using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Statement
{
    public class Label : IStatement
    {
        private string name;
        public Label(int line)
        {
            name = "lbl_" + line;
        }
        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
        }
        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("::" + name + "::");
        }
    }
}
