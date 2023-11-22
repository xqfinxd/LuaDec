using LuaDec.Decompile.Expression;
using LuaDec.Parser;

namespace LuaDec.Decompile
{
    public class Function
    {
        private readonly CodeExtract extract;
        private Constant[] constants;
        private Version version;

        public Function(LFunction function)
        {
            version = function.header.version;
            constants = new Constant[function.constants.Length];
            for (int i = 0; i < constants.Length; i++)
            {
                constants[i] = new Constant(function.constants[i]);
            }
            extract = function.header.extractor;
        }

        public int ConstantIndex(int register)
        {
            return extract.get_k(register);
        }

        public ConstantExpression GetConstantExpression(int constantIndex)
        {
            Constant constant = constants[constantIndex];
            return new ConstantExpression(constant, constant.isIdentifier(version), constantIndex);
        }

        public GlobalExpression GetGlobalExpression(int constantIndex)
        {
            return new GlobalExpression(GetGlobalName(constantIndex), constantIndex);
        }

        public ConstantExpression GetGlobalName(int constantIndex)
        {
            Constant constant = constants[constantIndex];
            if (!constant.isIdentifierPermissive(version)) throw new System.InvalidOperationException();
            return new ConstantExpression(constant, true, constantIndex);
        }

        public Version GetVersion()
        {
            return version;
        }

        public bool IsConstant(int register)
        {
            return extract.is_k(register);
        }
    }
}