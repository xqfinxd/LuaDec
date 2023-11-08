using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class AndCondition : ICondition
    {

        private ICondition left;
        private ICondition right;

        public AndCondition(ICondition left, ICondition right)
        {
            this.left = left;
            this.right = right;
        }

        public override ICondition inverse()
        {
            if (invertible())
            {
                return new OrCondition(left.inverse(), right.inverse());
            }
            else
            {
                return new NotCondition(this);
            }
        }

        public override bool invertible()
        {
            return right.invertible();
        }

        public override int register()
        {
            return right.register();
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
            return true;
        }

        public override ICondition[] split()
        {
            return new ICondition[] { left, right };
        }

        public override IExpression asExpression(Registers r)
        {
            return new BinaryExpression("and", left.asExpression(r), right.asExpression(r), IExpression.PRECEDENCE_AND, IExpression.ASSOCIATIVITY_NONE);
        }

        public override string ToString()
        {
            return left + " and " + right;
        }

    }

}
