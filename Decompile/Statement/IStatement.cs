using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public abstract class IStatement
    {
        public string comment;
        public static void printSequence(Decompiler d, Output output, List<IStatement> stmts)
        {
            int n = stmts.Count;
            for (int i = 0; i < n; i++)
            {
                bool last = (i + 1 == n);
                IStatement stmt = stmts[i];
                if (stmt.beginsWithParen() && (i > 0 || d.getVersion().allowPreceedingSemicolon.Value))
                {
                    output.WriteString(";");
                }
                if (last)
                {
                    stmt.printTail(d, output);
                }
                else
                {
                    stmt.print(d, output);
                }
                if (!stmt.suppressNewline())
                {
                    output.WriteLine();
                }
            }
        }

        abstract public void print(Decompiler d, Output output);
        public abstract void walk(Walker w);
        public virtual void printTail(Decompiler d, Output output)
        {
            print(d, output);
        }

        public virtual void addComment(string comment)
        {
            this.comment = comment;
        }
        public virtual bool beginsWithParen()
        {
            return false;
        }
        public virtual bool suppressNewline()
        {
            return false;
        }
        public virtual bool useConstant(Function f, int index)
        {
            return false;
        }

    }

}
