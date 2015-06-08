using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DotNetEx.Reactive
{
	public interface IObservableList<T> : IList<T>, ICollection<T>, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged
	{
		IObservable<RxPropertyChange<T>> ItemsChanges { get; }


		IObservable<NotifyCollectionChangedEventArgs> CollectionChanges { get; }
	}
}
