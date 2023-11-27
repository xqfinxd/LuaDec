using LuaDec.Decompile.Expression;

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

        public override IExpression AsExpression(Registers r)
        {
            return new BinaryExpression("and", left.AsExpression(r), right.AsExpression(r), IExpression.PRECEDENCE_AND, IExpression.ASSOCIATIVITY_NONE);
        }

        public override ICondition Inverse()
        {
            if (Invertible())
            {
                return new OrCondition(left.Inverse(), right.Inverse());
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

        public override bool IsOrCondition()
        {
            return false;
        }

        public override bool IsRegisterTest()
        {
            return false;
        }

        public override bool IsSplitable()
        {
            return true;
        }

        public override int Register()
        {
            return right.Register();
        }

        public override ICondition[] Split()
        {
            return new ICondition[] { left, right };
        }

        public override string ToString()
        {
            return left + " and " + right;
        }
    }
}