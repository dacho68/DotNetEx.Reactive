using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	[TestClass]
	public sealed class ObservableListTests
	{
		[TestMethod]
		public void Remove_All_Must_Remove_Only_One_Element()
		{
			ObservableList<Int32> list = new ObservableList<Int32>();

			list.Add( 1 );
			list.Add( 2 );
			list.Add( 3 );

			Assert.AreEqual( 1, list.RemoveAll( x => x <= 1 ) );
			Assert.AreEqual( 2, list.Count );
		}


		[TestMethod]
		public void Remove_All_Must_Remove_All_Elements()
		{
			ObservableList<Int32> list = new ObservableList<Int32>();

			list.Add( 1 );
			list.Add( 2 );
			list.Add( 3 );

			Assert.AreEqual( 3, list.RemoveAll( x => true ) );
			Assert.AreEqual( 0, list.Count );
		}


		[TestMethod]
		public void Item_Change_Must_Raise_Is_Changed_Of_The_List()
		{
			ObservableList<TestObservableObject> items = new ObservableList<TestObservableObject>( Enumerable.Range( 0, 5 ).Select( x => new TestObservableObject( x ) ) );

			items.BeginInit();
			items.Add( new TestObservableObject() );
			items.Add( new TestObservableObject() );
			items.EndInit();

			Assert.AreEqual( false, items.IsChanged );

			items[ 1 ].Name = "Joe";

			Assert.AreEqual( true, items.IsChanged );
		}
	}
}
