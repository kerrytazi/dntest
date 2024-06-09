namespace TestLib
{
	public class TestLibClass
	{
		private int valA;

		public TestLibClass(int a)
		{
			valA = a;
		}

		public int AddA(int b)
		{
			return valA + b;
		}
	}

	public class TestBaseLibClass
	{
	}
}
