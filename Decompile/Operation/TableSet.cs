using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class TableSet : IOperation
    {
        private IExpression index;
        private bool isTable;
        private IExpression table;
        private int timestamp;
        private IExpression value;

        public TableSet(int line, IExpression table, IExpression index, IExpression value, bool isTable, int timestamp)
            : base(line)
        {
            this.table = table;
            this.index = index;
            this.value = value;
            this.isTable = isTable;
            this.timestamp = timestamp;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            if (!r.IsNoDebug && table.IsTableLiteral() && (value.IsMultiple() || table.IsNewEntryAllowed()))
            {
                table.AddEntry(new TableLiteral.Entry(index, value, !isTable, timestamp));
                return new List<IStatement>();
            }
            else
            {
                return new List<IStatement> { new Assignment(new TableTarget(table, index), value, line) };
            }
        }
    }
}