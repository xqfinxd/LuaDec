using LuaDec.Assemble;
using LuaDec.Decompile;
using System;
using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    internal class LHeaderType50 : LHeaderType
    {
        private static readonly double TEST_NUMBER = 3.14159265358979323846E7;

        protected override void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            s.format = 0;
            ParseEndianness(buffer, header, s);
            ParseIntSize(buffer, header, s);
            ParseSizeTSize(buffer, header, s);
            ParseInstructionSize(buffer, header, s);
            ParseExtractor(buffer, header, s);
            ParseNumberSize(buffer, header, s);
            LNumberType lfloat = new LNumberType(s.lNumberSize, false, LNumberType.NumberMode.Number);
            LNumberType lint = new LNumberType(s.lNumberSize, true, LNumberType.NumberMode.Number);
            var mark = buffer.BaseStream.Position;
            double floatcheck = lfloat.Parse(buffer, header).GetValue();
            buffer.BaseStream.Position = mark;
            double intcheck = lint.Parse(buffer, header).GetValue();
            if (floatcheck == lfloat.Convert(TEST_NUMBER))
            {
                s.numberT = lfloat;
            }
            else if (intcheck == lint.Convert(TEST_NUMBER))
            {
                s.numberT = lint;
            }
            else
            {
                throw new System.InvalidOperationException("The input chunk is using an unrecognized number foreach mat in " + intcheck);
            }
        }

        public override List<Directive> GetDirectives()
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

        public override void Write(BinaryWriter output, BHeader header, LHeader o)
        {
            WriteEndianness(output, header, o);
            WriteIntSize(output, header, o);
            WriteSizeTSize(output, header, o);
            WriteInstructionSize(output, header, o);
            WriteExtractor(output, header, o);
            WriteNumberSize(output, header, o);
            o.numberT.Write(output, header, o.numberT.Create(TEST_NUMBER));
        }
    }

    internal class LHeaderType51 : LHeaderType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            ParseFormat(buffer, header, s);
            ParseEndianness(buffer, header, s);
            ParseIntSize(buffer, header, s);
            ParseSizeTSize(buffer, header, s);
            ParseInstructionSize(buffer, header, s);
            ParseNumberSize(buffer, header, s);
            ParseNumberIntegrality(buffer, header, s);
            s.numberT = new LNumberType(s.lNumberSize, s.lNumberIntegrality, LNumberType.NumberMode.Number);
        }

        public override List<Directive> GetDirectives()
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

        public override void Write(BinaryWriter output, BHeader header, LHeader o)
        {
            WriteFormat(output, header, o);
            WriteEndianness(output, header, o);
            WriteIntSize(output, header, o);
            WriteSizeTSize(output, header, o);
            WriteInstructionSize(output, header, o);
            WriteNumberSize(output, header, o);
            WriteNumberIntegrality(output, header, o);
        }
    }

    internal class LHeaderType52 : LHeaderType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            ParseFormat(buffer, header, s);
            ParseEndianness(buffer, header, s);
            ParseIntSize(buffer, header, s);
            ParseSizeTSize(buffer, header, s);
            ParseInstructionSize(buffer, header, s);
            ParseNumberSize(buffer, header, s);
            ParseNumberIntegrality(buffer, header, s);
            ParseTail(buffer, header, s);
            s.numberT = new LNumberType(s.lNumberSize, s.lNumberIntegrality, LNumberType.NumberMode.Number);
        }

        public override List<Directive> GetDirectives()
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

        public override void Write(BinaryWriter output, BHeader header, LHeader o)
        {
            WriteFormat(output, header, o);
            WriteEndianness(output, header, o);
            WriteIntSize(output, header, o);
            WriteSizeTSize(output, header, o);
            WriteInstructionSize(output, header, o);
            WriteNumberSize(output, header, o);
            WriteNumberIntegrality(output, header, o);
            WriteTail(output, header, o);
        }
    }

    internal class LHeaderType53 : LHeaderType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            ParseFormat(buffer, header, s);
            ParseTail(buffer, header, s);
            ParseIntSize(buffer, header, s);
            ParseSizeTSize(buffer, header, s);
            ParseInstructionSize(buffer, header, s);
            ParseIntegerSize(buffer, header, s);
            ParseFloatSize(buffer, header, s);
            ParseNumberFormat53(buffer, header, s);
        }

        public override List<Directive> GetDirectives()
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

        public override void Write(BinaryWriter output, BHeader header, LHeader o)
        {
            WriteFormat(output, header, o);
            WriteTail(output, header, o);
            WriteIntSize(output, header, o);
            WriteSizeTSize(output, header, o);
            WriteInstructionSize(output, header, o);
            output.Write((byte)header.longType.size);
            output.Write((byte)header.doubleType.size);
            header.longType.Write(output, header, header.longType.Create((double)TEST_INT));
            header.doubleType.Write(output, header, header.doubleType.Create(TEST_FLOAT));
        }
    }

    internal class LHeaderType54 : LHeaderType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            ParseFormat(buffer, header, s);
            ParseTail(buffer, header, s);
            ParseInstructionSize(buffer, header, s);
            ParseIntegerSize(buffer, header, s);
            ParseFloatSize(buffer, header, s);
            ParseNumberFormat53(buffer, header, s);
            s.intT = new BIntegerType54();
            s.sizeT = new BIntegerType54();
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.FORMAT,
                Directive.INSTRUCTION_SIZE,
                Directive.INT_FORMAT,
                Directive.FLOAT_FORMAT,
                Directive.ENDIANNESS,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LHeader o)
        {
            WriteFormat(output, header, o);
            WriteTail(output, header, o);
            WriteInstructionSize(output, header, o);
            output.Write((byte)header.longType.size);
            output.Write((byte)header.doubleType.size);
            header.longType.Write(output, header, header.longType.Create((double)TEST_INT));
            header.doubleType.Write(output, header, header.doubleType.Create(TEST_FLOAT));
        }
    }

    public abstract class LHeaderType : BObjectType<LHeader>
    {
        public class LHeaderParseState
        {
            public LNumberType doubleT;
            public LHeader.LEndianness endianness;
            public int format;
            public BIntegerType intT;
            public int lfloatSize;
            public int lintSize;
            public bool lNumberIntegrality;
            public int lNumberSize;
            public LNumberType longT;
            public LNumberType numberT;
            public int sizeA;
            public int sizeB;
            public int sizeC;
            public int sizeOp;
            public BIntegerType sizeT;
        }

        private static readonly byte[] luacTail = {
            0x19, 0x93, 0x0D, 0x0A, 0x1A, 0x0A,
        };

        protected static readonly double TEST_FLOAT = 370.5;
        protected static readonly int TEST_INT = 0x5678;
        public static readonly LHeaderType TYPE50 = new LHeaderType50();
        public static readonly LHeaderType TYPE51 = new LHeaderType51();
        public static readonly LHeaderType TYPE52 = new LHeaderType52();
        public static readonly LHeaderType TYPE53 = new LHeaderType53();
        public static readonly LHeaderType TYPE54 = new LHeaderType54();

        protected void ParseEndianness(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte endianness
            int endianness = 0xFF & buffer.ReadByte();
            switch (endianness)
            {
                case 0:
                    s.endianness = LHeader.LEndianness.Big;
                    //buffer.order(ByteOrder.BIG_ENDIAN);
                    break;

                case 1:
                    s.endianness = LHeader.LEndianness.Little;
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

        protected void ParseExtractor(BinaryReader buffer, BHeader header, LHeaderParseState s)
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

        protected void ParseFloatSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lFloatSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua float size: " + lFloatSize);
            }
            s.lfloatSize = lFloatSize;
        }

        protected void ParseFormat(BinaryReader buffer, BHeader header, LHeaderParseState s)
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
                Console.WriteLine("-- format: " + format);
            }
        }

        protected void ParseInstructionSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
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

        protected void ParseIntegerSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
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

        protected void ParseIntSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte int size
            int intSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- int size: " + intSize);
            }
            s.intT = new BIntegerType50(true, intSize, header.version.allowNegativeInt.Value);
        }

        protected abstract void ParseMain(BinaryReader buffer, BHeader header, LHeaderParseState s);

        protected void ParseNumberFormat53(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            byte[] endianness = new byte[s.lintSize];
            buffer.Read(endianness, 0, endianness.Length);

            byte test_high = (byte)((TEST_INT >> 8) & 0xFF);
            byte test_low = (byte)(TEST_INT & 0xFF);

            if (endianness[0] == test_low && endianness[1] == test_high)
            {
                s.endianness = LHeader.LEndianness.Little;
            }
            else if (endianness[s.lintSize - 1] == test_low && endianness[s.lintSize - 2] == test_high)
            {
                s.endianness = LHeader.LEndianness.Big;
            }
            else
            {
                throw new System.InvalidOperationException("The input chunk reports an invalid endianness: " + endianness.ToString());
            }
            s.longT = new LNumberType(s.lintSize, true, LNumberType.NumberMode.Integer);
            s.doubleT = new LNumberType(s.lfloatSize, false, LNumberType.NumberMode.Float);
            double floatcheck = s.doubleT.Parse(buffer, header).GetValue();
            if (floatcheck != s.doubleT.Convert(TEST_FLOAT))
            {
                throw new System.InvalidOperationException("The input chunk is using an unrecognized floating point format: " + floatcheck);
            }
        }

        protected void ParseNumberIntegrality(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lNumberIntegralityCode = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua number integrality code: " + lNumberIntegralityCode);
            }
            if (lNumberIntegralityCode > 1)
            {
                throw new System.InvalidOperationException("The input chunk reports an invalid code for lua number integrality: " + lNumberIntegralityCode);
            }
            s.lNumberIntegrality = (lNumberIntegralityCode == 1);
        }

        protected void ParseNumberSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            int lNumberSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- Lua number size: " + lNumberSize);
            }
            s.lNumberSize = lNumberSize;
        }

        protected void ParseSizeTSize(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            // 1 byte sizeT size
            int sizeTSize = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- size_t size: " + sizeTSize);
            }
            s.sizeT = new BIntegerType50(false, sizeTSize, false);
        }

        protected void ParseTail(BinaryReader buffer, BHeader header, LHeaderParseState s)
        {
            for (int i = 0; i < luacTail.Length; i++)
            {
                if (buffer.ReadByte() != luacTail[i])
                {
                    throw new System.InvalidOperationException("The input file does not have the header tail of a valid Lua file (it may be corrupted).");
                }
            }
        }

        protected void WriteEndianness(BinaryWriter output, BHeader header, LHeader o)
        {
            int value;
            switch (o.endianness)
            {
                case LHeader.LEndianness.Big:
                    value = 0;
                    break;

                case LHeader.LEndianness.Little:
                    value = 1;
                    break;

                default:
                    throw new System.InvalidOperationException();
            }
            output.Write((byte)value);
        }

        protected void WriteExtractor(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.extractor.op.Size);
            output.Write((byte)o.extractor.A.Size);
            output.Write((byte)o.extractor.B.Size);
            output.Write((byte)o.extractor.C.Size);
        }

        protected void WriteFormat(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.format);
        }

        protected void WriteInstructionSize(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write(4);
        }

        protected void WriteIntSize(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.intT.GetSize());
        }

        protected void WriteNumberIntegrality(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)(o.numberT.integral ? 1 : 0));
        }

        protected void WriteNumberSize(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.numberT.size);
        }

        protected void WriteSizeTSize(BinaryWriter output, BHeader header, LHeader o)
        {
            output.Write((byte)o.sizeT.GetSize());
        }

        protected void WriteTail(BinaryWriter output, BHeader header, LHeader o)
        {
            for (int i = 0; i < luacTail.Length; i++)
            {
                output.Write(luacTail[i]);
            }
        }

        public static LHeaderType Get(Version.HeaderType type)
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

        public abstract List<Directive> GetDirectives();

        public override LHeader Parse(BinaryReader buffer, BHeader header)
        {
            Version version = header.version;
            LHeaderParseState s = new LHeaderParseState();
            ParseMain(buffer, header, s);
            LBooleanType boolType = new LBooleanType();
            LStringType stringType = version.LStringType;
            LConstantType constant = version.LConstantType;
            LAbsLineInfoType abslineinfo = new LAbsLineInfoType();
            LLocalType local = new LLocalType();
            LUpvalueType upvalue = version.LUpvalueType;
            LFunctionType function = version.LFunctionType;
            CodeExtract extract = new CodeExtract(header.version, s.sizeOp, s.sizeA, s.sizeB, s.sizeC);
            return new LHeader(s.format, s.endianness, s.intT, s.sizeT, boolType, s.numberT, s.longT, s.doubleT, stringType, constant, abslineinfo, local, upvalue, function, extract);
        }
    }
}