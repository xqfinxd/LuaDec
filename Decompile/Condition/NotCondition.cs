using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class NotCondition : ICondition
    {

        private ICondition cond;

        public NotCondition(ICondition cond)
        {
            this.cond = cond;
        }

        public override ICondition inverse()
        {
            return cond;
        }

        public override bool invertible()
        {
            return true;
        }

        public override int register()
        {
            return cond.register();
        }

        public override bool isRegisterTest()
        {
            return cond.isRegisterTest();
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
            return new UnaryExpression("not ", cond.asExpression(r), IExpression.PRECEDENCE_UNARY);
        }

        public override string ToString()
        {
            return "not (" + cond + ")";
        }

    }

}
