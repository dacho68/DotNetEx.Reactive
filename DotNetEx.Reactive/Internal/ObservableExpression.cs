using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;

namespace DotNetEx.Reactive
{
	internal sealed class ObservableExpression<T, TValue> : ExpressionVisitor, IObservable<TValue>
		where T : INotifyPropertyChanged
	{
		private sealed class Binding
		{
			public Binding( Binding parent )
			{
				m_parent = parent;
			}


			public event EventHandler Changed;


			public Dictionary<String, Binding> Bindings
			{
				get
				{
					return m_bindings;
				}
			}


			public void UpdateSource( Object source )
			{
				if ( m_source != source )
				{
					if ( m_source != null )
					{
						var notifiable = m_source as INotifyPropertyChanged;
						m_source = null;

						if ( notifiable != null )
						{
							notifiable.PropertyChanged -= this.OnSourcePropertyChanged;
						}
					}
				}

				try
				{
					m_pendingRaise = true;

					if ( source != null )
					{
						m_source = source;
						var notifiable = source as INotifyPropertyChanged;

						if ( notifiable != null )
						{
							notifiable.PropertyChanged += this.OnSourcePropertyChanged;
						}

						foreach ( var binding in m_bindings )
						{
							binding.Value.UpdateSource( this.GetValue( binding.Key ) );
						}
					}
				}
				finally
				{
					m_pendingRaise = false;
				}

				this.RaiseChanged();
			}


			public Object GetValue( String propertyName )
			{
				if ( m_source != null )
				{
					PropertyInfo property;

					if ( !m_bindingProperties.TryGetValue( propertyName, out property ) )
					{
						property = m_source.GetType().GetProperty( propertyName, BindingFlags.Instance | BindingFlags.Public );
						m_bindingProperties.Add( propertyName, property );
					}

					return property.GetValue( m_source );
				}

				return null;
			}


			public override String ToString()
			{
				return String.Join( ", ", m_bindings.Keys );
			}


			private void OnSourcePropertyChanged( Object sender, PropertyChangedEventArgs e )
			{
				if ( e.PropertyName == String.Empty )
				{
					foreach ( var binding in m_bindings.Values )
					{
						binding.UpdateSource( this.GetValue( e.PropertyName ) );
					}
				}
				else
				{
					Binding binding;

					if ( m_bindings.TryGetValue( e.PropertyName, out binding ) )
					{
						binding.UpdateSource( this.GetValue( e.PropertyName ) );
					}
				}
			}


			private void RaiseChanged()
			{
				if ( !m_pendingRaise )
				{
					var changedHandler = this.Changed;

					if ( changedHandler != null )
					{
						changedHandler( this, EventArgs.Empty );
					}

					if ( m_parent != null )
					{
						m_parent.RaiseChanged();
					}
				}
			}


			private Object m_source;
			private Boolean m_pendingRaise = false;
			private readonly Binding m_parent;
			private readonly Dictionary<String, Binding> m_bindings = new Dictionary<String, Binding>( StringComparer.Ordinal );
			private readonly Dictionary<String, PropertyInfo> m_bindingProperties = new Dictionary<String, PropertyInfo>();
		}


		public ObservableExpression( T source, Expression<Func<T, TValue>> expression )
		{
			m_source = source;
			m_expression = expression.Compile();

			m_binding = new Binding( null );
			m_binding.Changed += OnBindingChanged;

			this.Visit( expression );
		}


		public IDisposable Subscribe( IObserver<TValue> observer )
		{
			var key = Guid.NewGuid();

			lock ( m_lock )
			{
				if ( m_observers.Count == 0 )
				{
					m_binding.UpdateSource( m_source );
				}

				m_observers.Add( key, observer );
				observer.OnNext( m_currentValue );
			}

			return Disposable.Create( () =>
			{
				lock ( m_lock )
				{
					if ( m_observers.Remove( key ) && m_observers.Count == 0 )
					{
						m_binding.UpdateSource( null );
					}
				}
			} );
		}


		protected override Expression VisitMember( MemberExpression node )
		{
			Boolean supported = false;

			Expression parentNode = node.Expression;

			while ( node != parentNode )
			{
				if ( parentNode.NodeType == ExpressionType.Parameter && parentNode.Type == typeof( T ) )
				{
					supported = true;
				}
				else
				{
					if ( parentNode.NodeType == ExpressionType.MemberAccess )
					{
						var memberExpression = (MemberExpression)parentNode;

						if ( memberExpression.Member is PropertyInfo && typeof( INotifyPropertyChanged ).IsAssignableFrom( memberExpression.Member.DeclaringType ) )
						{
							parentNode = memberExpression.Expression;

							continue;
						}
					}
				}

				break;

			}

			// Only nested properties are supported at the moment
			if ( supported )
			{
				String[] propertyNames = node.ToString().Split( '.' ).Skip( 1 ).ToArray();
				Binding parentBinding = m_binding;

				foreach ( var propertyName in propertyNames )
				{
					Binding currentBinding;

					if ( !parentBinding.Bindings.TryGetValue( propertyName, out currentBinding ) )
					{
						currentBinding = new Binding( parentBinding );
						parentBinding.Bindings.Add( propertyName, currentBinding );
					}

					parentBinding = currentBinding;
				}
			}

			return base.VisitMember( node );
		}


		private void OnBindingChanged( Object sender, EventArgs e )
		{
			lock ( m_lock )
			{
				if ( m_observers.Count > 0 )
				{
					TValue nextValue = m_expression( m_source );

					if ( !EqualityComparer<TValue>.Default.Equals( m_currentValue, nextValue ) )
					{
						m_currentValue = nextValue;

						foreach ( var observer in m_observers.Values )
						{
							observer.OnNext( nextValue );
						}
					}
				}
			}
		}


		private readonly Object m_lock = new Object();
		private readonly Func<T, TValue> m_expression;
		private readonly T m_source;
		private readonly Binding m_binding;
		private readonly Dictionary<Guid, IObserver<TValue>> m_observers = new Dictionary<Guid, IObserver<TValue>>();
		private TValue m_currentValue;
	}
}
