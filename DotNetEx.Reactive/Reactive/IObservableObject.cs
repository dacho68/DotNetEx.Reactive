using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public interface IObservableObject : INotifyPropertyChanged, IChangeTracking
	{
		IObservable<PropertyChangedEventArgs> PropertyChanges { get; }
	}
}
