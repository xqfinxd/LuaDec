using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
	public class AssertionManager
	{
		public static bool assertCritical(bool condition, string message)
		{
			if (condition)
			{
				// okay
			}
			else
			{
				critical(message);
			}
			return condition;
		}

		public static void critical(string message)
		{
			throw new System.InvalidOperationException(message);
		}

		//static only
		private AssertionManager() { }

	}
}
