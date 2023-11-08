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

        public override ICondition inverse()
        {
            return new NotCondition(this);
        }

        public override bool invertible()
        {
            return false;
        }

        public override int register()
        {
            return reg;
        }

        public override bool isRegisterTest()
        {
            return true;
        }

        public override bool isOrCondition()
        {
            return false;
        }

        public override bool isSplitable()
        {
            return false;
        }

        public override ICondition[] split()
        {
            throw new System.InvalidOperationException();
        }

        public override IExpression asExpression(Registers r)
        {
            return r.getExpression(register(), line);
        }

        public override string ToString()
        {
            return "(" + reg + ")";
        }

    }

}
