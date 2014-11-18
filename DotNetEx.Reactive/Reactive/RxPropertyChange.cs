using System;
using System.ComponentModel;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public struct RxPropertyChange<T> where T : INotifyPropertyChanged
	{
		internal RxPropertyChange( T source, String propertyName ) :
			this()
		{
			Check.NotNull( propertyName, "propertyName" );

			this.Source = source;
			this.PropertyName = propertyName;
		}


		/// <summary>
		/// Gets the source.
		/// </summary>
		public T Source { get; private set; }


		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		public String PropertyName { get; private set; }
	}
}
