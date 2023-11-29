using System.Collections.Generic;
using System.IO;

namespace Test
{
    public enum TestResult
    {
        Ok, Skipped, Failed
    }

    public class TestReport
    {
        private int failed = 0;
        private List<string> failedTests = new List<string>();
        private int passed = 0;
        private int skipped = 0;
        private List<string> skippedTests = new List<string>();

        public void Report(TextWriter output)
        {
            if (failed == 0 && skipped == 0)
            {
                output.WriteLine("All tests passed!");
            }
            else
            {
                foreach (string failed in failedTests)
                {
                    output.WriteLine("Failed: " + failed);
                }
                foreach (string skipped in skippedTests)
                {
                    output.WriteLine("Skipped: " + skipped);
                }
                output.WriteLine("Failed " + failed + " of " + (failed + passed) + " tests, skipped " + skipped + " tests.");
            }
        }

        public void Result(string test, TestResult result)
        {
            switch (result)
            {
                case TestResult.Ok:
                    passed++;
                    break;

                case TestResult.Failed:
                    failedTests.Add(test);
                    failed++;
                    break;

                case TestResult.Skipped:
                    skippedTests.Add(test);
                    skipped++;
                    break;

                default:
                    throw new System.InvalidOperationException();
            }
        }
    }
}