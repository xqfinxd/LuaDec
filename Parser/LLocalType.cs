using System;
using System.IO;

namespace LuaDec.Parser
{
    public class LLocalType : BObjectType<LLocal>
    {
        public override LLocal Parse(BinaryReader buffer, BHeader header)
        {
            LString name = header.stringType.Parse(buffer, header);
            BInteger start = header.integerType.Parse(buffer, header);
            BInteger end = header.integerType.Parse(buffer, header);
            if (header.debug)
            {
                Console.Write("-- parsing local, name: ");
                Console.Write(name);
                Console.Write(" from " + start.AsInt() + " to " + end.AsInt());
                Console.WriteLine();
            }
            return new LLocal(name, start, end);
        }

        public override void Write(BinaryWriter output, BHeader header, LLocal o)
        {
            header.stringType.Write(output, header, o.name);
            header.integerType.Write(output, header, new BInteger(o.start));
            header.integerType.Write(output, header, new BInteger(o.end));
        }
    }
}