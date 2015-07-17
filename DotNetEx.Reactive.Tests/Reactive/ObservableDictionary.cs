using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	[TestClass]
	public sealed partial class Tests
	{
		[TestMethod]
		[Description( "ContainsKey must return true for existing element and false for non existing." )]
		public void ObservableDictionary_Case_1()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );

			Assert.AreEqual( true, list.ContainsKey( 1 ) );
			Assert.AreEqual( false, list.ContainsKey( 3 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values." )]
		public void ObservableDictionary_Case_2()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );

			Assert.AreEqual( -1, list.IndexOfKey( 3 ) );
			Assert.AreEqual( 0, list.IndexOfKey( 1 ) );
			Assert.AreEqual( 1, list.IndexOfKey( 2 ) );

			list.Insert( 0, new TestObservableObject( 3 ) );

			Assert.AreEqual( 0, list.IndexOfKey( 3 ) );
			Assert.AreEqual( 1, list.IndexOfKey( 1 ) );
			Assert.AreEqual( 2, list.IndexOfKey( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after insert." )]
		public void ObservableDictionary_Case_3()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );

			list.Insert( 0, new TestObservableObject( 3 ) );

			Assert.AreEqual( 0, list.IndexOfKey( 3 ) );
			Assert.AreEqual( 1, list.IndexOfKey( 1 ) );
			Assert.AreEqual( 2, list.IndexOfKey( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after move." )]
		public void ObservableDictionary_Case_4()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );
			list.Add( new TestObservableObject( 3 ) );

			list.MoveItem( 2, 0 );

			Assert.AreEqual( 0, list.IndexOfKey( 3 ) );
			Assert.AreEqual( 1, list.IndexOfKey( 1 ) );
			Assert.AreEqual( 2, list.IndexOfKey( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after remove." )]
		public void ObservableDictionary_Case_5()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );
			list.Add( new TestObservableObject( 3 ) );

			Assert.AreEqual( true, list.RemoveKey( 2 ) );

			Assert.AreEqual( 0, list.IndexOfKey( 1 ) );
			Assert.AreEqual( -1, list.IndexOfKey( 2 ) );
			Assert.AreEqual( 1, list.IndexOfKey( 3 ) );
		}


		[TestMethod]
		public void ObservableDictionary_Case_6()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( new TestObservableObject( 1 ) );
			list.Add( new TestObservableObject( 2 ) );

			Assert.AreEqual( true, list.IsChanged );

			list.AcceptChanges();

			Assert.AreEqual( false, list.IsChanged );

			list[ 0 ].Nickname = "Tester";

			Assert.AreEqual( true, list[ 0 ].IsChanged );
			Assert.AreEqual( true, list.IsChanged );

			list.AcceptChanges();

			Assert.AreEqual( false, list[ 0 ].IsChanged );
			Assert.AreEqual( false, list.IsChanged );
		}
	}
}
