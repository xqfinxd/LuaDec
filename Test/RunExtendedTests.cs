using LuaDec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Test
{
    [TestClass]
    public class RunExtendedTests
    {
        private static void GatherTests(string workDir, string folder, List<TestFile> files)
        {
            var luaFiles = Directory.GetFiles(folder, "*.lua", SearchOption.AllDirectories);
            Uri workDirUri = new Uri(workDir);
            foreach (string file in luaFiles)
            {
                Uri uri = new Uri(file);

                string relative = workDirUri.MakeRelativeUri(uri).ToString();
                files.Add(new TestFile(relative.Substring(0, relative.Length - 4)));
            }
        }

        [TestMethod]
        public static void Test(string[] args)
        {
            var luaTestFiles = Directory.GetDirectories(args[0]);
            TestReport report = new TestReport();
            Configuration config = new Configuration();
            for (int version = 0x50; version <= 0x54; version++)
            {
                LuaSpec spec = new LuaSpec(version);
                LuaDecSpec uspec = new LuaDecSpec();
                Console.WriteLine(spec.ID);
                foreach (var subfolder in luaTestFiles)
                {
                    string filename = Path.GetFileName(subfolder);
                    if (spec.Compatible(filename))
                    {
                        List<TestFile> files = new List<TestFile>();
                        GatherTests(subfolder, subfolder, files);
                        TestSuite suite = new TestSuite(filename, subfolder + Path.DirectorySeparatorChar, files.ToArray());
                        Console.Write("\t" + filename);
                        suite.Run(spec, uspec, report, config);
                        Console.WriteLine();
                    }
                }
            }
            report.Report(Console.Out);
        }
    }
}