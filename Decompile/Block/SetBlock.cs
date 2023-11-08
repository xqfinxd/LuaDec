using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using System;
using System.Collections.Generic;
using LuaDec.Parser;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class SetBlock : IBlock
    {
        public readonly int target;
        private Assignment assign;
        public readonly ICondition cond;
        private Registers r;
        private bool readonlyize = false;

        public SetBlock(LFunction function, ICondition cond, int target, int line, int begin, int end, Registers r)
             : base(function, begin, end, 2)
        {
            if (begin == end) throw new System.InvalidOperationException();
            this.target = target;
            this.cond = cond;
            this.r = r;
            if (target == -1)
            {
                throw new System.InvalidOperationException();
            }
            // System.output.println("-- set block " + begin + " .. " + end);
        }

        public override void walk(Walker w)
        {
            throw new System.InvalidOperationException();
        }

        public override void addStatement(IStatement statement)
        {
            if (!readonlyize && statement is Assignment)
            {
                this.assign = (Assignment)statement;
            }/* else if(statement is boolIndicator) {
      readonlyize = true;
    }*/
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void print(Decompiler d, Output output)
        {
            if (assign != null && assign.getFirstTarget() != null)
            {
                Assignment assignOut = new Assignment(assign.getFirstTarget(), getValue(), assign.getFirstLine());
                assignOut.print(d, output);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        public override bool breakable()
        {
            return false;
        }

        public override bool isContainer()
        {
            return true;
        }

        public override bool isEmpty()
        {
            return true;
        }

        public void useAssignment(Assignment assign)
        {
            this.assign = assign;
            // branch.useExpression(assign.getFirstValue());
        }

        public IExpression getValue()
        {
            return cond.asExpression(r);
        }

        private class SetBlockOperation_1 : IOperation
        {
            Assignment assign;

            public SetBlockOperation_1(Assignment assign, int line)
                : base(line)
            {
                this.assign = assign;
            }
            public override List<IStatement> process(Registers r, IBlock block)
            {
                return new List<IStatement> { assign };
            }
        }
        private class SetBlockOperation_2 : IOperation
        {
            int target;
            ICondition cond;

            public SetBlockOperation_2(int target, ICondition cond, int line)
                : base(line)
            {
                this.target = target;
                this.cond = cond;
            }
            public override List<IStatement> process(Registers r, IBlock block)
            {
                if (r.isLocal(target, line))
                {
                    return new List<IStatement>{
                        new Assignment(r.getTarget(target, line), cond
                        .asExpression(r), line)
                    };
                }
                r.setValue(target, line, cond.asExpression(r));
                return new List<IStatement>();
            }
        }
        public override IOperation process(Decompiler d)
        {
            if (ControlFlowHandler.verbose)
            {
                Console.WriteLine("set expression: ");
                cond.asExpression(r).print(d, new Output());
                Console.WriteLine();
            }
            if (assign != null)
            {
                assign.replaceValue(target, getValue());
                return new SetBlockOperation_1(assign, end - 1);
            }
            else
            {
                return new SetBlockOperation_2(target, cond, end - 1);
            }
        }
    }
}
