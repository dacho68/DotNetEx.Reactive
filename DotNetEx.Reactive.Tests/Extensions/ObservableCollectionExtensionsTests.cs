using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	[TestClass]
	public sealed class ObservableCollectionExtensionsTests
	{
		[TestMethod]
		public void Clear_Does_Not_Provide_Old_Values()
		{
			ObservableCollection<TestItem> collection = new ObservableCollection<TestItem>();
			TestCounter counter = new TestCounter();

			collection.ObserveItems().Subscribe( x => counter.Increment( 1 ) );
			Assert.AreEqual( 0, counter.Value );

			TestItem item = new TestItem();
			collection.Add( item );
			Assert.AreEqual( 0, counter.Value );

			item.Id = 1;
			Assert.AreEqual( 1, counter.Value );

			collection.Remove( item );
			Assert.AreEqual( 1, counter.Value );

			item.Id = 2;
			Assert.AreEqual( 1, counter.Value );

			collection.Add( item );
			collection.Clear();
			Assert.AreEqual( 1, counter.Value );
		}


		[TestMethod]
		public void Add_Range()
		{
			ObservableCollection<Int32> collection = new ObservableCollection<Int32>();
			TestCounter counterInvokes = new TestCounter();
			TestCounter counterItems = new TestCounter();

			collection.ObserveCollection().Subscribe( x => counterInvokes.Increment( 1 ) );
			collection.ObserveCollection().Subscribe( x => counterItems.Increment( x.NewItems.Count ) );
			
			collection.AddRange( Enumerable.Range( 0, 10 ) );
			Assert.AreEqual( 1, counterInvokes.Value );
			Assert.AreEqual( 10, counterItems.Value );

			collection.AddRange( Enumerable.Range( 0, 20 ) );
			Assert.AreEqual( 2, counterInvokes.Value );
			Assert.AreEqual( 30, counterItems.Value );
		}


		[TestMethod]
		public void Reset()
		{
			ObservableCollection<Int32> collection = new ObservableCollection<Int32>();
			TestCounter counterInvokes = new TestCounter();
			TestCounter counterItems = new TestCounter();

			collection.ObserveCollection().Subscribe( x => counterInvokes.Increment( 1 ) );
			collection.ObserveCollection().Subscribe( x => counterItems.Increment( x.NewItems != null ? x.NewItems.Count : 0 ) );

			collection.AddRange( Enumerable.Range( 0, 10 ) );
			Assert.AreEqual( 1, counterInvokes.Value );
			Assert.AreEqual( 10, counterItems.Value );

			collection.Reset( Enumerable.Range( 0, 20 ) );
			Assert.AreEqual( 2, counterInvokes.Value );
			Assert.AreEqual( 10, counterItems.Value );
		}
	}
}
