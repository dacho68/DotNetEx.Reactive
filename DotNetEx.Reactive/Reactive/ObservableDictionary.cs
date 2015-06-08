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
			m_map = new Dictionary<TKey, TValue>( m_key.Comparer );
		}


		public ObservableDictionary( IEnumerable<TValue> items ) :
			base( items )
		{
			Check.NotNull( items, "items" );

			m_key = new T();
			m_map = new Dictionary<TKey, TValue>( m_key.Comparer );

			foreach ( var item in this )
			{
				m_map.Add( m_key.GetKey( item ), item );
			}
		}


		public IReadOnlyDictionary<TKey, TValue> Map
		{
			get
			{
				return m_map;
			}
		}


		public Boolean ContainsKey( TKey key )
		{
			return m_map.ContainsKey( key );
		}


		protected override void OnItemAdded( TValue item )
		{
			m_map[ m_key.GetKey( item ) ] = item;
		}


		protected override void OnItemRemoved( TValue item )
		{
			m_map.Remove( m_key.GetKey( item ) );
		}


		private readonly Dictionary<TKey, TValue> m_map;
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
