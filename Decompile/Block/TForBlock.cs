using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System;
using System.Collections.Generic;
using LuaDec.Parser;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaDec.Decompile.Target;

namespace LuaDec.Decompile.Block
{
    public class TForBlock : ContainerBlock
    {

        protected readonly int internalRegisterFirst;
        protected readonly int internalRegisterLast;

        protected readonly int explicitRegisterFirst;
        protected readonly int explicitRegisterLast;

        protected readonly int internalScopeBegin;
        protected readonly int internalScopeEnd;

        protected readonly int explicitScopeBegin;
        protected readonly int explicitScopeEnd;

        protected readonly int innerScopeEnd;

        private ITarget[] targets;
        private IExpression[] values;

        public static TForBlock make50(LFunction function, int begin, int end, int register, int length, bool innerClose)
        {
            int innerScopeEnd = end - 3;
            if (innerClose)
            {
                innerScopeEnd--;
            }
            return new TForBlock(
              function, begin, end,
              register, register + 1, register + 2, register + 1 + length,
              begin - 1, end - 1,
              begin - 1, end - 1,
              innerScopeEnd
            );
        }

        public static TForBlock make51(LFunction function, int begin, int end, int register, int length, bool forvarClose, bool innerClose)
        {
            int explicitScopeEnd = end - 3;
            int innerScopeEnd = end - 3;
            if (forvarClose)
            {
                explicitScopeEnd--;
                innerScopeEnd--;
            }
            if (innerClose)
            {
                innerScopeEnd--;
            }
            return new TForBlock(
              function, begin, end,
              register, register + 2, register + 3, register + 2 + length,
              begin - 2, end - 1,
              begin - 1, explicitScopeEnd,
              innerScopeEnd
            );
        }

        public static TForBlock make54(LFunction function, int begin, int end, int register, int length, bool forvarClose)
        {
            int innerScopeEnd = end - 3;
            if (forvarClose)
            {
                innerScopeEnd--;
            }
            return new TForBlock(
              function, begin, end,
              register, register + 3, register + 4, register + 3 + length,
              begin - 2, end,
              begin - 1, end - 3,
              innerScopeEnd
            );
        }

        public TForBlock(LFunction function, int begin, int end,
          int internalRegisterFirst, int internalRegisterLast,
          int explicitRegisterFirst, int explicitRegisterLast,
          int internalScopeBegin, int internalScopeEnd,
          int explicitScopeBegin, int explicitScopeEnd,
          int innerScopeEnd
        )
            : base(function, begin, end, CloseType.NONE, -1, -1)
        {
            this.internalRegisterFirst = internalRegisterFirst;
            this.internalRegisterLast = internalRegisterLast;
            this.explicitRegisterFirst = explicitRegisterFirst;
            this.explicitRegisterLast = explicitRegisterLast;
            this.internalScopeBegin = internalScopeBegin;
            this.internalScopeEnd = internalScopeEnd;
            this.explicitScopeBegin = explicitScopeBegin;
            this.explicitScopeEnd = explicitScopeEnd;
            this.innerScopeEnd = innerScopeEnd;
        }

        public List<ITarget> getTargets(Registers r)
        {
            List<ITarget> targets = new List<ITarget>(explicitRegisterLast - explicitRegisterFirst + 1);
            for (int register = explicitRegisterFirst; register <= explicitRegisterLast; register++)
            {
                targets.Add(r.getTarget(register, begin - 1));
            }
            return targets;
        }

        public void handleVariableDeclarations(Registers r)
        {
            for (int register = internalRegisterFirst; register <= internalRegisterLast; register++)
            {
                r.setInternalLoopVariable(register, internalScopeBegin, internalScopeEnd);
            }
            for (int register = explicitRegisterFirst; register <= explicitRegisterLast; register++)
            {
                r.setExplicitLoopVariable(register, explicitScopeBegin, explicitScopeEnd);
            }
        }

        public override void resolve(Registers r)
        {
            List<ITarget> targets = getTargets(r);
            List<IExpression> values = new List<IExpression>(3);
            for (int register = internalRegisterFirst; register <= internalRegisterLast; register++)
            {
                IExpression value = r.getValue(register, begin - 1);
                values.Add(value);
                if (value.isMultiple()) break;
            }

            this.targets = new ITarget[targets.Count];
            this.values = new IExpression[values.Count];
        }

        public override void walk(Walker w)
        {
            w.visitStatement(this);
            foreach (IExpression expression in values)
            {
                expression.walk(w);
            }
            foreach (IStatement statement in statements)
            {
                statement.walk(w);
            }
        }

        public override int scopeEnd()
        {
            return innerScopeEnd;
        }

        public override bool breakable()
        {
            return true;
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
            output.WriteString("for ");
            targets[0].print(d, output, false);
            for (int i = 1; i < targets.Length; i++)
            {
                output.WriteString(", ");
                targets[i].print(d, output, false);
            }
            output.WriteString(" in ");
            values[0].print(d, output);
            for (int i = 1; i < values.Length; i++)
            {
                output.WriteString(", ");
                values[i].print(d, output);
            }
            output.WriteString(" do");
            output.WriteLine();
            output.Indent();
            IStatement.printSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }

    }

}
