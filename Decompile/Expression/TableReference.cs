using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class TableReference : IExpression
    {

        private readonly IExpression table;
        private readonly IExpression index;

        public TableReference(IExpression table, IExpression index)
            : base(PRECEDENCE_ATOMIC)
        {
            this.table = table;
            this.index = index;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            table.Walk(w);
            index.Walk(w);
        }

        public override int GetConstantIndex()
        {
            return Math.Max(table.GetConstantIndex(), index.GetConstantIndex());
        }

        public override void Write(Decompiler d, Output output)
        {
            bool isGlobal = table.IsEnvironmentTable(d) && index.IsIdentifier();
            if (!isGlobal)
            {
                if (table.IsUngrouped())
                {
                    output.WriteString("(");
                    table.Write(d, output);
                    output.WriteString(")");
                }
                else
                {
                    table.Write(d, output);
                }
            }
            if (index.IsIdentifier())
            {
                if (!isGlobal)
                {
                    output.WriteString(".");
                }
                output.WriteString(index.AsName());
            }
            else
            {
                output.WriteString("[");
                index.WriteBraced(d, output);
                output.WriteString("]");
            }
        }

        public override bool IsDotChain()
        {
            return index.IsIdentifier() && table.IsDotChain();
        }

        public override bool IsMemberAccess()
        {
            return index.IsIdentifier();
        }

        public override bool BeginsWithParen()
        {
            return table.IsUngrouped() || table.BeginsWithParen();
        }

        public override IExpression GetTable()
        {
            return table;
        }

        public override string GetField()
        {
            return index.AsName();
        }


    }

}
