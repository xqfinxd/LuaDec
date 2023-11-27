using System.IO;

namespace LuaDec.Parser
{
    public class LAbsLineInfoType : BObjectType<LAbsLineInfo>
    {
        public override LAbsLineInfo Parse(BinaryReader buffer, BHeader header)
        {
            int pc = header.integerType.Parse(buffer, header).AsInt();
            int line = header.integerType.Parse(buffer, header).AsInt();
            return new LAbsLineInfo(pc, line);
        }

        public override void Write(BinaryWriter output, BHeader header, LAbsLineInfo o)
        {
            header.integerType.Write(output, header, new BInteger(o.pc));
            header.integerType.Write(output, header, new BInteger(o.line));
        }
    }
}