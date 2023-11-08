using System.IO;

namespace LuaDec.Parser
{
    public abstract class LUpvalueType : BObjectType<LUpvalue>
    {

        public static LUpvalueType get(Version.UpvalueType type)
        {
            switch (type)
            {
                case Version.UpvalueType.Lua50: return new LUpvalueType50();
                case Version.UpvalueType.Lua54: return new LUpvalueType54();
                default: throw new System.InvalidOperationException();
            }
        }

    }

    class LUpvalueType50 : LUpvalueType
    {

        public override LUpvalue parse(BinaryReader buffer, BHeader header)
        {
            LUpvalue upvalue = new LUpvalue();
            upvalue.instack = buffer.ReadByte() != 0;
            upvalue.idx = 0xFF & buffer.ReadByte();
            upvalue.kind = -1;
            return upvalue;
        }

        public override void write(BinaryWriter output, BHeader header, LUpvalue o)
        {
            output.Write((byte)(o.instack ? 1 : 0));
            output.Write((byte)o.idx);
        }
    }

    class LUpvalueType54 : LUpvalueType
    {

        public override LUpvalue parse(BinaryReader buffer, BHeader header)
        {
            LUpvalue upvalue = new LUpvalue();
            upvalue.instack = buffer.ReadByte() != 0;
            upvalue.idx = 0xFF & buffer.ReadByte();
            upvalue.kind = 0xFF & buffer.ReadByte();
            return upvalue;
        }

        public override void write(BinaryWriter output, BHeader header, LUpvalue o)
        {
            output.Write((byte)(o.instack ? 1 : 0));
            output.Write((byte)o.idx);
            output.Write((byte)o.kind);
        }
    }

}