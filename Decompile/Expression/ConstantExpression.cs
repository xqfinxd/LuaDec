using LuaDec.Parser;

namespace LuaDec.Decompile.Expression
{
    public class ConstantExpression : IExpression
    {
        private readonly Constant constant;
        private readonly bool identifier;
        private readonly int index;
        private readonly int line;

        private ConstantExpression(Constant constant, bool identifier, int index, int line)
            : base(GetPrecedence(constant))
        {
            this.constant = constant;
            this.identifier = identifier;
            this.index = index;
            this.line = line;
        }

        public ConstantExpression(Constant constant, bool identifier, int index)
            : this(constant, identifier, index, -1)
        {
        }

        private static int GetPrecedence(Constant constant)
        {
            if (constant.IsNumber() && constant.IsNegative())
            {
                return PRECEDENCE_UNARY;
            }
            else
            {
                return PRECEDENCE_ATOMIC;
            }
        }

        public static ConstantExpression CreateBool(bool v)
        {
            return new ConstantExpression(new Constant(v ? LBoolean.LTRUE : LBoolean.LFALSE), false, -1);
        }

        public static ConstantExpression CreateDouble(double x)
        {
            return new ConstantExpression(new Constant(x), false, -1);
        }

        public static ConstantExpression CreateInt(int i)
        {
            return new ConstantExpression(new Constant(i), false, -1);
        }

        public static ConstantExpression CreateNil(int line)
        {
            return new ConstantExpression(new Constant(LNil.NIL), false, -1, line);
        }

        public override int AsInteger()
        {
            return constant.AsInt();
        }

        public override string AsName()
        {
            return constant.AsName();
        }

        public override int GetConstantIndex()
        {
            return index;
        }

        public override int GetConstantLine()
        {
            return line;
        }

        public override bool IsBool()
        {
            return constant.IsBool();
        }

        public override bool IsBrief()
        {
            return !constant.IsString() || constant.AsName().Length <= 10;
        }

        public override bool IsConstant()
        {
            return true;
        }

        public override bool IsIdentifier()
        {
            return identifier;
        }

        public override bool IsInteger()
        {
            return constant.IsInt();
        }

        public override bool IsNil()
        {
            return constant.IsNil();
        }

        public override bool IsString()
        {
            return constant.IsString();
        }

        public override bool IsUngrouped()
        {
            return true;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            constant.Print(d, output, false);
        }

        public override void WriteBraced(Decompiler d, Output output)
        {
            constant.Print(d, output, true);
        }
    }
}