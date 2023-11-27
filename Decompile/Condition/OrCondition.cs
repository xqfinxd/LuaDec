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

        public override ICondition Inverse()
        {
            if (Invertible())
            {
                return new AndCondition(left.Inverse(), right.Inverse());
            }
            else
            {
                return new NotCondition(this);
            }
        }

        public override bool Invertible()
        {
            return right.Invertible();
        }

        public override int Register()
        {
            return right.Register();
        }

        public override bool IsRegisterTest()
        {
            return false;
        }

        public override bool IsOrCondition()
        {
            return true;
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
            return new BinaryExpression("or", left.AsExpression(r), right.AsExpression(r), IExpression.PRECEDENCE_OR, IExpression.ASSOCIATIVITY_NONE);
        }

        public override string ToString()
        {
            return "(" + left + " or " + right + ")";
        }

    }

}
