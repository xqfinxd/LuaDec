using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
	public class Validator
	{

		public static void process(Decompiler d)
		{
			Code code = d.code;
			for (int line = 1; line <= code.length; line++)
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

		//static only
		private Validator() { }

	}

}
