using LuaDec.Decompile;

namespace LuaDec.Parser
{
    public class LHeader : BObject
    {

        public enum LEndianness
        {
            BIG,
            LITTLE,
        }

        public readonly int format;
        public readonly LEndianness endianness;
        public readonly BIntegerType intT;
        public readonly BIntegerType sizeT;
        public readonly LBooleanType boolT;
        public readonly LNumberType numberT;
        public readonly LNumberType longT;
        public readonly LNumberType doubleT;
        public readonly LStringType stringT;
        public readonly LConstantType constant;
        public readonly LAbsLineInfoType abslineinfo;
        public readonly LLocalType local;
        public readonly LUpvalueType upvalue;
        public readonly LFunctionType function;
        public readonly CodeExtract extractor;

        public LHeader(int format, LEndianness endianness, BIntegerType intT, BIntegerType sizeT, LBooleanType boolT, LNumberType numberT, LNumberType longT, LNumberType doubleT, LStringType stringT, LConstantType constant, LAbsLineInfoType abslineinfo, LLocalType local, LUpvalueType upvalue, LFunctionType function, CodeExtract extractor)
        {
            this.format = format;
            this.endianness = endianness;
            this.intT = intT;
            this.sizeT = sizeT;
            this.boolT = boolT;
            this.numberT = numberT;
            this.longT = longT;
            this.doubleT = doubleT;
            this.stringT = stringT;
            this.constant = constant;
            this.abslineinfo = abslineinfo;
            this.local = local;
            this.upvalue = upvalue;
            this.function = function;
            this.extractor = extractor;
        }

    }

}