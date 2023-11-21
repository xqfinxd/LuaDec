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

        public override void walk(Walker w)
        {
            w.VisitExpression(this);
            left.walk(w);
            right.walk(w);
        }

        public override bool isUngrouped()
        {
            return !beginsWithParen();
        }

        public override int getConstantIndex()
        {
            return Math.Max(left.getConstantIndex(), right.getConstantIndex());
        }

        public override bool beginsWithParen()
        {
            return LeftGroup() || left.beginsWithParen();
        }

        public override void print(Decompiler d, Output output)
        {
            bool leftGroup = LeftGroup();
            bool rightGroup = RightGroup();
            if (leftGroup) output.WriteString("(");
            left.print(d, output);
            if (leftGroup) output.WriteString(")");
            output.WriteString(" ");
            output.WriteString(op);
            output.WriteString(" ");
            if (rightGroup) output.WriteString("(");
            right.print(d, output);
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
