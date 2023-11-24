using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class RegisterSet : IOperation
    {
        public readonly int register;
        public readonly IExpression value;

        public RegisterSet(int line, int register, IExpression value)
            : base(line)
        {
            this.register = register;
            this.value = value;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            r.SetValue(register, line, value);
            if (r.IsAssignable(register, line))
            {
                return new List<IStatement> { new Assignment(r.GetTarget(register, line), value, line) };
            }
            else
            {
                return new List<IStatement>();
            }
        }
    }
}