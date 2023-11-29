using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Test
{
    public class LuaC
    {
        public static void Compile(LuaSpec spec, string input, string output)
        {
            string luac = spec.GetLuaCName();
            // multi-platform
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                luac = luac + ".exe";
            }
            StringBuilder args = new StringBuilder();
            foreach (string arg in spec.GetArgs())
            {
                args.Append(" ");
                args.Append(arg);
            }
            args.Append(" ");
            args.Append("-o");
            args.Append(" ");
            args.Append(output);
            args.Append(" ");
            args.Append(input);

            var curDir = System.IO.Directory.GetCurrentDirectory();
            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(curDir, "luac", luac);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = args.ToString();
            process.StartInfo.WorkingDirectory = curDir;
            process.Start();
            try
            {
                process.WaitForExit();
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}