using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class UpvalueSet : IOperation
    {
        private UpvalueTarget target;
        private IExpression value;

        public UpvalueSet(int line, string upvalue, IExpression value)
            : base(line)
        {
            target = new UpvalueTarget(upvalue);
            this.value = value;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            return new List<IStatement> { new Assignment(target, value, line) };
        }
    }
}