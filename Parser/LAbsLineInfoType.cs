using System.IO;

namespace LuaDec.Parser
{
    public class LAbsLineInfoType : BObjectType<LAbsLineInfo>
    {

        public override LAbsLineInfo parse(BinaryReader buffer, BHeader header)
        {
            int pc = header.integerType.parse(buffer, header).asInt();
            int line = header.integerType.parse(buffer, header).asInt();
            return new LAbsLineInfo(pc, line);
        }

        public override void write(BinaryWriter output, BHeader header, LAbsLineInfo o)
        {
            header.integerType.write(output, header, new BInteger(o.pc));
            header.integerType.write(output, header, new BInteger(o.line));
        }

    }

}