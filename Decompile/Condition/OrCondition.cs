using LuaDec.Decompile.Expression;

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

        public override IExpression AsExpression(Registers r)
        {
            return new BinaryExpression("or", left.AsExpression(r), right.AsExpression(r), IExpression.PRECEDENCE_OR, IExpression.ASSOCIATIVITY_NONE);
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

        public override bool IsOrCondition()
        {
            return true;
        }

        public override bool IsRegisterTest()
        {
            return false;
        }

        public override bool IsSplitable()
        {
            return false;
        }

        public override int Register()
        {
            return right.Register();
        }

        public override ICondition[] Split()
        {
            throw new System.InvalidOperationException();
        }

        public override string ToString()
        {
            return "(" + left + " or " + right + ")";
        }
    }
}