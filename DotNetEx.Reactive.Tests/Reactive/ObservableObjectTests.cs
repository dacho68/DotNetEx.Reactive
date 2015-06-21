using System.IO;
using System.Runtime.Serialization;
using DotNetEx.Reactive.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive.Reactive
{
	[TestClass]
	public sealed class ObservableObjectTests
	{
		[TestMethod]
		public void Changes_During_Initialize_Should_Not_Change_IsChanged_Property()
		{
			TestObservableObject obj = new TestObservableObject();

			obj.BeginInit();
			obj.Name = "Joe";
			obj.Nickname = "Orange";

			Assert.AreEqual( false, obj.IsChanged );
			Assert.AreEqual( true, obj.IsInitializing );

			obj.EndInit();

			Assert.AreEqual( false, obj.IsChanged );
			Assert.AreEqual( false, obj.IsInitializing );
		}


		[TestMethod]
		public void Changes_To_Children_During_Initialize_Must_Not_Change_IsChanged_Property()
		{
			TestObservableObject obj = new TestObservableObject();

			obj.BeginInit();
			obj.Name = "Joe";
			obj.Nickname = "Apple";
			obj.Child = new TestObservableObject();
			obj.Children.Add( new TestObservableObject() );

			Assert.AreEqual( true, obj.Child.IsInitializing );
			Assert.AreEqual( true, obj.Children.IsInitializing );
			Assert.AreEqual( true, obj.Children[ 0 ].IsInitializing );

			obj.EndInit();

			Assert.AreEqual( false, obj.Child.IsInitializing );
			Assert.AreEqual( false, obj.Children.IsInitializing );
			Assert.AreEqual( false, obj.Children[ 0 ].IsInitializing );
		}


		[TestMethod]
		public void Deserialization_Must_Not_Change_IsChanged_Property()
		{
			TestObservableObject obj = new TestObservableObject();

			obj.BeginInit();
			obj.Name = "Joe";
			obj.Nickname = "Apple";
			obj.EndInit();

			DataContractSerializer serializer = new DataContractSerializer( typeof( TestObservableObject ) );

			using ( var stream = new MemoryStream() )
			{
				serializer.WriteObject( stream, obj );
				stream.Position = 0;

				obj = (TestObservableObject)serializer.ReadObject( stream );
			}

			Assert.AreEqual( false, obj.IsChanged );
			Assert.AreEqual( false, obj.IsInitializing );

			obj.Name = "Jane";
			Assert.AreEqual( true, obj.IsChanged );

			using ( var stream = new MemoryStream() )
			{
				serializer.WriteObject( stream, obj );
				stream.Position = 0;

				obj = (TestObservableObject)serializer.ReadObject( stream );
			}

			Assert.AreEqual( true, obj.IsChanged );
		}


		[TestMethod]
		public void AcceptChanges_Must_Change_IsChanged_Property()
		{
			TestObservableObject obj = new TestObservableObject();

			obj.Name = "Joe";
			obj.Nickname = "Apple";

			Assert.AreEqual( true, obj.IsChanged );

			obj.AcceptChanges();

			Assert.AreEqual( false, obj.IsChanged );
		}
	}
}
