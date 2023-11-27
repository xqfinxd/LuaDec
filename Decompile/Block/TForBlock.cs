using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using LuaDec.Parser;
using System.Collections.Generic;

namespace LuaDec.Decompile.Block
{
    public class TForBlock : ContainerBlock
    {
        private ITarget[] targets;
        private IExpression[] values;
        protected readonly int explicitRegisterFirst;
        protected readonly int explicitRegisterLast;
        protected readonly int explicitScopeBegin;
        protected readonly int explicitScopeEnd;
        protected readonly int innerScopeEnd;
        protected readonly int internalRegisterFirst;
        protected readonly int internalRegisterLast;
        protected readonly int internalScopeBegin;
        protected readonly int internalScopeEnd;

        public TForBlock(LFunction function, int begin, int end,
          int internalRegisterFirst, int internalRegisterLast,
          int explicitRegisterFirst, int explicitRegisterLast,
          int internalScopeBegin, int internalScopeEnd,
          int explicitScopeBegin, int explicitScopeEnd,
          int innerScopeEnd
        )
            : base(function, begin, end, CloseType.None, -1, -1)
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

        public static TForBlock Make50(LFunction function, int begin, int end, int register, int length, bool innerClose)
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

        public static TForBlock Make51(LFunction function, int begin, int end, int register, int length, bool forvarClose, bool innerClose)
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

        public static TForBlock Make54(LFunction function, int begin, int end, int register, int length, bool forvarClose)
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

        public override bool Breakable()
        {
            return true;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public List<ITarget> GetTargets(Registers r)
        {
            List<ITarget> targets = new List<ITarget>(explicitRegisterLast - explicitRegisterFirst + 1);
            for (int register = explicitRegisterFirst; register <= explicitRegisterLast; register++)
            {
                targets.Add(r.GetTarget(register, begin - 1));
            }
            return targets;
        }

        public void HandleVariableDeclarations(Registers r)
        {
            for (int register = internalRegisterFirst; register <= internalRegisterLast; register++)
            {
                r.SetInternalLoopVariable(register, internalScopeBegin, internalScopeEnd);
            }
            for (int register = explicitRegisterFirst; register <= explicitRegisterLast; register++)
            {
                r.SetExplicitLoopVariable(register, explicitScopeBegin, explicitScopeEnd);
            }
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override void Resolve(Registers r)
        {
            List<ITarget> targets = GetTargets(r);
            List<IExpression> values = new List<IExpression>(3);
            for (int register = internalRegisterFirst; register <= internalRegisterLast; register++)
            {
                IExpression value = r.GetValue(register, begin - 1);
                values.Add(value);
                if (value.IsMultiple()) break;
            }

            this.targets = new ITarget[targets.Count];
            this.values = new IExpression[values.Count];
        }

        public override int ScopeEnd()
        {
            return innerScopeEnd;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            foreach (IExpression expression in values)
            {
                expression.Walk(w);
            }
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("for ");
            targets[0].Write(d, output, false);
            for (int i = 1; i < targets.Length; i++)
            {
                output.WriteString(", ");
                targets[i].Write(d, output, false);
            }
            output.WriteString(" in ");
            values[0].Write(d, output);
            for (int i = 1; i < values.Length; i++)
            {
                output.WriteString(", ");
                values[i].Write(d, output);
            }
            output.WriteString(" do");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }
    }
}