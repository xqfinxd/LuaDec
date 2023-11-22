using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Block;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;
using LuaDec.Decompile.Target;

namespace LuaDec.Decompile.Operation
{
    public class TableSet : IOperation
    {

        private IExpression table;
        private IExpression index;
        private IExpression value;
        private bool isTable;
        private int timestamp;

        public TableSet(int line, IExpression table, IExpression index, IExpression value, bool isTable, int timestamp)
            : base(line)
        {
          this.table = table;
            this.index = index;
            this.value = value;
            this.isTable = isTable;
            this.timestamp = timestamp;
        }

        public override List<IStatement> process(Registers r, IBlock block)
        {
            // .isTableLiteral() is sufficient when there is debugging info
            if (!r.IsNoDebug && table.isTableLiteral() && (value.isMultiple() || table.isNewEntryAllowed()))
            {
                table.addEntry(new TableLiteral.Entry(index, value, !isTable, timestamp));
                return new List<IStatement>();
            }
            else
            {
                return new List<IStatement> { new Assignment(new TableTarget(table, index), value, line) };
            }
        }

    }

}
