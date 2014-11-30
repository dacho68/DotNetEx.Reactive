using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public class ObservableHashSet<T> : ICollection<T>, ISet<T>,  INotifyCollectionChanged, INotifyPropertyChanged
	{
		public ObservableHashSet()
		{
			m_set = new HashSet<T>();
		}


		public ObservableHashSet( IEnumerable<T> collection )
		{
			m_set = new HashSet<T>( collection );
		}


		public ObservableHashSet( IEqualityComparer<T> comparer )
		{
			m_set = new HashSet<T>( comparer );
		}


		public ObservableHashSet( IEnumerable<T> collection, IEqualityComparer<T> comparer )
		{
			m_set = new HashSet<T>( collection, comparer );
		}


		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;


		public virtual event PropertyChangedEventHandler PropertyChanged;


		void ICollection<T>.Add( T item )
		{
			this.Add( item );
		}


		public void Clear()
		{
			if ( m_set.Count > 0 )
			{
				m_set.Clear();
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}


		public Boolean Contains( T item )
		{
			return m_set.Contains( item );
		}


		public void CopyTo( T[] array, Int32 arrayIndex )
		{
			m_set.CopyTo( array, arrayIndex );
		}


		public Int32 Count
		{
			get
			{
				return m_set.Count;
			}
		}


		public Boolean IsReadOnly
		{
			get
			{
				return false;
			}
		}


		public Boolean Remove( T item )
		{
			if ( m_set.Remove( item ) )
			{
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item ) );

				return true;
			}

			return false;
		}


		public IEnumerator<T> GetEnumerator()
		{
			return m_set.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return m_set.GetEnumerator();
		}


		public Boolean Add( T item )
		{
			if ( m_set.Add( item ) )
			{
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item ) );

				return true;
			}

			return false;
		}


		public void ExceptWith( IEnumerable<T> other )
		{
			Check.NotNull( other, "other" );

			if ( this.Count == 0 )
			{
				return;
			}
			else if ( this == other )
			{
				this.Clear();
			}
			else
			{
				foreach ( var item in other )
				{
					this.Remove( item );
				}
			}
		}


		public void IntersectWith( IEnumerable<T> other )
		{
			if ( this.Count != 0 && other != this )
			{
				HashSet<T> current = m_set;
				HashSet<T> next = new HashSet<T>( other );

				next.IntersectWith( this );
				m_set = next;

				foreach ( var item in current )
				{
					if ( !next.Contains( item ) )
					{
						this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item ) );
					}
				}
			}
		}


		public Boolean IsProperSubsetOf( IEnumerable<T> other )
		{
			return m_set.IsProperSubsetOf( other );
		}


		public Boolean IsProperSupersetOf( IEnumerable<T> other )
		{
			return m_set.IsProperSupersetOf( other );
		}


		public Boolean IsSubsetOf( IEnumerable<T> other )
		{
			return m_set.IsSubsetOf( other );
		}


		public Boolean IsSupersetOf( IEnumerable<T> other )
		{
			return m_set.IsSupersetOf( other );
		}


		public Boolean Overlaps( IEnumerable<T> other )
		{
			return m_set.Overlaps( other );
		}


		public Boolean SetEquals( IEnumerable<T> other )
		{
			return m_set.SetEquals( other );
		}


		public void SymmetricExceptWith( IEnumerable<T> other )
		{
			if ( this.Count == 0 )
			{
				this.UnionWith( other );
			}
			else if ( other == this )
			{
				this.Clear();
			}
			else
			{
				HashSet<T> current = m_set;
				HashSet<T> next = new HashSet<T>( other );
				
				next.SymmetricExceptWith( this );
				m_set = next;

				foreach ( var item in current )
				{
					if ( !next.Contains( item ) )
					{
						this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item ) );
					}
				}
			}
		}


		public void UnionWith( IEnumerable<T> other )
		{
			foreach ( var item in other )
			{
				this.Add( item );
			}
		}


		protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			var handler = this.CollectionChanged;
		
			if ( handler != null )
			{
				handler( this, e );
			}

			this.PropertyChanged.Raise( this, "Count" );
		}


		private HashSet<T> m_set;
	}
}
