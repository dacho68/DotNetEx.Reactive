using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;

namespace DotNetEx.Reactive
{
	public class ObservableList<T> : ObservableObject, IObservableList<T>
	{
		public ObservableList()
		{
			m_items = new List<T>();
		}


		public ObservableList( IEnumerable<T> items )
		{
			Check.NotNull( items, "items" );

			m_items = new List<T>( items );
			m_items.ForEach( x => this.SetupItem( x, false, true ) );
		}


		public event NotifyCollectionChangedEventHandler CollectionChanged;


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


		public IObservable<RxPropertyChange<T>> ItemsChanges
		{
			get
			{
				if ( !s_childrenSupportNotifyPropertyChanged )
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


		public Int32 Count
		{
			get
			{
				return m_items.Count;
			}
		}


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
					m_items[ index ] = value;

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

			m_items.Add( item );

			this.SetupItem( item, false );
			this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
			this.RaisePropertyChanged( COUNT_PROPERTY_NAME );
			this.RaisePropertyChanged( INDEXER_PROPERTY_NAME );
			this.RaiseCollectionChanged( NotifyCollectionChangedAction.Add, item, itemIndex );
		}


		public void Clear()
		{
			if ( m_items.Count > 0 )
			{
				m_items.ForEach( x => this.SetupItem( x, true ) );
				m_items.Clear();

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
				m_items.ForEach( x => this.SetupItem( x, true ) );
				m_items.Clear();

				cleared = true;
			}

			m_items.AddRange( items );
			m_items.ForEach( x => this.SetupItem( x, false ) );

			if ( cleared || m_items.Count > 0 )
			{
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
			m_items.Insert( index, item );

			this.SetupItem( item, false );

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

			m_items.RemoveAt( index );

			this.SetupItem( item, true );

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
			if ( m_items.Count > 0 )
			{
				m_items.Sort( comparison );

				this.RaisePropertyChanged( FIRST_ITEM_PROPERTY_NAME );
				this.RaisePropertyChanged( LAST_ITEM_PROPERTY_NAME );
				this.RaiseCollectionReset();
			}
		}


		public override void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				if ( s_childrenSupportChangeTracking && m_items.Count > 0 )
				{
					foreach ( var item in m_items.Cast<IChangeTracking>() )
					{
						item.AcceptChanges();
					}
				}
			}
		}


		protected virtual void OnItemAdded( T item )
		{
		}


		protected virtual void OnItemRemoved( T item )
		{
		}


		protected override void OnBeginInit()
		{
			if ( s_childrenSupportInitialize )
			{
				foreach ( ISupportInitialize item in m_items )
				{
					if ( item != null )
					{
						item.BeginInit();
					}
				}
			}
			
			base.OnBeginInit();
		}


		protected override void OnEndInit()
		{
			if ( s_childrenSupportInitialize )
			{
				foreach ( ISupportInitialize item in m_items )
				{
					if ( item != null )
					{
						item.EndInit();
					}
				}
			}
			
			base.OnEndInit();
		}


		private void SetupItem( T item, Boolean remove, Boolean constructor = false )
		{
			if ( s_childrenSupportNotifyPropertyChanged )
			{
				INotifyPropertyChanged observable = (INotifyPropertyChanged)item;

				if ( observable != null )
				{
					if ( remove )
					{
						observable.PropertyChanged -= this.OnItemPropertyChanged;
					}
					else
					{
						observable.PropertyChanged += this.OnItemPropertyChanged;
					}
				}
			}

			if ( this.IsInitializing && s_childrenSupportInitialize )
			{
				ISupportInitialize initializable = (ISupportInitialize)item;

				if ( initializable != null )
				{
					initializable.BeginInit();
				}
			}

			if ( !constructor )
			{
				if ( remove )
				{
					this.OnItemRemoved( item );
				}
				else
				{
					this.OnItemAdded( item );
				}
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

			if ( !this.IsChanged && s_childrenSupportChangeTracking )
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

		private static readonly Boolean s_childrenSupportNotifyPropertyChanged = typeof( INotifyPropertyChanged ).IsAssignableFrom( typeof( T ) );
		private static readonly Boolean s_childrenSupportChangeTracking = typeof( IChangeTracking ).IsAssignableFrom( typeof( T ) );
		private static readonly Boolean s_childrenSupportInitialize = typeof( ISupportInitialize ).IsAssignableFrom( typeof( T ) );
	}
}
