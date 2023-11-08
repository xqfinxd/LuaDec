using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Target
{
    public class TableTarget : ITarget
    {

        private readonly IExpression table;
        private readonly IExpression index;

        public TableTarget(IExpression table, IExpression index)
        {
            this.table = table;
            this.index = index;
        }

        public override void walk(Walker w)
        {
            table.walk(w);
            index.walk(w);
        }

        public override void print(Decompiler d, Output output, bool declare)
        {
            new TableReference(table, index).print(d, output);
        }

        public override void printMethod(Decompiler d, Output output)
        {
            table.print(d, output);
            output.print(":");
            output.print(index.asName());
        }

        public override bool isFunctionName()
        {
            if (!index.isIdentifier())
            {
                return false;
            }
            if (!table.isDotChain())
            {
                return false;
            }
            return true;
        }

        public override bool beginsWithParen()
        {
            return table.isUngrouped() || table.beginsWithParen();
        }

    }

}
