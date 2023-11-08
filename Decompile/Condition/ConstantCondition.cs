using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class ConstantCondition : ICondition
    {

        private int reg;
        private bool value;

        public ConstantCondition(int reg, bool value)
        {
            this.reg = reg;
            this.value = value;
        }

        public override ICondition inverse()
        {
            return new ConstantCondition(reg, !value);
        }

        public override bool invertible()
        {
            return true;
        }

        public override int register()
        {
            return reg;
        }

        public override bool isRegisterTest()
        {
            return false;
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
            return ConstantExpression.createbool(value);
        }

    }

}
