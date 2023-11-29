using System;

namespace LuaDec.Decompile.Expression
{
    public class BinaryExpression : IExpression
    {
        private readonly int associativity;
        private readonly IExpression left;
        private readonly string op;
        private readonly IExpression right;
        public string Op => op;

        public BinaryExpression(string op, IExpression left, IExpression right, int precedence, int associativity)
            : base(precedence)
        {
            this.op = op;
            this.left = left;
            this.right = right;
            this.associativity = associativity;
        }

        private bool LeftGroup()
        {
            return precedence > left.precedence || (precedence == left.precedence && associativity == ASSOCIATIVITY_RIGHT);
        }

        private bool RightGroup()
        {
            return precedence > right.precedence || (precedence == right.precedence && associativity == ASSOCIATIVITY_LEFT);
        }

        public static BinaryExpression ReplaceRight(BinaryExpression template, IExpression replacement)
        {
            return new BinaryExpression(template.op, template.left, replacement, template.precedence, template.associativity);
        }

        public override bool BeginsWithParen()
        {
            return LeftGroup() || left.BeginsWithParen();
        }

        public override int GetConstantIndex()
        {
            return Math.Max(left.GetConstantIndex(), right.GetConstantIndex());
        }

        public override bool IsUngrouped()
        {
            return !BeginsWithParen();
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            left.Walk(w);
            right.Walk(w);
        }

        public override void Write(Decompiler d, Output output)
        {
            bool leftGroup = LeftGroup();
            bool rightGroup = RightGroup();
            if (leftGroup) output.Write("(");
            left.Write(d, output);
            if (leftGroup) output.Write(")");
            output.Write(" ");
            output.Write(op);
            output.Write(" ");
            if (rightGroup) output.Write("(");
            right.Write(d, output);
            if (rightGroup) output.Write(")");
        }
    }
}