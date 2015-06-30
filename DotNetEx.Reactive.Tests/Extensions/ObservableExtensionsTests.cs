using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	[TestClass]
	public sealed class ObservableExtensionsTests
	{
		[TestMethod]
		public void Observe_Should_Raise_Change_Only_Once_Per_Change()
		{
			TestItem item = new TestItem();
			TestCounter counter = new TestCounter();

			item.Observe( x => new { Name = x.Name, Text = x.DisplayText } ).Subscribe( x => counter.Increment( 1 ) );
			Assert.AreEqual( 1, counter.Value );

			item.Name = "Ivan";
			Assert.AreEqual( 2, counter.Value );

			item.Age = 5;
			Assert.AreEqual( 3, counter.Value );
		}


		[TestMethod]
		public void Observe_Supports_Nested_Properties()
		{
			TestItem item = new TestItem();
			TestCounter counter = new TestCounter();

			item.Observe( x => new 
			{ 
				Name = x.Name, 
				Text = x.DisplayText,
				ChildName = x.Child != null ? x.Child.Name : null,
				ChildrenCount = x.Children.Count,
				FirstChildrenName = x.Children.Count > 0 ? x.Children[ 0 ].Name : null
			} ).Subscribe( x => counter.Increment( 1 ) );

			Assert.AreEqual( 1, counter.Value );

			item.Name = "Ivan";
			Assert.AreEqual( 2, counter.Value );

			item.Child = new TestItem();
			Assert.AreEqual( 2, counter.Value );

			item.Child.Name = "Ivan";
			Assert.AreEqual( 3, counter.Value );

			item.Children.Add( new TestItem() );
			Assert.AreEqual( 4, counter.Value );
		}
	}
}
