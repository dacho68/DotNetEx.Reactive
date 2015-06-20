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


		private String m_name;
		private String m_nickname;
	}
}
