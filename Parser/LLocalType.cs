using System;
using System.IO;

namespace LuaDec.Parser
{
    public class LLocalType : BObjectType<LLocal>
    {

        public override LLocal parse(BinaryReader buffer, BHeader header)
        {
            LString name = header.stringType.parse(buffer, header);
            BInteger start = header.integerType.parse(buffer, header);
            BInteger end = header.integerType.parse(buffer, header);
            if (header.debug)
            {
                Console.Write("-- parsing local, name: ");
                Console.Write(name);
                Console.Write(" from " + start.asInt() + " to " + end.asInt());
                Console.WriteLine();
            }
            return new LLocal(name, start, end);
        }

        public override void write(BinaryWriter output, BHeader header, LLocal o)
        {
            header.stringType.write(output, header, o.name);
            header.integerType.write(output, header, new BInteger(o.start));
            header.integerType.write(output, header, new BInteger(o.end));
        }

    }

}