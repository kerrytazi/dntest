using TestLib;

namespace TestGenLib
{
	public class TestGenLibClass : TestBaseLibClass
	{
		private static Dictionary<int, string> dict = new Dictionary<int, string>();

		public void Test()
		{
			var inst = new TestLibClass(3);
			Console.WriteLine(inst.AddA(4));
		}
	}
}
