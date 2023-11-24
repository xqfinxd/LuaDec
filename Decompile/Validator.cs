namespace LuaDec.Decompile
{
    public class Validator
    {
        private Validator()
        {
        }

        public static void Process(Decompiler d)
        {
            Code code = d.code;
            for (int line = 1; line <= code.Length; line++)
            {
                switch (code.GetOp(line).Type)
                {
                    case Op.OpT.EQ:
                    {
                        break;
                    }
                    case Op.OpT.LT:
                    {
                        break;
                    }
                    default:
                        break;
                }
            }
        }
    }
}