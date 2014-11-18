using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;

namespace DotNetEx.Reactive
{
	internal sealed class ObservableCollectionListener<TCollection, TItem> 
		where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
		where TItem : class, INotifyPropertyChanged
	{
		public ObservableCollectionListener( TCollection collection )
		{
			m_collection = collection;
			m_collection.CollectionChanged += OnCollectionChanged;

			this.Subscribe( collection );
		}


		public IObservable<RxPropertyChange<TItem>> Changes
		{
			get
			{
				return m_changes;
			}
		}


		private void OnItemPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			m_changes.OnNext( new RxPropertyChange<TItem>( (TItem)sender, e.PropertyName ) );
		}


		private void OnCollectionChanged( Object sender, NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					Subscribe( e.NewItems.OfType<TItem>() );
					break;

				case NotifyCollectionChangedAction.Remove:
					Unsubscribe( e.OldItems.OfType<TItem>() );
					break;

				case NotifyCollectionChangedAction.Replace:
					Unsubscribe( e.OldItems.OfType<TItem>() );
					Subscribe( e.NewItems.OfType<TItem>() );
					break;

				case NotifyCollectionChangedAction.Reset:
					Unsubscribe( e.OldItems != null ? e.OldItems.OfType<TItem>() : m_subscribed.ToArray() );
					Subscribe( m_collection );
					break;
			}
		}


		private void Subscribe( IEnumerable<TItem> items )
		{
			foreach ( var item in items )
			{
				if ( item != null )
				{
					if ( m_subscribed.Add( item ) )
					{
						item.PropertyChanged += this.OnItemPropertyChanged;
					}
				}
			}
		}


		private void Unsubscribe( IEnumerable<TItem> items )
		{
			foreach ( var item in items )
			{
				if ( item != null )
				{
					if ( m_subscribed.Remove( item ) )
					{
						item.PropertyChanged -= this.OnItemPropertyChanged;
					}
				}
			}
		}


		private readonly TCollection m_collection;
		private readonly Subject<RxPropertyChange<TItem>> m_changes = new Subject<RxPropertyChange<TItem>>();
		private readonly HashSet<TItem> m_subscribed = new HashSet<TItem>( ReferenceEqualityComparer<TItem>.Instance );
	}
}