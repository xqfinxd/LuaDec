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
            /*
            if(value.isMultiple()) {
              System.output.println("-- multiple @" + register);
            }
            */
        }

        public override List<IStatement> process(Registers r, IBlock block)
        {
            //System.output.println("-- processing register set " + register + "@" + line);
            r.setValue(register, line, value);
            /*
            if(value.isMultiple()) {
              System.output.println("-- process multiple @" + register);
            }
            */
            if (r.isAssignable(register, line))
            {
                //System.output.println("-- assignment!");
                return new List<IStatement> { new Assignment(r.getTarget(register, line), value, line) };
            }
            else
            {
                return new List<IStatement>();
            }
        }

    }

}
