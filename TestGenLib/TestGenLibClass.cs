using TestLib;

namespace TestGenLib
{
	public class TestGenLibClass
	{
		public void Test()
		{
			var inst = new TestLibClass(3);
			Console.WriteLine(inst.AddA(4));
		}
	}
}
