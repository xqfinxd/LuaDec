using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class OrCondition : ICondition
    {

        private ICondition left;
        private ICondition right;

        public OrCondition(ICondition left, ICondition right)
        {
            this.left = left;
            this.right = right;
        }

        public override ICondition inverse()
        {
            if (invertible())
            {
                return new AndCondition(left.inverse(), right.inverse());
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
            return true;
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
            return new BinaryExpression("or", left.asExpression(r), right.asExpression(r), IExpression.PRECEDENCE_OR, IExpression.ASSOCIATIVITY_NONE);
        }

        public override string ToString()
        {
            return "(" + left + " or " + right + ")";
        }

    }

}
