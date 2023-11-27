using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile.Block
{
    public class SetBlock : IBlock
    {
        private class AssignOperation : IOperation
        {
            private Assignment assign;

            public AssignOperation(Assignment assign, int line)
                : base(line)
            {
                this.assign = assign;
            }

            public override List<IStatement> Process(Registers r, IBlock block)
            {
                return new List<IStatement> { assign };
            }
        }

        private class NoAssignOperation : IOperation
        {
            private ICondition cond;
            private int target;

            public NoAssignOperation(int target, ICondition cond, int line)
                : base(line)
            {
                this.target = target;
                this.cond = cond;
            }

            public override List<IStatement> Process(Registers r, IBlock block)
            {
                if (r.IsLocal(target, line))
                {
                    return new List<IStatement>{
                        new Assignment(r.GetTarget(target, line), cond
                        .AsExpression(r), line)
                    };
                }
                r.SetValue(target, line, cond.AsExpression(r));
                return new List<IStatement>();
            }
        }

        private Assignment assign;
        private Registers r;
        private bool readonlyize = false;
        public readonly ICondition cond;
        public readonly int target;

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

        public override void AddStatement(IStatement statement)
        {
            if (!readonlyize && statement is Assignment)
            {
                this.assign = (Assignment)statement;
            }
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public IExpression getValue()
        {
            return cond.AsExpression(r);
        }

        public override bool IsContainer()
        {
            return true;
        }

        public override bool IsEmpty()
        {
            return true;
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override IOperation Process(Decompiler d)
        {
            if (ControlFlowHandler.Verbose)
            {
                Console.WriteLine("set expression: ");
                cond.AsExpression(r).Write(d, new Output());
                Console.WriteLine();
            }
            if (assign != null)
            {
                assign.ReplaceValue(target, getValue());
                return new AssignOperation(assign, end - 1);
            }
            else
            {
                return new NoAssignOperation(target, cond, end - 1);
            }
        }

        public void useAssignment(Assignment assign)
        {
            this.assign = assign;
            // branch.useExpression(assign.getFirstValue());
        }

        public override void Walk(Walker w)
        {
            throw new System.InvalidOperationException();
        }

        public override void Write(Decompiler d, Output output)
        {
            if (assign != null && assign.GetFirstTarget() != null)
            {
                Assignment assignOut = new Assignment(assign.GetFirstTarget(), getValue(), assign.GetFirstLine());
                assignOut.Write(d, output);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }
    }
}