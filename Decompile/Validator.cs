namespace LuaDec.Decompile
{
    public class Validator
    {
        //static only
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
                        /* TODO
                          AssertionManager.assertCritical(
                              line + 1 <= code.length && code.isJMP(line + 1),
                              "ByteCode validation failed; EQ instruction is not followed by JMP"
                          );
                          break;*/
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