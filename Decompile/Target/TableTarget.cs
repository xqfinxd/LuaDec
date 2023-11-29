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
            return table.IsUngrouped() || table.BeginsWithParen();
        }

        public override bool IsFunctionName()
        {
            if (!index.IsIdentifier())
            {
                return false;
            }
            if (!table.IsDotChain())
            {
                return false;
            }
            return true;
        }

        public override void Walk(Walker w)
        {
            table.Walk(w);
            index.Walk(w);
        }

        public override void Write(Decompiler d, Output output, bool declare)
        {
            new TableReference(table, index).Write(d, output);
        }

        public override void WriteMethod(Decompiler d, Output output)
        {
            table.Write(d, output);
            output.Write(":");
            output.Write(index.AsName());
        }
    }
}