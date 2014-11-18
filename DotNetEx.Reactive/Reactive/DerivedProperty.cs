using System;
using System.ComponentModel;
using DotNetEx.Reactive.Internal;
using System.Reactive.Linq;

namespace DotNetEx.Reactive
{
	public sealed class DerivedProperty<T> : IDisposable
	{
		public DerivedProperty( IObservable<T> source ) :
			this( source, default( T ) )
		{
		}


		public DerivedProperty( IObservable<T> source, T initialValue )
		{
			Check.NotNull( source, "source" );

			this.Value = initialValue;

			m_subscription = source.Subscribe( this.OnChange );

			this.Changed = source.DistinctUntilChanged();
		}


		/// <summary>
		/// Gets the value.
		/// </summary>
		public T Value { get; private set; }


		/// <summary>
		/// Gets the value changed observable.
		/// </summary>
		public IObservable<T> Changed { get; private set; }


		/// <summary>
		/// Unsubscribes the derived property from the property source.
		/// </summary>
		public void Dispose()
		{
			m_subscription.Dispose();
		}


		public override String ToString()
		{
			if ( Object.ReferenceEquals( this.Value, null ) )
			{
				return String.Empty;
			}

			return this.Value.ToString();
		}


		public static implicit operator T( DerivedProperty<T> property )
		{
			if ( property == null )
			{
				return default( T );
			}

			return property.Value;
		}


		private void OnChange( T value )
		{
			if ( !Object.Equals( this.Value, value ) )
			{
				this.Value = value;
			}
		}


		private IDisposable m_subscription;
	}
}