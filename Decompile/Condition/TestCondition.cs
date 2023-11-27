using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class TestCondition : ICondition
    {

        private int line;
        private int reg;

        public TestCondition(int line, int reg)
        {
            this.line = line;
            this.reg = reg;
        }

        public override ICondition Inverse()
        {
            return new NotCondition(this);
        }

        public override bool Invertible()
        {
            return false;
        }

        public override int Register()
        {
            return reg;
        }

        public override bool IsRegisterTest()
        {
            return true;
        }

        public override bool IsOrCondition()
        {
            return false;
        }

        public override bool IsSplitable()
        {
            return false;
        }

        public override ICondition[] Split()
        {
            throw new System.InvalidOperationException();
        }

        public override IExpression AsExpression(Registers r)
        {
            return r.GetExpression(Register(), line);
        }

        public override string ToString()
        {
            return "(" + reg + ")";
        }

    }

}
