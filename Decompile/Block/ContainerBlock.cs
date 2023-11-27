using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile.Block
{
    public abstract class ContainerBlock : IBlock
    {
        protected readonly int closeLine;
        protected readonly CloseType closeType;
        protected readonly List<IStatement> statements;
        protected bool usingClose;

        public ContainerBlock(LFunction function, int begin, int end, CloseType closeType, int closeLine, int priority)
            : base(function, begin, end, priority)
        {
            this.closeType = closeType;
            this.closeLine = closeLine;
            usingClose = false;
            statements = new List<IStatement>(Math.Max(4, end - begin + 1));
        }

        public override void AddStatement(IStatement statement)
        {
            statements.Add(statement);
        }

        public override int GetCloseLine()
        {
            if (closeType == CloseType.None)
            {
                throw new System.InvalidOperationException();
            }
            return closeLine;
        }

        public override bool HasCloseLine()
        {
            return closeType != CloseType.None;
        }

        public override bool IsContainer()
        {
            return begin < end;
        }

        public override bool IsEmpty()
        {
            return statements.Count == 0;
        }

        public override void UseClose()
        {
            usingClose = true;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }
    }
}