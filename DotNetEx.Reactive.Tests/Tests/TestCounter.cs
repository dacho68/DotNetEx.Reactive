using System;

namespace DotNetEx.Reactive
{
	public sealed class TestCounter
	{
		public TestCounter()
		{
		}


		public Int32 Count { get; private set; }


		public void Increment( Int32 value )
		{
			this.Count += value;
		}
	}
}
