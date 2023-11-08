using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RAW,
            REGISTER,
            UPVALUE,
            REGISTER_K,
            REGISTER_K54,
            CONSTANT,
            CONSTANT_int,
            CONSTANT_STRING,
            FUNCTION,
            IMMEDIATE_int,
            IMMEDIATE_SIGNED_int,
            IMMEDIATE_FLOAT,
            JUMP,
            JUMP_NEGATIVE,
        }

        static readonly public OperandFormat A = new OperandFormat(Field.A, Format.RAW);
        static readonly public OperandFormat AR = new OperandFormat(Field.A, Format.REGISTER);
        static readonly public OperandFormat AU = new OperandFormat(Field.A, Format.UPVALUE);
        static readonly public OperandFormat B = new OperandFormat(Field.B, Format.RAW);
        static readonly public OperandFormat BR = new OperandFormat(Field.B, Format.REGISTER);
        static readonly public OperandFormat BRK = new OperandFormat(Field.B, Format.REGISTER_K);
        static readonly public OperandFormat BK = new OperandFormat(Field.B, Format.CONSTANT);
        static readonly public OperandFormat BKS = new OperandFormat(Field.B, Format.CONSTANT_STRING);
        static readonly public OperandFormat BI = new OperandFormat(Field.B, Format.IMMEDIATE_int);
        static readonly public OperandFormat BsI = new OperandFormat(Field.B, Format.IMMEDIATE_SIGNED_int);
        static readonly public OperandFormat BU = new OperandFormat(Field.B, Format.UPVALUE);
        static readonly public OperandFormat C = new OperandFormat(Field.C, Format.RAW);
        static readonly public OperandFormat CR = new OperandFormat(Field.C, Format.REGISTER);
        static readonly public OperandFormat CRK = new OperandFormat(Field.C, Format.REGISTER_K);
        static readonly public OperandFormat CRK54 = new OperandFormat(Field.C, Format.REGISTER_K54);
        static readonly public OperandFormat CK = new OperandFormat(Field.C, Format.CONSTANT);
        static readonly public OperandFormat CKI = new OperandFormat(Field.C, Format.CONSTANT_int);
        static readonly public OperandFormat CKS = new OperandFormat(Field.C, Format.CONSTANT_STRING);
        static readonly public OperandFormat CI = new OperandFormat(Field.C, Format.IMMEDIATE_int);
        static readonly public OperandFormat CsI = new OperandFormat(Field.C, Format.IMMEDIATE_SIGNED_int);
        static readonly public OperandFormat k = new OperandFormat(Field.k, Format.RAW);
        static readonly public OperandFormat Ax = new OperandFormat(Field.Ax, Format.RAW);
        static readonly public OperandFormat sJ = new OperandFormat(Field.sJ, Format.JUMP);
        static readonly public OperandFormat Bx = new OperandFormat(Field.Bx, Format.RAW);
        static readonly public OperandFormat BxK = new OperandFormat(Field.Bx, Format.CONSTANT);
        static readonly public OperandFormat BxJ = new OperandFormat(Field.Bx, Format.JUMP);
        static readonly public OperandFormat BxJ1 = new OperandFormat(Field.Bx, Format.JUMP, 1);
        static readonly public OperandFormat BxJn = new OperandFormat(Field.Bx, Format.JUMP_NEGATIVE);
        static readonly public OperandFormat BxF = new OperandFormat(Field.Bx, Format.FUNCTION);
        static readonly public OperandFormat sBxJ = new OperandFormat(Field.sBx, Format.JUMP);
        static readonly public OperandFormat sBxI = new OperandFormat(Field.sBx, Format.IMMEDIATE_int);
        static readonly public OperandFormat sBxF = new OperandFormat(Field.sBx, Format.IMMEDIATE_FLOAT);
        static readonly public OperandFormat x = new OperandFormat(Field.x, Format.RAW);

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
