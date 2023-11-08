using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Util
{
    public class FileUtils
    {
        public static StreamReader createSmartTextFileReader(FileStream file)
        {

            return new StreamReader(file);
            /*
            byte[] header = new byte[2];
            int header_length = 0;
            var mark = file.Position;
            do
            {
                int n = file.Read(header, header_length, header.Length - header_length);
                if (n == -1) break;
                header_length += n;
            } while (header_length < header.Length);
            if (header.Length >= 2 && header[0] == 0xff && header[1] == 0xfe)
            {
                return new StreamReader(file, Encoding.GetEncoding("UTF-16LE"));
            }
            else if (header.Length >= 2 && header[0] == 0xfe && header[1] == 0xff)
            {
                return new StreamReader(file, Encoding.GetEncoding("UTF-16BE"));
            }
            else
            {
                file.Position = mark;
                return new StreamReader(file);
            }
            */
        }
    }

}
