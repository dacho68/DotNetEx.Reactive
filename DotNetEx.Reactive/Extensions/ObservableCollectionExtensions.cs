using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace DotNetEx.Reactive
{
	public static class ObservableCollectionExtensions
	{
		public static IObservable<RxPropertyChange<T>> ObserveItems<T>( this ObservableCollection<T> collection )
			where T : INotifyPropertyChanged
		{
			if ( collection == null )
			{
				return null;
			}

			lock ( s_collectionListeners )
			{
				WeakReference<Object> reference;
				Object listener;

				if ( !s_collectionListeners.TryGetValue( collection, out reference ) || !reference.TryGetTarget( out listener ) )
				{
					listener = new ObservableCollectionListener<ObservableCollection<T>, T>( collection );

					if ( reference == null )
					{
						reference = new WeakReference<Object>( listener );
						s_collectionListeners.Add( collection, reference );
					}
					else
					{
						reference.SetTarget( listener );
					}
				}

				return ( (ObservableCollectionListener<ObservableCollection<T>, T>)listener ).Changes;
			}
		}


		public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollection<TCollection>( this TCollection collection )
			where TCollection : INotifyCollectionChanged
		{
			if ( collection == null )
			{
				return null;
			}

			var changes = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>( x => collection.CollectionChanged += x, x => collection.CollectionChanged -= x );

			return changes.Select( x => x.EventArgs );
		}


		private static readonly ConditionalWeakTable<Object, WeakReference<Object>> s_collectionListeners = new ConditionalWeakTable<Object, WeakReference<Object>>();
	}
}
