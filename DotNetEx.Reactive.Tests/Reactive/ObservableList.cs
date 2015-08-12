﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
{
	public sealed partial class Tests
	{
		[TestMethod]
		[Description( "RemoveAll must remove only one element." )]
		public void ObservableList_Case_1()
		{
			ObservableList<Int32> list = new ObservableList<Int32>();

			list.Add( 1 );
			list.Add( 2 );
			list.Add( 3 );

			Assert.AreEqual( 1, list.RemoveAll( x => x <= 1 ) );
			Assert.AreEqual( 2, list.Count );
		}


		[TestMethod]
		[Description( "RemoveAll must remove all elements." )]
		public void ObservableList_Case_2()
		{
			ObservableList<Int32> list = new ObservableList<Int32>();

			list.Add( 1 );
			list.Add( 2 );
			list.Add( 3 );

			Assert.AreEqual( 3, list.RemoveAll( x => true ) );
			Assert.AreEqual( 0, list.Count );
		}


		[TestMethod]
		[Description( "" )]
		public void ObservableList_Case_3()
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();

			ObservableList<TestObservableObject> lists = new ObservableList<TestObservableObject>( Enumerable.Range( 1, 1000 ).Select( x => new TestObservableObject( x ) ) );

			lists.BeginInit();

			foreach ( var list in lists )
			{
				list.Children = new ObservableList<TestObservableObject>( Enumerable.Range( 1, 100 ).Select( x => new TestObservableObject( x ) ) );
			}

			lists.EndInit();
			watch.Stop();

			Assert.IsTrue( watch.ElapsedMilliseconds < 100 );
		}
	}
}
