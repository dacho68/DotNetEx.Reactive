using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public interface IReadOnlyObservableList<T> : IObservableObject, INotifyCollectionChanged, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		/// <summary>
		/// Returns the first item in the collection or default( T ).
		/// </summary>
		T First { get; }


		/// <summary>
		/// Returns the last item in the collection or default( T ).
		/// </summary>
		T Last { get; }


		/// <summary>
		/// An observable for changes in the items contained in this list.
		/// </summary>
		IObservable<RxPropertyChange<T>> ItemsChanges { get; }
	}
}