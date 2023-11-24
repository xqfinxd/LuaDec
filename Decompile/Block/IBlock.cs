using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    abstract public class IBlock : IStatement, IComparable<IBlock>
    {

        protected readonly LFunction function;
        public int begin;
        public int end;
        public int closeRegister;
        private readonly int priority;
        public bool loopRedirectAdjustment = false;
        protected bool scopeUsed = false;

        public IBlock(LFunction function, int begin, int end, int priority)
        {
            this.function = function;
            this.begin = begin;
            this.end = end;
            this.closeRegister = -1;
            this.priority = priority;
        }

        public abstract void addStatement(IStatement statement);

        public virtual void resolve(Registers r) { }

        public virtual bool contains(IBlock block)
        {
            return begin <= block.begin && end >= block.end;
        }

        public virtual bool contains(int line)
        {
            return begin <= line && line < end;
        }

        public virtual int scopeEnd()
        {
            return end - 1;
        }

        public virtual void useScope()
        {
            scopeUsed = true;
        }

        public virtual bool hasCloseLine()
        {
            return false;
        }

        public virtual int getCloseLine()
        {
            throw new System.InvalidOperationException();
        }

        public virtual void useClose()
        {
            throw new System.InvalidOperationException();
        }

        /**
         * An unprotected block is one that ends in a JMP instruction.
         * If this is the case, any inner statement that tries to jump
         * to the end of this block will be redirected.
         * 
         * (One of the Lua compiler's few optimizations is that is changes
         * any JMP that targets another JMP to the ultimate target. This
         * is what I call redirection.)
         */
        abstract public bool isUnprotected();

        public virtual int getUnprotectedTarget()
        {
            throw new System.InvalidOperationException(ToString());
        }

        public virtual int getUnprotectedLine()
        {
            throw new System.InvalidOperationException(ToString());
        }

        abstract public int getLoopback();

        abstract public bool breakable();

        abstract public bool isContainer();

        abstract public bool isEmpty();

        public virtual bool allowsPreDeclare()
        {
            return false;
        }

        public virtual bool isSplitable()
        {
            return false;
        }

        public virtual IBlock[] split(int line, CloseType closeType)
        {
            throw new System.InvalidOperationException();
        }

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

        private class BlockOperation_ : IOperation
        {
            IStatement statement;
            public BlockOperation_(IStatement statement, int line)
                : base(line)
            {
                this.statement = statement;
            }
            public override List<IStatement> Process(Registers r, IBlock block)
            {
                return new List<IStatement> { statement };
            }
        }
        public virtual IOperation process(Decompiler d)
        {
            IStatement statement = this;
            return new BlockOperation_(statement, end - 1);
        }
    }
}
