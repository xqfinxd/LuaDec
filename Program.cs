using LuaDec.Assemble;
using LuaDec.Decompile;
using LuaDec.Parser;
using System;
using System.IO;
using static LuaDec.Configuration;

namespace LuaDec
{
    public class Program
    {

        public static string version = "1.2.3.491";

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
                        config.rawString = true;
                    }
                    else if (arg == "--nodebug")
                    {
                        config.variable = Configuration.VariableMode.Nodebug;
                    }
                    else if (arg == "--disassemble")
                    {
                        config.mode = Mode.Disassemble;
                    }
                    else if (arg == "--assemble")
                    {
                        config.mode = Mode.Assemble;
                    }
                    else if (arg == "--output" || arg == "-o")
                    {
                        if (i + 1 < args.Length)
                        {
                            config.output = args[i + 1];
                            i++;
                        }
                        else
                        {
                            error("option \"" + arg + "\" doesn't have an argument", true);
                        }
                    }
                    else if (arg == "--opmap")
                    {
                        if (i + 1 < args.Length)
                        {
                            config.opmap = args[i + 1];
                            i++;
                        }
                        else
                        {
                            error("option \"" + arg + "\" doesn't have an argument", true);
                        }
                    }
                    else
                    {
                        error("unrecognized option: " + arg, true);
                    }
                }
                else if (fn == null)
                {
                    fn = arg;
                }
                else
                {
                    error("too many arguments: " + arg, true);
                }
            }
            if (fn == null)
            {
                error("no input file provided", true);
            }
            else
            {
                switch (config.mode)
                {
                    case Mode.Decompile:
                        {
                            LFunction lmain = null;
                            try
                            {
                                lmain = file_to_function(fn, config);
                            }
                            catch (IOException e)
                            {
                                error(e.Message, false);
                            }
                            Decompiler d = new Decompiler(lmain);
                            Decompiler.State result = d.decompile();
                            d.print(result, config.getOutput());
                            break;
                        }
                    case Mode.Disassemble:
                        {
                            LFunction lmain = null;
                            try
                            {
                                lmain = file_to_function(fn, config);
                            }
                            catch (IOException e)
                            {
                                error(e.Message, false);
                            }
                            Disassembler d = new Disassembler(lmain);
                            d.disassemble(config.getOutput());
                            break;
                        }
                    case Mode.Assemble:
                        {
                            if (config.output == null)
                            {
                                error("assembler mode requires an output file", true);
                            }
                            else
                            {
                                try
                                {
                                    Assembler a = new Assembler(
                                        new StreamReader(File.OpenRead(fn)),
                                      new BinaryWriter(File.OpenWrite(config.output))
                                    );
                                    a.assemble();
                                }
                                catch (IOException e)
                                {
                                    error(e.Message, false);
                                }
                                catch (AssemblerException e)
                                {
                                    error(e.Message, false);
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

        public static void error(string err, bool usage)
        {
            Console.Error.WriteLine("unluac v" + version);
            Console.Error.Write("  error: ");
            Console.Error.WriteLine(err);
            if (usage)
            {
                Console.Error.WriteLine("  usage: java -jar unluac.jar [options] <file>");
            }
            Environment.Exit(1);
        }

        private static LFunction file_to_function(string fn, Configuration config)
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

        private class _OutputProvider1 : OutputProvider
        {
            StreamWriter pout;

            public _OutputProvider1(StreamWriter pout)
            {
                this.pout = pout;
            }
            public void print(string s)
            {
                pout.Write(s);
            }

            public void print(byte b)
            {
                pout.Write(b);
            }

            public void println()
            {
                pout.WriteLine();
            }
        }
        public static void decompile(string input, string output, Configuration config)
        {
            LFunction lmain = file_to_function(input, config);
            Decompiler d = new Decompiler(lmain);
            Decompiler.State result = d.decompile();
            StreamWriter pout = new StreamWriter(output);
            d.print(result, new Output(new _OutputProvider1(pout)));
            pout.Flush();
            pout.Close();
        }

        public static void assemble(string input, string output)
        {
            BinaryWriter outstream = new BinaryWriter(File.OpenWrite(output));
            Assembler a = new Assembler(new StreamReader(File.OpenRead(input)), outstream);
            a.assemble();
            outstream.Flush();
            outstream.Close();
        }

        private class _OutputProvider2 : OutputProvider
        {
            StreamWriter pout;

            public _OutputProvider2(StreamWriter pout)
            {
                this.pout = pout;
            }
            public void print(string s)
            {
                pout.Write(s);
            }

            public void print(byte b)
            {
                pout.Write(b);
            }

            public void println()
            {
                pout.WriteLine();
            }
        }
        public static void disassemble(string input, string output)
        {
            LFunction lmain = file_to_function(input, new Configuration());
            Disassembler d = new Disassembler(lmain);
            StreamWriter pout = new StreamWriter(output);
            d.disassemble(new Output(new _OutputProvider2(pout)));
            pout.Flush();
            pout.Close();
        }

    }

}
