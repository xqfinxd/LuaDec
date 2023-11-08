using LuaDec.Assemble;
using LuaDec.Decompile;
using System;
using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public abstract class LHeaderType : BObjectType<LHeader>
    {

        public static readonly LHeaderType TYPE50 = new LHeaderType50();
        public static readonly LHeaderType TYPE51 = new LHeaderType51();
        public static readonly LHeaderType TYPE52 = new LHeaderType52();
        public static readonly LHeaderType TYPE53 = new LHeaderType53();
        public static readonly LHeaderType TYPE54 = new LHeaderType54();

        public static LHeaderType get(Version.HeaderType type)
        {
            switch (type)
            {
                case Version.HeaderType.Lua50: return TYPE50;
                case Version.HeaderType.Lua51: return TYPE51;
                case Version.HeaderType.Lua52: return TYPE52;
                case Version.HeaderType.Lua53: return TYPE53;
                case Version.HeaderType.Lua54: return TYPE54;
                default: throw new System.InvalidOperationException();
            }
        }

        private static readonly byte[] luacTail = {
            0x19, (byte) 0x93, 0x0D, 0x0A, 0x1A, 0x0A,
        };

        protected static readonly int TEST_INT = 0x5678;

        protected static readonly double TEST_FLOAT = 370.5;

        public class LHeaderParseState
        {
            public BIntegerType intT;
            public BIntegerType sizeT;
            public LNumberType numberT;
            public LNumberType longT;
            public LNumberType doubleT;

            public int format;
            public LHeader.LEndianness endianness;

            public int lNumberSize;
            public bool lNumberIntegrality;

            public int lintSize;
            public int lFloatSize;

            public int sizeOp;
            public int sizeA;
            public int sizeB;
            public int sizeC;
        }

        public override LHeader parse(BinaryReader buffer, BHeader header)
        {
            Version version = header.version;
            LHeaderParseState s = new LHeaderParseState();
            parse_main(buffer, header, s);
            LBooleanType boolType = new LBooleanType();
            LStringType stringType = version.LstringType;
            LConstantType constant = version.LConstantType;
            LAbsLineInfoType abslineinfo = new LAbsLineInfoType();
            LLocalType local = new LLocalType();
            LUpvalueType upvalue = version.LUpvalueType;
            LFunctionType function = version.LFunctionType;
            CodeExtract extract = new CodeExtract(header.version, s.sizeOp, s.sizeA, s.sizeB, s.sizeC);
            return new LHeader(s.format, s.endianness, s.intT, s.sizeT, boolType, s.numberT, s.longT, s.doubleT, stringType, constant, abslineinfo, local, upvalue, function, extract);
        }

        abstract public List<Directive> get_directives();

        abstract protected void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s);

        protected void parse_format(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte Lua "format"
            int format = 0xFF & buffer.ReadByte();
            if (format != 0)
            {
                throw new System.InvalidOperationException("The input chunk reports a non-standard lua foreach mat in " + format);
            }
            s.format = format;
            if (header.debug)
            {
                Console.WriteLine("-- foreach mat in " + format);
            }
        }

        protected void write_format(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.format);
        }

        protected void parse_endianness(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte endianness
            int endianness = 0xFF & buffer.ReadByte();
            switch (endianness)
            {
                case 0:
                    s.endianness = LHeader.LEndianness.BIG;
                    //buffer.order(ByteOrder.BIG_ENDIAN);
                    break;
                case 1:
                    s.endianness = LHeader.LEndianness.LITTLE;
                    //buffer.order(ByteOrder.LITTLE_ENDIAN);
                    break;
                default:
                    throw new System.InvalidOperationException("The input chunk reports an invalid endianness: " + endianness);
            }
            if (header.debug)
            {
                Console.WriteLine("-- endianness: " + endianness + (endianness == 0 ? " (big)" : " (little)"));
            }
        }

        protected void write_endianness(BinaryWriter output, BHeader header, LHeader o)
        {
            int value;
            switch (o.endianness)
            {
                case LHeader.LEndianness.BIG:
                    value = 0;
                    break;
                case LHeader.LEndianness.LITTLE:
                    value = 1;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }
            output.Write((byte)value);
        }

        protected void parse_int_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte int size
            int intSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- int size: " + intSize);
            }
            s.intT = new BIntegerType50(intSize);
        }

        protected void write_int_size(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.intT.getSize());
        }

        protected void parse_size_t_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte sizeT size
            int sizeTSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- size_t size: " + sizeTSize);
            }
            s.sizeT = new BIntegerType50(sizeTSize);
        }

        protected void write_size_t_size(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.sizeT.getSize());
        }

        protected void parse_instruction_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte instruction size
            int instructionSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- instruction size: " + instructionSize);
            }
            if (instructionSize != 4)
            {
                throw new System.InvalidOperationException("The input chunk reports an unsupported instruction size: " + instructionSize + " bytes");
            }
        }

        protected void write_instruction_size(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write(4);
        }

        protected void parse_number_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lNumberSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua number size: " + lNumberSize);
            }
            s.lNumberSize = lNumberSize;
        }

        protected void write_number_size(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.numberT.size);
        }

        protected void parse_number_integrality(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lNumberIntegralityCode = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua number integrality code: " + lNumberIntegralityCode);
            }
            if (lNumberIntegralityCode > 1)
            {
                throw new System.InvalidOperationException("The input chunk reports an invalid code foreach  lua number integrality in " + lNumberIntegralityCode);
            }
            s.lNumberIntegrality = (lNumberIntegralityCode == 1);
        }

        protected void write_number_integrality(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)(o.numberT.integral ? 1 : 0));
        }

        protected void parse_integer_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lintSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua int size: " + lintSize);
            }
            if (lintSize < 2)
            {
                throw new System.InvalidOperationException("The input chunk reports an int size that is too small: " + lintSize);
            }
            s.lintSize = lintSize;
        }

        protected void parse_float_size(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lFloatSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua float size: " + lFloatSize);
            }
            s.lFloatSize = lFloatSize;
        }

        protected void parse_number_format_53(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            byte[] endianness = new byte[s.lintSize];
            buffer.Read(endianness, 0, endianness.Length);

            byte test_high = (byte)((TEST_INT >> 8) & 0xFF);
            byte test_low = (byte)(TEST_INT & 0xFF);

            if (endianness[0] == test_low && endianness[1] == test_high)
            {
                s.endianness = LHeader.LEndianness.LITTLE;
                //buffer.order(ByteOrder.LITTLE_ENDIAN);
            }
            else if (endianness[s.lintSize - 1] == test_low && endianness[s.lintSize - 2] == test_high)
            {
                s.endianness = LHeader.LEndianness.BIG;
                //buffer.order(ByteOrder.BIG_ENDIAN);
            }
            else
            {
                throw new System.InvalidOperationException("The input chunk reports an invalid endianness: " + endianness.ToString());
            }
            s.longT = new LNumberType(s.lintSize, true, LNumberType.NumberMode.MODE_int);
            s.doubleT = new LNumberType(s.lFloatSize, false, LNumberType.NumberMode.MODE_FLOAT);
            double floatcheck = s.longT.parse(buffer, header).value();
            if (floatcheck != s.longT.convert(TEST_FLOAT))
            {
                throw new System.InvalidOperationException("The input chunk is using an unrecognized floating point foreach mat in " + floatcheck);
            }
        }

        protected void parse_extractor(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            s.sizeOp = 0xFF & buffer.ReadByte();
            s.sizeA = 0xFF & buffer.ReadByte();
            s.sizeB = 0xFF & buffer.ReadByte();
            s.sizeC = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua opcode extractor sizeOp: " + s.sizeOp + ", sizeA: " + s.sizeA + ", sizeB: " + s.sizeB + ", sizeC: " + s.sizeC);
            }
        }

        protected void write_extractor(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.extractor.op.size);
            output.Write((byte)o.extractor.A.size);
            output.Write((byte)o.extractor.B.size);
            output.Write((byte)o.extractor.C.size);
        }

        protected void parse_tail(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            for (int i = 0; i < luacTail.Length; i++)
            {
                if (buffer.ReadByte() != luacTail[i])
                {
                    throw new System.InvalidOperationException("The input file does not have the header tail of a valid Lua file (it may be corrupted).");
                }
            }
        }

        protected void write_tail(BinaryWriter output, BHeader header, LHeader o)
        {
            for (int i = 0; i < luacTail.Length; i++)
            {
                output.Write(luacTail[i]);
            }
        }

    }


    class LHeaderType50 : LHeaderType
    {

        private static readonly double TEST_NUMBER = 3.14159265358979323846E7;

        protected override void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            s.format = 0;
            parse_endianness(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_size_t_size(buffer, header, s);
            parse_instruction_size(buffer, header, s);
            parse_extractor(buffer, header, s);
            parse_number_size(buffer, header, s);
            LNumberType lfloat = new LNumberType(s.lNumberSize, false, LNumberType.NumberMode.MODE_NUMBER);
            LNumberType lint = new LNumberType(s.lNumberSize, true, LNumberType.NumberMode.MODE_NUMBER);
            var mark = buffer.BaseStream.Position;
            double floatcheck = lfloat.parse(buffer, header).value();
            buffer.BaseStream.Seek(mark - buffer.BaseStream.Position, SeekOrigin.Current);
            double intcheck = lint.parse(buffer, header).value();
            if (floatcheck == lfloat.convert(TEST_NUMBER))
            {
                s.numberT = lfloat;
            }
            else if (intcheck == lint.convert(TEST_NUMBER))
            {
                s.numberT = lint;
            }
            else
            {
                throw new System.InvalidOperationException("The input chunk is using an unrecognized number foreach mat in " + intcheck);
            }
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.ENDIANNESS,
                Directive.INT_SIZE,
                Directive.SIZE_T_SIZE,
                Directive.INSTRUCTION_SIZE,
                Directive.SIZE_OP,
                Directive.SIZE_A,
                Directive.SIZE_B,
                Directive.SIZE_C,
                Directive.NUMBER_FORMAT,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LHeader o)
        {
            write_endianness(output, header, o);
            write_int_size(output, header, o);
            write_size_t_size(output, header, o);
            write_instruction_size(output, header, o);
            write_extractor(output, header, o);
            write_number_size(output, header, o);
            o.numberT.write(output, header, o.numberT.create(TEST_NUMBER));
        }

    }



    class LHeaderType51 : LHeaderType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            parse_format(buffer, header, s);
            parse_endianness(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_size_t_size(buffer, header, s);
            parse_instruction_size(buffer, header, s);
            parse_number_size(buffer, header, s);
            parse_number_integrality(buffer, header, s);
            s.numberT = new LNumberType(s.lNumberSize, s.lNumberIntegrality, LNumberType.NumberMode.MODE_NUMBER);
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.FORMAT,
                Directive.ENDIANNESS,
                Directive.INT_SIZE,
                Directive.SIZE_T_SIZE,
                Directive.INSTRUCTION_SIZE,
                Directive.NUMBER_FORMAT,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LHeader o)
        {
            write_format(output, header, o);
            write_endianness(output, header, o);
            write_int_size(output, header, o);
            write_size_t_size(output, header, o);
            write_instruction_size(output, header, o);
            write_number_size(output, header, o);
            write_number_integrality(output, header, o);
        }

    }

    class LHeaderType52 : LHeaderType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            parse_format(buffer, header, s);
            parse_endianness(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_size_t_size(buffer, header, s);
            parse_instruction_size(buffer, header, s);
            parse_number_size(buffer, header, s);
            parse_number_integrality(buffer, header, s);
            parse_tail(buffer, header, s);
            s.numberT = new LNumberType(s.lNumberSize, s.lNumberIntegrality, LNumberType.NumberMode.MODE_NUMBER);
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.FORMAT,
                Directive.ENDIANNESS,
                Directive.INT_SIZE,
                Directive.SIZE_T_SIZE,
                Directive.INSTRUCTION_SIZE,
                Directive.NUMBER_FORMAT,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LHeader o)
        {
            write_format(output, header, o);
            write_endianness(output, header, o);
            write_int_size(output, header, o);
            write_size_t_size(output, header, o);
            write_instruction_size(output, header, o);
            write_number_size(output, header, o);
            write_number_integrality(output, header, o);
            write_tail(output, header, o);
        }

    }

    class LHeaderType53 : LHeaderType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            parse_format(buffer, header, s);
            parse_tail(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_size_t_size(buffer, header, s);
            parse_instruction_size(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_float_size(buffer, header, s);
            parse_number_format_53(buffer, header, s);
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.FORMAT,
                Directive.INT_SIZE,
                Directive.SIZE_T_SIZE,
                Directive.INSTRUCTION_SIZE,
                Directive.INT_FORMAT,
                Directive.FLOAT_FORMAT,
                Directive.ENDIANNESS,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LHeader o)
        {
            write_format(output, header, o);
            write_tail(output, header, o);
            write_int_size(output, header, o);
            write_size_t_size(output, header, o);
            write_instruction_size(output, header, o);
            output.Write((byte)header.longType.size);
            output.Write((byte)header.doubleType.size);
            header.longType.write(output, header, header.longType.create((double)TEST_INT));
            header.doubleType.write(output, header, header.doubleType.create(TEST_FLOAT));
        }

    }

    class LHeaderType54 : LHeaderType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            parse_format(buffer, header, s);
            parse_tail(buffer, header, s);
            parse_instruction_size(buffer, header, s);
            parse_int_size(buffer, header, s);
            parse_float_size(buffer, header, s);
            parse_number_format_53(buffer, header, s);
            s.intT = new BIntegerType54();
            s.sizeT = new BIntegerType54();
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.FORMAT,
                Directive.INSTRUCTION_SIZE,
                Directive.INT_FORMAT,
                Directive.FLOAT_FORMAT,
                Directive.ENDIANNESS,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LHeader o)
        {
            write_format(output, header, o);
            write_tail(output, header, o);
            write_instruction_size(output, header, o);
            output.Write((byte)header.longType.size);
            output.Write((byte)header.doubleType.size);
            header.longType.write(output, header, header.longType.create((double)TEST_INT));
            header.doubleType.write(output, header, header.doubleType.create(TEST_FLOAT));
        }
    }
}