using System;

namespace DotNetEx.Reactive
{
	public sealed class TestCounter
	{
		public TestCounter()
		{
		}


		public Int32 Value { get; private set; }


		public void Increment( Int32 value )
		{
			this.Value += value;
		}
	}
}
