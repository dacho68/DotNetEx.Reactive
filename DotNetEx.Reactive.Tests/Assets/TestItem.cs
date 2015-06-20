using System;
using System.ComponentModel;

namespace DotNetEx.Reactive
{
	public sealed class TestItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;


		public Int32 Id
		{
			get
			{
				return m_id;
			}
			set
			{
				this.PropertyChanged.RaiseWhenChanged( this, ref m_id, value );
			}
		}


		public String Name
		{
			get
			{
				return m_name;
			}
			set
			{
				this.PropertyChanged.RaiseWhenChanged( this, ref m_name, value );
			}
		}


		public Int32 Age
		{
			get
			{
				return m_age;
			}
			set
			{
				this.PropertyChanged.RaiseWhenChanged( this, ref m_age, value );
			}
		}


		[References( "Name", "Age" )]
		public String DisplayText
		{
			get
			{
				return "Name: " + this.Name + ", Age: " + this.Age;
			}
		}


		private String m_name;
		private Int32 m_age;
		private Int32 m_id;
	}
}
