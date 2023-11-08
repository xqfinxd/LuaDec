using System;
using System.IO;

namespace LuaDec.Parser
{
    public class LBooleanType : BObjectType<LBoolean>
    {

        public override LBoolean parse(BinaryReader buffer, BHeader header)
        {
            int value = buffer.ReadByte();
            if ((value & 0xFFFFFFFE) != 0)
            {
                throw new System.InvalidOperationException();
            }
            else
            {
                LBoolean boolValue = value == 0 ? LBoolean.LFALSE : LBoolean.LTRUE;
                if (header.debug)
                {
                    Console.WriteLine("-- parsed <bool> " + boolValue);
                }
                return boolValue;
            }
        }

        public override void write(BinaryWriter output, BHeader header, LBoolean o)
        {
            int value = o.Value ? 1 : 0;
            output.Write((byte)value);
        }

    }


}