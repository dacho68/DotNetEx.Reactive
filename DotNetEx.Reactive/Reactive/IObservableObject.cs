using System;
using System.ComponentModel;

namespace DotNetEx.Reactive
{
	public interface IObservableObject : INotifyPropertyChanged
	{
		IObservable<PropertyChangedEventArgs> PropertyChanges { get; }
	}
}
