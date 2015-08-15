using System.IO;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetEx.Reactive
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
			obj.Child = new TestObservableObject( 1 );
			obj.Child.Name = "Ivan";

			Assert.AreEqual( true, obj.IsChanged );

			obj.AcceptChanges();

			Assert.AreEqual( false, obj.IsChanged );
		}
	}
}
