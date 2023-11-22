namespace LuaDec.Decompile
{
    public class AssertionManager
    {
        private AssertionManager()
        { }

        public static bool AssertCritical(bool condition, string message)
        {
            if (condition)
            {
            }
            else
            {
                Critical(message);
            }
            return condition;
        }

        public static void Critical(string message)
        {
            throw new System.InvalidOperationException(message);
        }
    }
}