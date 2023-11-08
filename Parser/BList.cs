using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Parser
{
    public class BList<T> : BObject where T : BObject
    {

        public readonly BInteger blength;
        private readonly List<T> values;

        public BList(BInteger length, List<T> values)
        {
            this.blength = length;
            this.values = values;
        }

        public T get(int index)
        {
            return values[index];
        }

        public IEnumerator<T> iterator()
        {
            return values.GetEnumerator();
        }

        public T[] asArray(T[] array)
        {
            int i = 0;
            blength.iterate(() =>
            {

                array[i] = values[i];
                i++;
            });
            return array;
        }

    }

}
