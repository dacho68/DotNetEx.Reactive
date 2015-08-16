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

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );

			Assert.AreEqual( true, list.ContainsKey( 1 ) );
			Assert.AreEqual( false, list.ContainsKey( 3 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values." )]
		public void ObservableDictionary_Case_2()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );

			Assert.AreEqual( -1, list.IndexOf( 3 ) );
			Assert.AreEqual( 0, list.IndexOf( 1 ) );
			Assert.AreEqual( 1, list.IndexOf( 2 ) );

			list.Insert( 0, new ObservableKeyValuePair<Int32, TestObservableObject>( 3, new TestObservableObject( 3 ) ) );

			Assert.AreEqual( 0, list.IndexOf( 3 ) );
			Assert.AreEqual( 1, list.IndexOf( 1 ) );
			Assert.AreEqual( 2, list.IndexOf( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after insert." )]
		public void ObservableDictionary_Case_3()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );

			list.Insert( 0, new ObservableKeyValuePair<int,TestObservableObject>( 3, new TestObservableObject( 3 ) ) );

			Assert.AreEqual( 0, list.IndexOf( 3 ) );
			Assert.AreEqual( 1, list.IndexOf( 1 ) );
			Assert.AreEqual( 2, list.IndexOf( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after move." )]
		public void ObservableDictionary_Case_4()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );
			list.Add( 3, new TestObservableObject( 3 ) );

			list.MoveItem( 2, 0 );

			Assert.AreEqual( 0, list.IndexOf( 3 ) );
			Assert.AreEqual( 1, list.IndexOf( 1 ) );
			Assert.AreEqual( 2, list.IndexOf( 2 ) );
		}


		[TestMethod]
		[Description( "IndexOfKey must report correct values after remove." )]
		public void ObservableDictionary_Case_5()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );
			list.Add( 3, new TestObservableObject( 3 ) );

			Assert.AreEqual( true, list.Remove( 2 ) );

			Assert.AreEqual( 0, list.IndexOf( 1 ) );
			Assert.AreEqual( -1, list.IndexOf( 2 ) );
			Assert.AreEqual( 1, list.IndexOf( 3 ) );
		}


		[TestMethod]
		public void ObservableDictionary_Case_6()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );

			Assert.AreEqual( true, list.IsChanged );

			list.AcceptChanges();

			Assert.AreEqual( false, list.IsChanged );

			list[ 0 ].Value.Nickname = "Tester";

			Assert.AreEqual( true, list[ 0 ].IsChanged );
			Assert.AreEqual( true, list.IsChanged );

			list.AcceptChanges();

			Assert.AreEqual( false, list[ 0 ].IsChanged );
			Assert.AreEqual( false, list.IsChanged );
		}


		[TestMethod]
		public void ObservableDictionary_Case_7()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.BeginInit();
			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 2, new TestObservableObject( 2 ) );
			list.EndInit();

			Assert.AreEqual( false, list.IsChanged );

			list.AddOrUpdate( 1, new TestObservableObject( 1 ) );

			Assert.AreEqual( true, list.IsChanged );
		}


		[TestMethod]
		[ExpectedException( typeof( ArgumentException ) )]
		public void ObservableDictionary_Must_Not_Allow_Duplicate_Keys()
		{
			ObservableDictionary<Int32, TestObservableObject> list = new ObservableDictionary<Int32, TestObservableObject>();

			list.Add( 1, new TestObservableObject( 1 ) );
			list.Add( 1, new TestObservableObject( 1 ) );
		}
	}
}
