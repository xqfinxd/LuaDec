using LuaDec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class RunTests
    {
        [TestMethod]
        public void Test()
        {
            bool result = true;
            TestReport report = new TestReport();
            Configuration config = new Configuration();
            config.StrictScope = true;
            var verSpecs = new LuaSpec[] {
                new LuaSpec(0x50, 3),
                new LuaSpec(0x51, 5),
                new LuaSpec(0x52, 4),
                new LuaSpec(0x53, 6),
                new LuaSpec(0x54, 6),
            };
            foreach (LuaSpec spec in verSpecs)
            {
                LuaDecSpec uspec = new LuaDecSpec();
                Console.Write(spec.ID);
                result = result && TestFiles.Suite.Run(spec, uspec, report, config);
                Console.WriteLine();
            }
            report.Report(Console.Out);
            Assert.IsTrue(result);
        }
    }
}