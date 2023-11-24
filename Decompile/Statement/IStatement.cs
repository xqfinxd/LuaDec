using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public abstract class IStatement
    {
        private string comment;

        public string Comment { get => comment; set => comment = value; }

        public static void WriteSequence(Decompiler d, Output output, List<IStatement> stmts)
        {
            int n = stmts.Count;
            for (int i = 0; i < n; i++)
            {
                bool last = (i + 1 == n);
                IStatement stmt = stmts[i];
                if (stmt.BeginsWithParen() && (i > 0 || d.GetVersion().allowPreceedingSemicolon.Value))
                {
                    output.WriteString(";");
                }
                if (last)
                {
                    stmt.WriteTail(d, output);
                }
                else
                {
                    stmt.Write(d, output);
                }
                if (!stmt.SuppressNewline())
                {
                    output.WriteLine();
                }
            }
        }

        public virtual void AddComment(string comment)
        {
            this.comment = comment;
        }

        public virtual bool BeginsWithParen()
        {
            return false;
        }

        public abstract void Write(Decompiler d, Output output);

        public virtual void WriteTail(Decompiler d, Output output)
        {
            Write(d, output);
        }

        public virtual bool SuppressNewline()
        {
            return false;
        }

        public virtual bool UseConstant(Function f, int index)
        {
            return false;
        }

        public abstract void Walk(Walker w);
    }
}