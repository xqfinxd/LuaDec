using System;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{

    public abstract class ContainerBlock : IBlock
    {

        protected readonly List<IStatement> statements;
        protected readonly CloseType closeType;
        protected readonly int closeLine;
        protected bool usingClose;

        public ContainerBlock(LFunction function, int begin, int end, CloseType closeType, int closeLine, int priority)
            : base(function, begin, end, priority)
        {
            this.closeType = closeType;
            this.closeLine = closeLine;
            usingClose = false;
            statements = new List<IStatement>(Math.Max(4, end - begin + 1));
        }

        public override void walk(Walker w)
        {
            w.visitStatement(this);
            foreach (IStatement statement in statements)
            {
                statement.walk(w);
            }
        }

        public override bool isContainer()
        {
            return begin < end;
        }

        public override bool isEmpty()
        {
            return statements.Count == 0;
        }

        public override void addStatement(IStatement statement)
        {
            statements.Add(statement);
        }

        public override bool hasCloseLine()
        {
            return closeType != CloseType.NONE;
        }

        public override int getCloseLine()
        {
            if (closeType == CloseType.NONE)
            {
                throw new System.InvalidOperationException();
            }
            return closeLine;
        }

        public override void useClose()
        {
            usingClose = true;
        }

    }


}
