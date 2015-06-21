using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEx.Reactive.Assets
{
	[DataContract]
	public sealed class TestObservableObject : ObservableObject
	{
		public TestObservableObject()
		{
			this.BeginInit();

			this.Children = new ObservableList<TestObservableObject>();

			this.EndInit();
		}


		[DataMember]
		public String Name
		{
			get
			{
				return m_name;
			}
			set
			{
				this.SetValue( ref m_name, value );
			}
		}


		[DataMember]
		public String Nickname
		{
			get
			{
				return m_nickname;
			}
			set
			{
				this.SetValue( ref m_nickname, value );
			}
		}


		[DataMember]
		public TestObservableObject Child
		{
			get
			{
				return m_child;
			}
			set
			{
				this.SetValue( ref m_child, value );
			}
		}


		[DataMember]
		public ObservableList<TestObservableObject> Children
		{
			get
			{
				return m_children;
			}
			private set
			{
				this.SetValue( ref m_children, value );
			}
		}


		private String m_name;
		private String m_nickname;
		private TestObservableObject m_child;
		private ObservableList<TestObservableObject> m_children;
	}
}
