using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public abstract class BObjectType<T> where T : BObject
    {

        public abstract T parse(BinaryReader buffer, BHeader header);

        public abstract void write(BinaryWriter output, BHeader header, T o);

        public BList<T> parseList(BinaryReader buffer, BHeader header)
        {
            BInteger length = header.integerType.parse(buffer, header);
            List<T> values = new List<T>();
            length.iterate(() =>
            {
                values.Add(parse(buffer, header));
            });
            return new BList<T>(length, values);
        }

        public void writeList(BinaryWriter output, BHeader header, T[] array)
        {
            header.integerType.write(output, header, new BInteger(array.Length));
            foreach (T o in array)
            {
                write(output, header, o);
            }
        }

        public void writeList(BinaryWriter output, BHeader header, BList<T> blist)
        {
            header.integerType.write(output, header, blist.blength);
            IEnumerator<T> it = blist.iterator();
            while (it.MoveNext())
            {
                write(output, header, it.Current);
            }
        }

    }

}
