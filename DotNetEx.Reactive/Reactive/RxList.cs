using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public class RxList<T> : ObservableCollection<T>
	{
		public RxList()
		{
			this.Initialize();
		}


		public RxList( IEnumerable<T> collection ) 
			: base( collection )
		{
			this.Initialize();
		}


		public void Reset( IEnumerable<T> collection )
		{
			Check.NotNull( collection, "collection" );

			Boolean raiseCollectionChangedEvents = this.RaiseCollectionChangedEvents;

			try
			{
				this.RaiseCollectionChangedEvents = false;

				this.Clear();
				
				foreach ( var item in collection )
				{
					this.Add( item );
				}
			}
			finally
			{
				this.RaiseCollectionChangedEvents = raiseCollectionChangedEvents;
			}

			if ( raiseCollectionChangedEvents  )
			{
				this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}


		public void AddRange( IEnumerable<T> collection )
		{
			Check.NotNull( collection, "collection" );

			Boolean raiseCollectionChangedEvents	= this.RaiseCollectionChangedEvents;
			List<T> added							= new List<T>();

			try
			{
				this.RaiseCollectionChangedEvents = false;

				foreach ( var item in collection )
				{
					this.Add( item );
					added.Add( item );
				}
			}
			finally
			{
				this.RaiseCollectionChangedEvents = raiseCollectionChangedEvents;
			}

			if ( raiseCollectionChangedEvents && added.Count > 0 )
			{
				this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, added ) );
			}
		}


		public Boolean RaiseCollectionChangedEvents
		{
			get
			{
				return m_raiseCollectionChangedEvents;
			}
			set
			{
				if ( m_raiseCollectionChangedEvents != value )
				{
					m_raiseCollectionChangedEvents = value;

					this.OnPropertyChanged( new PropertyChangedEventArgs( "RaiseCollectionChangedEvents" ) );
				}
			}
		}


		public IObservable<RxPropertyChange<T>> ChildrenPropertyChanges
		{
			get
			{
				return m_childrenPropertyChanges;
			}
		}


		public IObservable<NotifyCollectionChangedEventArgs> CollectionChanges
		{
			get
			{
				return m_collectionChanges;
			}
		}


		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					Subscribe( e.NewItems );
					break;

				case NotifyCollectionChangedAction.Remove:
					Unsubscribe( e.OldItems );
					break;

				case NotifyCollectionChangedAction.Replace:
					Unsubscribe( e.OldItems );
					Subscribe( e.NewItems );
					break;

				case NotifyCollectionChangedAction.Reset:
					Unsubscribe( e.OldItems );
					Subscribe( this );
					break;
			}

			if ( m_raiseCollectionChangedEvents )
			{
				base.OnCollectionChanged( e );

				m_collectionChanges.OnNext( e );
			}
		}


		protected virtual void OnChildrenPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			m_childrenPropertyChanges.OnNext( new RxPropertyChange<T>( (T)sender, e.PropertyName ) );
		}


		[OnDeserialized]
		private void OnDeserialized( StreamingContext context )
		{
			this.Initialize();
		}


		private void Initialize()
		{
			m_raiseCollectionChangedEvents = true;
			m_childrenPropertyChanges = new Subject<RxPropertyChange<T>>();
			m_collectionChanges	= new Subject<NotifyCollectionChangedEventArgs>();

			this.Subscribe( this );
		}


		private void Subscribe( IList items )
		{
			if ( items != null )
			{
				for ( Int32 i = 0; i < items.Count; ++i )
				{
					INotifyPropertyChanged notify = items[ i ] as INotifyPropertyChanged;

					if ( notify != null )
					{
						notify.PropertyChanged += OnChildrenPropertyChanged;
					}
				}
			}
		}


		private void Unsubscribe( IList items )
		{
			if ( items != null )
			{
				for ( Int32 i = 0; i < items.Count; ++i )
				{
					INotifyPropertyChanged notify = items[ i ] as INotifyPropertyChanged;

					if ( notify != null )
					{
						notify.PropertyChanged -= OnChildrenPropertyChanged;
					}
				}
			}
		}


		private Boolean										m_raiseCollectionChangedEvents;
		private Subject<RxPropertyChange<T>>				m_childrenPropertyChanges;
		private Subject<NotifyCollectionChangedEventArgs>	m_collectionChanges;
	}
}