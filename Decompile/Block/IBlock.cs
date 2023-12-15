using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile.Block
{
    public abstract class IBlock : IStatement, IComparable<IBlock>
    {
        private class DefaultOperation : IOperation
        {
            private IStatement statement;

            public DefaultOperation(IStatement statement, int line)
                : base(line)
            {
                this.statement = statement;
            }

            public override List<IStatement> Process(Registers r, IBlock block)
            {
                return new List<IStatement> { statement };
            }
        }

        private readonly int priority;
        protected readonly LFunction function;
        protected bool scopeUsed = false;
        public int begin;
        public int closeRegister;
        public int end;
        public bool loopRedirectAdjustment = false;

        public IBlock(LFunction function, int begin, int end, int priority)
        {
            this.function = function;
            this.begin = begin;
            this.end = end;
            this.closeRegister = -1;
            this.priority = priority;
        }

        public abstract void AddStatement(IStatement statement);

        public virtual bool AllowsPreDeclare()
        {
            return false;
        }

        public abstract bool Breakable();

        public virtual int CompareTo(IBlock block)
        {
            if (begin < block.begin)
            {
                return -1;
            }
            else if (begin == block.begin)
            {
                if (end < block.end)
                {
                    return 1;
                }
                else if (end == block.end)
                {
                    return priority - block.priority;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return 1;
            }
        }

        public bool Contains(IBlock block)
        {
            return Contains(block.begin, block.end);
        }

        public bool Contains(int line)
        {
            return begin <= line && line < end;
        }

        public bool Contains(int begin, int end)
        {
            return this.begin <= begin && this.end >= end;
        }

        public virtual int GetCloseLine()
        {
            throw new System.InvalidOperationException();
        }

        public abstract bool HasHeader();

        public abstract int GetLoopback();

        public virtual int GetUnprotectedLine()
        {
            throw new System.NotImplementedException(ToString());
        }

        public virtual int GetUnprotectedTarget()
        {
            throw new System.NotImplementedException(ToString());
        }

        public virtual bool HasCloseLine()
        {
            return false;
        }

        public abstract bool IsContainer();

        public abstract bool IsEmpty();

        public virtual bool IsSplitable()
        {
            return false;
        }

        public abstract bool IsUnprotected();

        public virtual IOperation Process(Decompiler d)
        {
            IStatement statement = this;
            return new DefaultOperation(statement, end - 1);
        }

        public virtual void Resolve(Registers r)
        { }

        public virtual int ScopeEnd()
        {
            return end - 1;
        }

        public virtual IBlock[] Split(int line, CloseType closeType)
        {
            throw new System.NotImplementedException();
        }

        public virtual void UseClose()
        {
            throw new System.NotImplementedException();
        }

        public virtual void UseScope()
        {
            scopeUsed = true;
        }
    }
}