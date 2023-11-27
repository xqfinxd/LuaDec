using System.Collections.Generic;

namespace LuaDec.Parser
{
    public class BList<T> : BObject where T : BObject
    {
        private readonly List<T> values;
        public readonly BInteger blength;

        public BList(BInteger length, List<T> values)
        {
            this.blength = length;
            this.values = values;
        }

        public T[] AsArray(T[] array)
        {
            int i = 0;
            blength.Iterate(() =>
            {
                array[i] = values[i];
                i++;
            });
            return array;
        }

        public T Get(int index)
        {
            return values[index];
        }

        public IEnumerator<T> Iterator()
        {
            return values.GetEnumerator();
        }
    }
}