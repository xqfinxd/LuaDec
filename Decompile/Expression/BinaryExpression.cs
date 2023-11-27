using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class BinaryExpression : IExpression
    {

        private readonly string op;
        private readonly IExpression left;
        private readonly IExpression right;
        private readonly int associativity;

        public static BinaryExpression replaceRight(BinaryExpression template, IExpression replacement)
        {
            return new BinaryExpression(template.op, template.left, replacement, template.precedence, template.associativity);
        }

        public BinaryExpression(string op, IExpression left, IExpression right, int precedence, int associativity)
            : base(precedence)
        {
            this.op = op;
            this.left = left;
            this.right = right;
            this.associativity = associativity;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            left.Walk(w);
            right.Walk(w);
        }

        public override bool IsUngrouped()
        {
            return !BeginsWithParen();
        }

        public override int GetConstantIndex()
        {
            return Math.Max(left.GetConstantIndex(), right.GetConstantIndex());
        }

        public override bool BeginsWithParen()
        {
            return LeftGroup() || left.BeginsWithParen();
        }

        public override void Write(Decompiler d, Output output)
        {
            bool leftGroup = LeftGroup();
            bool rightGroup = RightGroup();
            if (leftGroup) output.WriteString("(");
            left.Write(d, output);
            if (leftGroup) output.WriteString(")");
            output.WriteString(" ");
            output.WriteString(op);
            output.WriteString(" ");
            if (rightGroup) output.WriteString("(");
            right.Write(d, output);
            if (rightGroup) output.WriteString(")");
        }

        public string getOp()
        {
            return op;
        }

        private bool LeftGroup()
        {
            return precedence > left.precedence || (precedence == left.precedence && associativity == ASSOCIATIVITY_RIGHT);
        }

        private bool RightGroup()
        {
            return precedence > right.precedence || (precedence == right.precedence && associativity == ASSOCIATIVITY_LEFT);
        }

    }

}
