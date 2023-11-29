using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public class Declare : IStatement
    {
        private readonly List<Declaration> decls;

        public Declare(List<Declaration> decls)
        {
            this.decls = decls;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write("local ");
            output.Write(decls[0].name);
            for (int i = 1; i < decls.Count; i++)
            {
                output.Write(", ");
                output.Write(decls[i].name);
            }
        }
    }
}