using System.IO;

namespace LuaDec.Decompile
{
    public class FileOutputProvider : OutputProvider
    {

        private readonly FileStream output;
        private readonly string eol;

        public FileOutputProvider(FileStream output)
        {
            this.output = output;
            eol = System.Environment.NewLine;
        }

        public void print(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                int c = s[i];
                if (c < 0 || c > 255) throw new System.InvalidOperationException();
                print((byte)c);
            }
        }

        public void print(byte b)
        {
            try
            {
                output.WriteByte(b);
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        public void println()
        {
            print(eol);
        }

    }

}
