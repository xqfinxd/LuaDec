using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Target
{
    public class TableTarget : ITarget
    {
        private readonly IExpression index;
        private readonly IExpression table;

        public TableTarget(IExpression table, IExpression index)
        {
            this.table = table;
            this.index = index;
        }

        public override bool BeginsWithParen()
        {
            return table.isUngrouped() || table.beginsWithParen();
        }

        public override bool IsFunctionName()
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

        public override void Walk(Walker w)
        {
            table.walk(w);
            index.walk(w);
        }

        public override void Write(Decompiler d, Output output, bool declare)
        {
            new TableReference(table, index).print(d, output);
        }

        public override void WriteMethod(Decompiler d, Output output)
        {
            table.print(d, output);
            output.WriteString(":");
            output.WriteString(index.asName());
        }
    }
}