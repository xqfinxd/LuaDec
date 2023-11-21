using LuaDec.Assemble;
using LuaDec.Decompile;
using LuaDec.Parser;
using System;
using System.IO;

namespace LuaDec
{
    public class Program
    {
        private class FileOutputProvider : IOutputProvider
        {
            private StreamWriter writer;

            public FileOutputProvider(StreamWriter writer)
            {
                this.writer = writer;
            }

            public void WriteByte(byte b)
            {
                writer.Write(b);
            }

            public void WriteLine()
            {
                writer.WriteLine();
            }

            public void WriteString(string s)
            {
                writer.Write(s);
            }
        }

        public static string Version = "0.0.0.1";

        private static LFunction File2Function(string fn, Configuration config)
        {
            FileStream file = null;
            try
            {
                file = File.OpenRead(fn);
                byte[] buffer = new byte[file.Length];
                file.Read(buffer, 0, buffer.Length);
                BHeader header = new BHeader(new BinaryReader(file), config);
                return header.main;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }

        public static void Assemble(string input, string output)
        {
            BinaryWriter outstream = new BinaryWriter(File.OpenWrite(output));
            Assembler a = new Assembler(new StreamReader(File.OpenRead(input)), outstream);
            a.assemble();
            outstream.Flush();
            outstream.Close();
        }

        public static void Decompile(string input, string output, Configuration config)
        {
            LFunction lmain = File2Function(input, config);
            Decompiler d = new Decompiler(lmain);
            Decompiler.State result = d.decompile();
            StreamWriter pout = new StreamWriter(output);
            d.print(result, new Output(new FileOutputProvider(pout)));
            pout.Flush();
            pout.Close();
        }

        public static void Disassemble(string input, string output)
        {
            LFunction lmain = File2Function(input, new Configuration());
            Disassembler d = new Disassembler(lmain);
            StreamWriter pout = new StreamWriter(output);
            d.disassemble(new Output(new FileOutputProvider(pout)));
            pout.Flush();
            pout.Close();
        }

        public static void Error(string err, bool usage)
        {
            Console.Error.WriteLine("unluac v" + Version);
            Console.Error.Write("  error: ");
            Console.Error.WriteLine(err);
            if (usage)
            {
                Console.Error.WriteLine("  usage: java -jar unluac.jar [options] <file>");
            }
            Environment.Exit(1);
        }

        public static void Main(string[] args)
        {
            string fn = null;
            Configuration config = new Configuration();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    // option
                    if (arg == "--rawstring")
                    {
                        config.RawString = true;
                    }
                    else if (arg == "--nodebug")
                    {
                        config.Variable = Configuration.VariableMode.Nodebug;
                    }
                    else if (arg == "--disassemble")
                    {
                        config.Mode = Configuration.OpMode.Disassemble;
                    }
                    else if (arg == "--assemble")
                    {
                        config.Mode = Configuration.OpMode.Assemble;
                    }
                    else if (arg == "--output" || arg == "-o")
                    {
                        if (i + 1 < args.Length)
                        {
                            config.Output = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Error("option \"" + arg + "\" doesn't have an argument", true);
                        }
                    }
                    else if (arg == "--opmap")
                    {
                        if (i + 1 < args.Length)
                        {
                            config.OpMapFile = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Error("option \"" + arg + "\" doesn't have an argument", true);
                        }
                    }
                    else
                    {
                        Error("unrecognized option: " + arg, true);
                    }
                }
                else if (fn == null)
                {
                    fn = arg;
                }
                else
                {
                    Error("too many arguments: " + arg, true);
                }
            }
            if (fn == null)
            {
                Error("no input file provided", true);
            }
            else
            {
                switch (config.Mode)
                {
                    case Configuration.OpMode.Decompile:
                    {
                        LFunction lmain = null;
                        try
                        {
                            lmain = File2Function(fn, config);
                        }
                        catch (IOException e)
                        {
                            Error(e.Message, false);
                        }
                        Decompiler d = new Decompiler(lmain);
                        Decompiler.State result = d.decompile();
                        d.print(result, config.GetOutput());
                        break;
                    }
                    case Configuration.OpMode.Disassemble:
                    {
                        LFunction lmain = null;
                        try
                        {
                            lmain = File2Function(fn, config);
                        }
                        catch (IOException e)
                        {
                            Error(e.Message, false);
                        }
                        Disassembler d = new Disassembler(lmain);
                        d.disassemble(config.GetOutput());
                        break;
                    }
                    case Configuration.OpMode.Assemble:
                    {
                        if (config.Output == null)
                        {
                            Error("assembler mode requires an output file", true);
                        }
                        else
                        {
                            try
                            {
                                Assembler a = new Assembler(
                                    new StreamReader(File.OpenRead(fn)),
                                  new BinaryWriter(File.OpenWrite(config.Output))
                                );
                                a.assemble();
                            }
                            catch (IOException e)
                            {
                                Error(e.Message, false);
                            }
                            catch (AssemblerException e)
                            {
                                Error(e.Message, false);
                            }
                        }
                        break;
                    }
                    default:
                        throw new System.InvalidOperationException();
                }

                Environment.Exit(0);
            }
        }
    }
}