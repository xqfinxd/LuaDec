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

        public override void walk(Walker w)
        {
            w.visitExpression(this);
            table.walk(w);
            index.walk(w);
        }

        public override int getConstantIndex()
        {
            return Math.Max(table.getConstantIndex(), index.getConstantIndex());
        }

        public override void print(Decompiler d, Output output)
        {
            bool isGlobal = table.isEnvironmentTable(d) && index.isIdentifier();
            if (!isGlobal)
            {
                if (table.isUngrouped())
                {
                    output.WriteString("(");
                    table.print(d, output);
                    output.WriteString(")");
                }
                else
                {
                    table.print(d, output);
                }
            }
            if (index.isIdentifier())
            {
                if (!isGlobal)
                {
                    output.WriteString(".");
                }
                output.WriteString(index.asName());
            }
            else
            {
                output.WriteString("[");
                index.printBraced(d, output);
                output.WriteString("]");
            }
        }

        public override bool isDotChain()
        {
            return index.isIdentifier() && table.isDotChain();
        }

        public override bool isMemberAccess()
        {
            return index.isIdentifier();
        }

        public override bool beginsWithParen()
        {
            return table.isUngrouped() || table.beginsWithParen();
        }

        public override IExpression getTable()
        {
            return table;
        }

        public override string getField()
        {
            return index.asName();
        }


    }

}
