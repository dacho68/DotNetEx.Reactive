using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

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


		public Boolean IsInitializing
		{
			get
			{
				return m_init > 0;
			}
		}


		public virtual void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				// Propagate the accept changes to nested items
				FieldInfo[] trackableFields;

				lock ( s_trackableFields )
				{
					Type thisType = this.GetType();

					if ( !s_trackableFields.TryGetValue( thisType, out trackableFields ) )
					{
						trackableFields = thisType
							.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
							.Where( x => typeof( IChangeTracking ).IsAssignableFrom( x.FieldType ) )
							.ToArray();

						s_trackableFields.Add( thisType, trackableFields );
					}
				}
				
				foreach ( var field in trackableFields )
				{
					IChangeTracking value = (IChangeTracking)field.GetValue( this );

					if ( value != null && value.IsChanged )
					{
						value.AcceptChanges();
					}
				}
			}
		}


		public void BeginInit()
		{
			if ( ++m_init == 1 )
			{
				this.OnBeginInit();
				this.RaisePropertyChanged( "IsInitializing" );

				// Propagate the begin init to nested items
				FieldInfo[] initializableFields = GetInitializableFields();

				foreach ( var field in initializableFields )
				{
					ISupportInitialize value = (ISupportInitialize)field.GetValue( this );

					if ( value != null )
					{
						value.BeginInit();
					}
				}
			}
		}


		public void EndInit()
		{
			if ( m_init == 0 )
			{
				throw new InvalidOperationException( "BeginInit must be callled before EndInit." );
			}

			if ( --m_init == 0 )
			{
				this.OnEndInit();
				this.RaisePropertyChanged( "IsInitializing" );

				// Propagate the begin init to nested items
				FieldInfo[] initializableFields = GetInitializableFields();

				foreach ( var field in initializableFields )
				{
					ISupportInitialize value = (ISupportInitialize)field.GetValue( this );

					if ( value != null )
					{
						value.EndInit();
					}
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

				if ( !this.IsInitializing )
				{
					this.IsChanged = true;
				}

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


		protected virtual void OnBeginInit()
		{
		}


		protected virtual void OnEndInit()
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
			if ( !this.IsChanged && !this.IsInitializing )
			{
				if ( e.PropertyName == "IsChanged" )
				{
					this.IsChanged = true;
				}
			}
		}


		[OnDeserializing]
		private void OnDeserializing( StreamingContext context )
		{
			this.BeginInit();
		}


		[OnDeserialized]
		private void OnDeserialized( StreamingContext context )
		{
			this.EndInit();
		}


		private void Attach<T>( T item )
			where T : IChangeTracking
		{
			if ( item != null )
			{
				if ( this.IsInitializing )
				{
					var initializable = item as ISupportInitialize;

					if ( initializable != null )
					{
						initializable.BeginInit();
					}
				}

				var notifiable = item as INotifyPropertyChanged;

				if ( notifiable != null )
				{
					notifiable.PropertyChanged += OnItemPropertyChanged;
				}
			}
		}


		private void Detach<T>( T item )
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


		private FieldInfo[] GetInitializableFields()
		{
			FieldInfo[] initializableFields;

			lock ( s_initializableFields )
			{
				Type thisType = this.GetType();

				if ( !s_initializableFields.TryGetValue( thisType, out initializableFields ) )
				{
					initializableFields = thisType
						.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
						.Where( x => typeof( ISupportInitialize ).IsAssignableFrom( x.FieldType ) )
						.ToArray();

					s_initializableFields.Add( thisType, initializableFields );
				}
			}

			return initializableFields;
		}


		[NonSerialized]
		private Subject<PropertyChangedEventArgs> m_propertyChanges;
		[NonSerialized]
		private Int32 m_init = 0; 
		private Boolean m_isChanged = false;


		private static readonly Dictionary<Type, FieldInfo[]> s_trackableFields = new Dictionary<Type, FieldInfo[]>();
		private static readonly Dictionary<Type, FieldInfo[]> s_initializableFields = new Dictionary<Type, FieldInfo[]>();
	}
}