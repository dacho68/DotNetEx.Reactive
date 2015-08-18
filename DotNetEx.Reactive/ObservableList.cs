using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;

namespace DotNetEx.Reactive
{
	/// <summary>
	/// Implementation of a dynamic data collection based on ObservableObject that uses List&lt;T&gt; as internal container, 
	/// implementing INotifyCollectionChanged to notify listeners when items get added, removed or the whole list is refreshed.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable" )]
	public class ObservableList<T> : 
		ObservableObject, INotifyCollectionChanged, 
		IList, IList<T>, IReadOnlyList<T>, IReadOnlyObservableList<T>,
		ICollection, ICollection<T>, IReadOnlyCollection<T>, 
		IEnumerable, IEnumerable<T>
	{
		/// <summary>
		/// Initializes a new empty instance of ObservableList.
		/// </summary>
		public ObservableList()
		{
			m_items = new List<T>();
		}


		public ObservableList( Int32 capacity )
		{
			m_items = new List<T>( capacity );
		}


		/// <summary>
		/// Initializes a new instance of ObservableList class that contains
		/// elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param>
		/// <exception cref="ArgumentNullException">collection is a null reference</exception>
		public ObservableList( IEnumerable<T> collection )
		{
			Check.NotNull( collection, "collection" );

			m_items = new List<T>( collection );

			if ( ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
			{
				foreach ( INotifyPropertyChanged observable in m_items )
				{
					if ( observable != null )
					{
						observable.PropertyChanged += this.OnItemPropertyChanged;
					}
				}
			}
		}


		/// <summary>
		/// Event for notifying about changes in the list such as Add, Remove, Reset, Move.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;


		/// <summary>
		/// An observable for the CollectionChanged event.
		/// </summary>
		public IObservable<NotifyCollectionChangedEventArgs> CollectionChanges
		{
			get
			{
				if ( m_collectionChanges == null )
				{
					m_collectionChanges = new Subject<NotifyCollectionChangedEventArgs>();
				}

				return m_collectionChanges;
			}
		}


		/// <summary>
		/// An observable for changes in the items contained in this list.
		/// </summary>
		public IObservable<RxPropertyChange<T>> ItemsChanges
		{
			get
			{
				if ( !ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
				{
					return null;
				}

				if ( m_itemsChanges == null )
				{
					m_itemsChanges = new Subject<RxPropertyChange<T>>();
				}

				return m_itemsChanges;
			}
		}


		/// <summary>
		/// Returns how many elements are in the list.
		/// </summary>
		public Int32 Count
		{
			get
			{
				return m_items.Count;
			}
		}


		/// <summary>
		/// Gets or sets the first item in the list. If the list is empty, the getter will return default( T ).
		/// The setter will add the value to the collection if the list is empty or replace the first item.
		/// </summary>
		public T First
		{
			get
			{
				if ( m_items.Count > 0 )
				{
					return m_items[ 0 ];
				}

				return default( T );
			}
			set
			{
				if ( m_items.Count == 0 )
				{
					this.Add( value );
				}
				else
				{
					this[ 0 ] = value;
				}
			}
		}


		/// <summary>
		/// Gets or sets the last item in the list. If the list is empty, the getter will return default( T ).
		/// The setter will add the value to the collection if the list is empty or replace the last item.
		/// </summary>
		public T Last
		{
			get
			{
				if ( m_items.Count > 0 )
				{
					return m_items[ m_items.Count - 1 ];
				}

				return default( T );
			}
			set
			{
				if ( m_items.Count == 0 )
				{
					this.Add( value );
				}
				else
				{
					this[ m_items.Count - 1 ] = value;
				}
			}
		}


		public T this[ Int32 index ]
		{
			get
			{
				return m_items[ index ];
			}
			set
			{
				T originalItem = m_items[ index ];

				if ( !EqualityComparer<T>.Default.Equals( originalItem, value ) )
				{
					this.OnRemove( originalItem );
					this.OnInsert( value, index );

					m_items[ index ] = value;

					this.RaiseChanged();

					if ( index == 0 )
					{
						this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
					}
					else if ( index == m_items.Count - 1 )
					{
						this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
					}

					this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
					this.RaiseCollectionChanged( NotifyCollectionChangedAction.Replace, originalItem, value, index );
				}
			}
		}


		public void Add( T item )
		{
			Int32 itemIndex = m_items.Count;

			this.OnInsert( item, itemIndex );

			m_items.Add( item );

			this.RaiseChanged();
			this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
			this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
			this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
			this.RaiseCollectionChanged( NotifyCollectionChangedAction.Add, item, itemIndex );
		}


		public void Clear()
		{
			if ( m_items.Count > 0 )
			{
				m_items.ForEach( x => this.OnRemove( x ) );
				m_items.Clear();

				this.RaiseChanged();
				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
				this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
				this.RaiseCollectionReset();
			}
		}


		public void Reset( IEnumerable<T> items )
		{
			Check.NotNull( items, "items" );

			Boolean cleared = false;

			if ( m_items.Count > 0 )
			{
				m_items.ForEach( x => this.OnRemove( x ) );
				m_items.Clear();

				cleared = true;
			}

			ICollection<T> collection = items as ICollection<T>;
			Int32 itemIndex = -1;

			if ( collection != null )
			{
				m_items.Capacity = collection.Count;
			}

			foreach ( var item in items )
			{
				this.OnInsert( item, ++itemIndex );

				m_items.Add( item );
			}

			if ( cleared || m_items.Count > 0 )
			{
				this.RaiseChanged();
				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
				this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
				this.RaiseCollectionReset();
			}
		}


		public void CopyTo( T[] array, Int32 index )
		{
			m_items.CopyTo( array, index );
		}


		public Boolean Contains( T item )
		{
			return m_items.Contains( item );
		}


		public Boolean Contains( T item, IEqualityComparer<T> comparer )
		{
			return m_items.Contains( item, comparer );
		}


		public IEnumerator<T> GetEnumerator()
		{
			return m_items.GetEnumerator();
		}


		public Int32 IndexOf( T item )
		{
			return m_items.IndexOf( item );
		}


		public void Insert( Int32 index, T item )
		{
			this.OnInsert( item, index );

			m_items.Insert( index, item );

			for ( Int32 i = index + 1; i < m_items.Count; ++i )
			{
				this.OnMove( m_items[ i ], i );
			}

			this.RaiseChanged();

			if ( index == 0 )
			{
				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
			}
			else if ( index == m_items.Count - 1 )
			{
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
			}

			this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
			this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
			this.RaiseCollectionChanged( NotifyCollectionChangedAction.Add, item, index );
		}


		public Boolean Remove( T item )
		{
			Int32 index = m_items.IndexOf( item );

			if ( index > -1 )
			{
				this.RemoveAt( index );

				return true;
			}

			return false;
		}


		public void RemoveAt( Int32 index )
		{
			T item = m_items[ index ];

			this.OnRemove( item );

			m_items.RemoveAt( index );

			for ( Int32 i = index; i < m_items.Count; ++i )
			{
				this.OnMove( m_items[ i ], i );
			}

			this.RaiseChanged();

			if ( index == 0 )
			{
				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
			}
			else if ( index == m_items.Count - 2 )
			{
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
			}

			this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
			this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
			this.RaiseCollectionChanged( NotifyCollectionChangedAction.Remove, item, index );
		}


		public void MoveItem( Int32 oldIndex, Int32 newIndex )
		{
			if ( oldIndex < 0 || oldIndex >= m_items.Count )
			{
				throw new ArgumentOutOfRangeException( "oldIndex" );
			}
			else if ( newIndex < 0 || newIndex >= m_items.Count )
			{
				throw new ArgumentOutOfRangeException( "newIndex" );
			}

			if ( oldIndex != newIndex )
			{
				T removedItem = m_items[ oldIndex ];

				m_items.RemoveAt( oldIndex );
				m_items.Insert( newIndex, removedItem );

				// All items between oldIndex and newIndex have their indexes invalidated.
				if ( oldIndex < newIndex )
				{
					for ( Int32 i = oldIndex; i <= newIndex; ++i )
					{
						this.OnMove( m_items[ i ], i );
					}
				}
				else
				{
					for ( Int32 i = newIndex; i <= oldIndex; ++i )
					{
						this.OnMove( m_items[ i ], i );
					}
				}

				this.RaiseChanged();

				if ( oldIndex == 0 || newIndex == 0 )
				{
					this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
				}

				if ( oldIndex == m_items.Count - 1 || newIndex == m_items.Count - 1 )
				{
					this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
				}

				this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
				this.RaiseCollectionChanged( NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex );
			}
		}


		public void ForEach( Action<T> action )
		{
			m_items.ForEach( action );
		}


		public void Sort( Comparison<T> comparison )
		{
			Check.NotNull( comparison, "comparison" );

			if ( m_items.Count > 0 )
			{
				m_items.Sort( comparison );

				for ( Int32 i = 0; i < m_items.Count; ++i )
				{
					this.OnMove( m_items[ i ], i );
				}

				this.RaiseChanged();
				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
				this.RaiseCollectionReset();
			}
		}


		/// <summary>
		/// This method removes all items which matches the predicate.
		/// </summary>
		public Int32 RemoveAll( Predicate<T> predicate )
		{
			Check.NotNull( predicate, "predicate" );

			Int32 removeCount = 0;

			for ( Int32 i = this.Count - 1; i >= 0; --i )
			{
				if ( predicate( m_items[ i ] ) )
				{
					this.RemoveAt( i );

					++removeCount;
				}
			}

			return removeCount;
		}


		public override void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				if ( m_items.Count > 0 && ReflectionTraits.Assignable<IChangeTracking, T>.Value )
				{
					foreach ( var item in m_items )
					{
						if ( !Object.ReferenceEquals( item, null ) )
						{
							( (IChangeTracking)item ).AcceptChanges();
						}
					}
				}
			}
		}


		/// <summary>
		/// Called before inserting an item in the collection.
		/// </summary>
		/// <param name="item">The item being inserted.</param>
		/// <param name="index">The index where the item will be inserted.</param>
		protected virtual void OnInsert( T item, Int32 index )
		{
			if ( ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
			{
				INotifyPropertyChanged observable = (INotifyPropertyChanged)item;

				if ( observable != null )
				{
					observable.PropertyChanged += this.OnItemPropertyChanged;
				}
			}
		}


		/// <summary>
		/// Called before removing an item from the collection.
		/// </summary>
		/// <param name="item">The item being removed.</param>
		protected virtual void OnRemove( T item )
		{
			if ( ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
			{
				INotifyPropertyChanged observable = (INotifyPropertyChanged)item;

				if ( observable != null )
				{
					observable.PropertyChanged -= this.OnItemPropertyChanged;
				}
			}
		}


		/// <summary>
		/// Called before moving an item in the collection.
		/// </summary>
		/// <param name="item">The item being moved.</param>
		/// <param name="newIndex">The index where the item will be moved to.</param>
		protected virtual void OnMove( T item, Int32 newIndex )
		{
		}


		private void RaiseChanged()
		{
			if ( !this.IsInitializing )
			{
				this.IsChanged = true;
			}
		}


		Boolean ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable)m_items ).GetEnumerator();
		}


		Boolean ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}


		Object ICollection.SyncRoot
		{
			get
			{
				return ( (ICollection)m_items ).SyncRoot;
			}
		}


		void ICollection.CopyTo( Array array, Int32 index )
		{
			( (ICollection)m_items ).CopyTo( array, index );
		}


		Object IList.this[ Int32 index ]
		{
			get
			{
				return m_items[ index ];
			}
			set
			{
				this[ index ] = (T)value;
			}
		}


		Boolean IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}


		Boolean IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}


		Int32 IList.Add( Object item )
		{
			Int32 index = this.Count;

			this.Add( (T)item );
			
			return index;
		}


		Boolean IList.Contains( Object item )
		{
			return this.Contains( (T)item );
		}


		void IList.Insert( Int32 index, Object item )
		{
			this.Insert( index, (T)item );
		}


		Int32 IList.IndexOf( Object item )
		{
			return this.IndexOf( (T)item );
		}


		void IList.Remove( Object item )
		{
			this.Remove( (T)item );
		}


		private void RaiseCollectionChanged( NotifyCollectionChangedAction action, T item, Int32 index )
		{
			var handler = this.CollectionChanged;
			NotifyCollectionChangedEventArgs args = null;

			if ( handler != null )
			{
				args = new NotifyCollectionChangedEventArgs( action, item, index );

				handler( this, args );
			}

			if ( m_collectionChanges != null )
			{
				if ( args == null )
				{
					args = new NotifyCollectionChangedEventArgs( action, item, index );
				}

				m_collectionChanges.OnNext( args );
			}
		}


		private void RaiseCollectionChanged( NotifyCollectionChangedAction action, T oldItem, T newItem, Int32 index )
		{
			var handler = this.CollectionChanged;
			NotifyCollectionChangedEventArgs args = null;

			if ( handler != null )
			{
				args = new NotifyCollectionChangedEventArgs( action, newItem, oldItem, index );

				handler( this, args );
			}

			if ( m_collectionChanges != null )
			{
				if ( args == null )
				{
					args = new NotifyCollectionChangedEventArgs( action, newItem, oldItem, index );
				}

				m_collectionChanges.OnNext( args );
			}
		}


		private void RaiseCollectionChanged( NotifyCollectionChangedAction action, T oldItem, Int32 index, Int32 oldIndex )
		{
			var handler = this.CollectionChanged;
			NotifyCollectionChangedEventArgs args = null;

			if ( handler != null )
			{
				args = new NotifyCollectionChangedEventArgs( action, oldItem, index, oldIndex );

				handler( this, args );
			}

			if ( m_collectionChanges != null )
			{
				if ( args == null )
				{
					args = new NotifyCollectionChangedEventArgs( action, oldItem, index, oldIndex );
				}

				m_collectionChanges.OnNext( args );
			}
		}


		private void RaiseCollectionReset()
		{
			var handler = this.CollectionChanged;
			NotifyCollectionChangedEventArgs args = null;

			if ( handler != null )
			{
				args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );

				handler( this, args );
			}

			if ( m_collectionChanges != null )
			{
				if ( args == null )
				{
					args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
				}

				m_collectionChanges.OnNext( args );
			}
		}


		private void OnItemPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			if ( m_itemsChanges  != null )
			{
				m_itemsChanges.OnNext( new RxPropertyChange<T>( (T)sender, e.PropertyName ) );
			}

			if ( !this.IsChanged && ReflectionTraits.Assignable<IChangeTracking, T>.Value )
			{
				IChangeTracking item = (IChangeTracking)sender;

				if ( item.IsChanged )
				{
					this.IsChanged = true;
				}
			}
		}


		private readonly List<T> m_items;
		private Subject<RxPropertyChange<T>> m_itemsChanges;
		private Subject<NotifyCollectionChangedEventArgs> m_collectionChanges;

		private const String INDEXER_PROPERTY_NAME = "Item[]"; // This must agree with Binding.IndexerName. It is declared separately here so as to avoid a dependency on PresentationFramework.dll.
		private const String COUNT_PROPERTY_NAME = "Count";
		private const String FIRST_ITEM_PROPERTY_NAME = "First";
		private const String LAST_ITEM_PROPERTY_NAME = "Last";
	}
}
