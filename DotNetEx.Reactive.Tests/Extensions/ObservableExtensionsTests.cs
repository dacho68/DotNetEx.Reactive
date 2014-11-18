using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	[TestClass]
	public sealed class ObservableExtensionsTests
	{
		[TestMethod]
		public void Should_Raise_Change_Only_Once_Per_Change()
		{
			TestItem item = new TestItem();
			TestCounter counter = new TestCounter();

			item.Observe( x => new { Name = x.Name, Text = x.DisplayText } ).Subscribe( x => counter.Increment( 1 ) );
			Assert.AreEqual( 1, counter.Count );

			item.Name = "Ivan";
			Assert.AreEqual( 2, counter.Count );

			item.Age = 5;
			Assert.AreEqual( 3, counter.Count );
		}
	}
}
