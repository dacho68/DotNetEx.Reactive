using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetEx.Reactive
{
	public class ObservableDictionary<T, TKey, TValue> : ObservableList<TValue>, IObservableDictionary<TKey, TValue>
		where T : ObservableDictionaryKey<TValue, TKey>, new()
	{
		public ObservableDictionary()
		{
			m_key = new T();
			m_indexes = new Dictionary<TKey, Int32>( m_key.Comparer );
		}


		public ObservableDictionary( Int32 capacity ) :
			base( capacity )
		{
			m_key = new T();
			m_indexes = new Dictionary<TKey, Int32>( m_key.Comparer );
		}


		public ObservableDictionary( IEnumerable<TValue> items ) :
			base( items )
		{
			Check.NotNull( items, "items" );

			m_key = new T();
			m_indexes = new Dictionary<TKey, Int32>( m_key.Comparer );

			for ( Int32 i = 0; i < this.Count; ++i )
			{
				m_indexes.Add( m_key.GetKey( this[ i ] ), i );
			}
		}


		/// <summary>
		///  Determines whether the collection contains the specified key. 
		/// </summary>
		public Boolean ContainsKey( TKey key )
		{
			return m_indexes.ContainsKey( key );
		}


		/// <summary>
		/// Removes the value located at the specified key.
		/// </summary>
		/// <returns>True if the key existed, False when it didn't</returns>
		public Boolean RemoveKey( TKey key )
		{
			Int32 index = this.IndexOfKey( key );

			if ( index > -1 )
			{
				this.RemoveAt( index );

				return true;
			}

			return false;
		}


		public Int32 IndexOfKey( TKey key )
		{
			Int32 index;

			if ( !m_indexes.TryGetValue( key, out index ) )
			{
				index = -1;
			}

			return index;
		}


		public Boolean TryGetValue( TKey key, out TValue value )
		{
			Int32 index = this.IndexOfKey( key );
			value = index > -1 ? this[ index ] : default( TValue );

			return index > -1;
		}


		/// <summary>
		/// Returns the value associated with the specified key.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">the key doesn't exist</exception>
		public TValue GetValue( TKey key )
		{
			Int32 index = this.IndexOfKey( key );

			if ( index < 0 )
			{
				throw new KeyNotFoundException();
			}

			return this[ index ];
		}


		public void AddOrUpdate( TValue value )
		{
			TKey key = m_key.GetKey( value );
			var index = this.IndexOfKey( key );

			if ( index > -1 )
			{
				this[ index ] = value;
			}
			else
			{
				this.Add( value );
			}
		}


		protected override void OnItemAdded( TValue item, Int32 index )
		{
			m_indexes[ m_key.GetKey( item ) ] = index;

			base.OnItemAdded( item, index );
		}


		protected override void OnItemRemoved( TValue item )
		{
			m_indexes.Remove( m_key.GetKey( item ) );

			base.OnItemRemoved( item );
		}


		protected override void OnItemMoved( TValue item, Int32 newIndex )
		{
			m_indexes[ m_key.GetKey( item ) ] = newIndex;

			base.OnItemMoved( item, newIndex );
		}


		private readonly Dictionary<TKey, Int32> m_indexes;
		private readonly ObservableDictionaryKey<TValue, TKey> m_key;
	}


	public sealed class ObservableDictionary<TKey, TValue> : ObservableDictionary<ObservableDictionary<TKey, TValue>.Key, TKey, TValue>
		where TValue : IObservableDictionaryKey<TKey>
	{
		public sealed class Key : ObservableDictionaryKey<TValue, TKey>
		{
			public override TKey GetKey( TValue item )
			{
				return item.GetKey();
			}
		}


		public ObservableDictionary() :
			base()
		{
		}


		public ObservableDictionary( IEnumerable<TValue> items ) :
			base( items )
		{
		}
	}
}
