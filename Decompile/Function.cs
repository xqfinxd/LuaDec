using LuaDec.Decompile.Expression;
using LuaDec.Parser;

namespace LuaDec.Decompile
{
    public class Function
    {

        private Version version;
        private Constant[] constants;
        private readonly CodeExtract extract;

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

        public bool isConstant(int register)
        {
            return extract.is_k(register);
        }

        public int constantIndex(int register)
        {
            return extract.get_k(register);
        }

        public ConstantExpression getGlobalName(int constantIndex)
        {
            Constant constant = constants[constantIndex];
            if (!constant.isIdentifierPermissive(version)) throw new System.InvalidOperationException();
            return new ConstantExpression(constant, true, constantIndex);
        }

        public ConstantExpression getConstantExpression(int constantIndex)
        {
            Constant constant = constants[constantIndex];
            return new ConstantExpression(constant, constant.isIdentifier(version), constantIndex);
        }

        public GlobalExpression getGlobalExpression(int constantIndex)
        {
            return new GlobalExpression(getGlobalName(constantIndex), constantIndex);
        }

        public Version getVersion()
        {
            return version;
        }

    }

}
