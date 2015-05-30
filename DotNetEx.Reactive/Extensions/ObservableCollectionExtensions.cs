using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public static class ObservableCollectionExtensions
	{
		public static IObservable<RxPropertyChange<T>> ObserveItems<T>( this ObservableCollection<T> collection )
			where T : class, INotifyPropertyChanged
		{
			Check.NotNull( collection, "collection" );

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
			where TCollection : class, INotifyCollectionChanged
		{
			Check.NotNull( collection, "collection" );

			var changes = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>( x => collection.CollectionChanged += x, x => collection.CollectionChanged -= x );

			return changes.Select( x => x.EventArgs );
		}


		public static void AddRange<T>( this ObservableCollection<T> collection, IEnumerable<T> values )
		{
			Check.NotNull( collection, "collection" );

			FieldInfo field = typeof( ObservableCollection<T> ).GetField( "CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic );

			if ( field == null )
			{
				throw new InvalidOperationException( "ObservableCollection<T> class doesn't have a CollectionChanged backing field." );
			}

			MulticastDelegate eventDelegate = (MulticastDelegate)field.GetValue( collection );
			List<T> addedValues = new List<T>();

			try
			{
				field.SetValue( collection, null ); // Disable any notifications

				foreach ( var value in values )
				{
					collection.Add( value );
					addedValues.Add( value );
				}
			}
			finally
			{
				field.SetValue( collection, eventDelegate );
			}

			if ( addedValues.Count > 0 && eventDelegate != null )
			{
				eventDelegate.DynamicInvoke( collection, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedValues ) );
			}
		}


		public static void Reset<T>( this ObservableCollection<T> collection, IEnumerable<T> values )
		{
			Check.NotNull( collection, "collection" );

			FieldInfo field = typeof( ObservableCollection<T> ).GetField( "CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic );

			if ( field == null )
			{
				throw new InvalidOperationException( "ObservableCollection<T> class doesn't have a CollectionChanged backing field." );
			}

			MulticastDelegate eventDelegate = (MulticastDelegate)field.GetValue( collection );

			try
			{
				field.SetValue( collection, null ); // Disable any notifications
				collection.Clear();

				foreach ( var value in values )
				{
					collection.Add( value );
				}
			}
			finally
			{
				field.SetValue( collection, eventDelegate );
			}

			if ( eventDelegate != null )
			{
				eventDelegate.DynamicInvoke( collection, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}


		public static IDisposable Merge<T>( this ObservableCollection<T> target, IObservable<T> add, IObservable<T> remove )
		{
			Check.NotNull( target, "target" );
			Check.NotNull( add, "add" );
			Check.NotNull( remove, "remove" );

			Dictionary<T, Int32> values = new Dictionary<T, Int32>();
			target.Clear();

			IDisposable addSubscription = add.Subscribe( x =>
			{
				lock ( values )
				{
					if ( values.ContainsKey( x ) )
					{
						++values[ x ];
					}
					else
					{
						values.Add( x, 1 );
						target.Add( x );
					}
				}
			} );

			IDisposable removeSubscription = remove.Subscribe( x =>
			{
				lock ( values )
				{
					if ( values.ContainsKey( x ) && --values[ x ] == 0 )
					{
						values.Remove( x );
						target.Remove( x );
					}
				}
			} );

			return Disposable.Create( () =>
			{
				addSubscription.Dispose();
				removeSubscription.Dispose();
				target.Clear();
			} );
		}


		private static readonly ConditionalWeakTable<Object, WeakReference<Object>> s_collectionListeners = new ConditionalWeakTable<Object, WeakReference<Object>>();
	}
}
