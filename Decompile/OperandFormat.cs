namespace LuaDec.Decompile
{
    public class OperandFormat
    {
        public enum Field
        {
            A,
            B,
            C,
            k,
            Ax,
            sJ,
            Bx,
            sBx,
            x,
        }

        public enum Format
        {
            Raw,
            Register,
            Upvalue,
            RegisterK,
            RegisterK54,
            Constant,
            ConstantNumber,
            ConstantString,
            Function,
            ImmediateUInt,
            ImmediateSInt,
            ImmediateFloat,
            Jump,
            JumpNegative,
        }
        
        #region Enum
        public static readonly OperandFormat A = new OperandFormat(Field.A, Format.Raw);
        public static readonly OperandFormat AR = new OperandFormat(Field.A, Format.Register);
        public static readonly OperandFormat AU = new OperandFormat(Field.A, Format.Upvalue);
        public static readonly OperandFormat Ax = new OperandFormat(Field.Ax, Format.Raw);
        public static readonly OperandFormat B = new OperandFormat(Field.B, Format.Raw);
        public static readonly OperandFormat BI = new OperandFormat(Field.B, Format.ImmediateUInt);
        public static readonly OperandFormat BK = new OperandFormat(Field.B, Format.Constant);
        public static readonly OperandFormat BKS = new OperandFormat(Field.B, Format.ConstantString);
        public static readonly OperandFormat BR = new OperandFormat(Field.B, Format.Register);
        public static readonly OperandFormat BRK = new OperandFormat(Field.B, Format.RegisterK);
        public static readonly OperandFormat BsI = new OperandFormat(Field.B, Format.ImmediateSInt);
        public static readonly OperandFormat BU = new OperandFormat(Field.B, Format.Upvalue);
        public static readonly OperandFormat Bx = new OperandFormat(Field.Bx, Format.Raw);
        public static readonly OperandFormat BxF = new OperandFormat(Field.Bx, Format.Function);
        public static readonly OperandFormat BxJ = new OperandFormat(Field.Bx, Format.Jump);
        public static readonly OperandFormat BxJ1 = new OperandFormat(Field.Bx, Format.Jump, 1);
        public static readonly OperandFormat BxJn = new OperandFormat(Field.Bx, Format.JumpNegative);
        public static readonly OperandFormat BxK = new OperandFormat(Field.Bx, Format.Constant);
        public static readonly OperandFormat C = new OperandFormat(Field.C, Format.Raw);
        public static readonly OperandFormat CI = new OperandFormat(Field.C, Format.ImmediateUInt);
        public static readonly OperandFormat CK = new OperandFormat(Field.C, Format.Constant);
        public static readonly OperandFormat CKI = new OperandFormat(Field.C, Format.ConstantNumber);
        public static readonly OperandFormat CKS = new OperandFormat(Field.C, Format.ConstantString);
        public static readonly OperandFormat CR = new OperandFormat(Field.C, Format.Register);
        public static readonly OperandFormat CRK = new OperandFormat(Field.C, Format.RegisterK);
        public static readonly OperandFormat CRK54 = new OperandFormat(Field.C, Format.RegisterK54);
        public static readonly OperandFormat CsI = new OperandFormat(Field.C, Format.ImmediateSInt);
        public static readonly OperandFormat k = new OperandFormat(Field.k, Format.Raw);
        public static readonly OperandFormat sBxF = new OperandFormat(Field.sBx, Format.ImmediateFloat);
        public static readonly OperandFormat sBxI = new OperandFormat(Field.sBx, Format.ImmediateUInt);
        public static readonly OperandFormat sBxJ = new OperandFormat(Field.sBx, Format.Jump);
        public static readonly OperandFormat sJ = new OperandFormat(Field.sJ, Format.Jump);
        public static readonly OperandFormat x = new OperandFormat(Field.x, Format.Raw);
        #endregion

        public readonly Field field;
        public readonly Format format;
        public readonly int offset;

        private OperandFormat(Field field, Format format)
            : this(field, format, 0)
        {
        }

        private OperandFormat(Field field, Format format, int offset)
        {
            this.field = field;
            this.format = format;
            this.offset = offset;
        }
    }
}