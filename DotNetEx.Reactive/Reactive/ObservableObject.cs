using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetEx.Reactive
{
	[Serializable]
	public abstract class ObservableObject : IObservableObject
	{
		protected ObservableObject()
		{
		}


		[field:NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;


		public IObservable<PropertyChangedEventArgs> PropertyChanges
		{
			get
			{
				if ( m_propertyChanges == null )
				{
					m_propertyChanges = new Subject<PropertyChangedEventArgs>();
				}

				return m_propertyChanges;
			}
		}


		public Boolean IsChanged
		{
			get
			{
				return m_isChanged;
			}
			protected set
			{
				if ( m_isChanged != value )
				{
					m_isChanged = value;
					this.RaisePropertyChanged();
				}
			}
		}


		public virtual void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				// Propagate the accept changes to nested contained items
				var fields = this.GetType()
					.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
					.Where( x => typeof( IChangeTracking ).IsAssignableFrom( x.FieldType ) );

				foreach ( var field in fields )
				{
					IChangeTracking value = (IChangeTracking)field.GetValue( this );

					if ( value != null )
					{
						value.AcceptChanges();
					}
				}
			}
		}


		protected void Attach<T>( T item )
			where T : IChangeTracking
		{
			if ( item != null )
			{
				var notifiable = item as INotifyPropertyChanged;

				if ( notifiable != null )
				{
					notifiable.PropertyChanged += OnItemPropertyChanged;
				}
			}
		}


		protected void Detach<T>( T item )
			where T : IChangeTracking
		{
			if ( item != null )
			{
				var notifiable = item as INotifyPropertyChanged;

				if ( notifiable != null )
				{
					notifiable.PropertyChanged -= OnItemPropertyChanged;
				}
			}
		}


		protected Boolean SetValue<T>( ref T value, T newValue, [CallerMemberName] String propertyName = null )
		{
			if ( !EqualityComparer<T>.Default.Equals( value, newValue ) )
			{
				if ( typeof( IChangeTracking ).IsAssignableFrom( typeof( T ) ) )
				{
					this.Detach( (IChangeTracking)value );
					this.Attach( (IChangeTracking)newValue );
				}

				this.OnPropertyChanging( propertyName );

				value = newValue;

				this.OnPropertyChanged( propertyName );
				this.RaisePropertyChanged( propertyName );
				this.IsChanged = true;

				return true;
			}

			return false;
		}


		protected virtual void OnPropertyChanging( String propertyName )
		{
		}


		protected virtual void OnPropertyChanged( String propertyName )
		{
		}


		protected void RaisePropertyChanged( [CallerMemberName] String propertyName = null )
		{
			var handler = this.PropertyChanged;

			if ( handler != null || m_propertyChanges != null )
			{
				var args = new PropertyChangedEventArgs( propertyName );

				if ( handler != null )
				{
					handler( this, args );
				}

				if ( m_propertyChanges != null )
				{
					m_propertyChanges.OnNext( args );
				}

				foreach ( var referencePropertyName in ReferencesAttribute.Get( this.GetType(), propertyName ) )
				{
					this.RaisePropertyChanged( referencePropertyName );
				}
			}
		}


		private void OnItemPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			if ( !this.IsChanged )
			{
				if ( e.PropertyName == "IsChanged" )
				{
					this.IsChanged = true;
				}
			}
		}


		[NonSerialized]
		private Subject<PropertyChangedEventArgs> m_propertyChanges;
		private Boolean m_isChanged = false;
	}
}