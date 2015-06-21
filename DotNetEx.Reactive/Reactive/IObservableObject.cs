using System;
using System.ComponentModel;

namespace DotNetEx.Reactive
{
	public interface IObservableObject : INotifyPropertyChanged, IChangeTracking, ISupportInitialize
	{
		IObservable<PropertyChangedEventArgs> PropertyChanges { get; }
	}
}
