using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public sealed class RxCommandSource : DynamicObject, IDisposable, INotifyPropertyChanged
	{
		public RxCommandSource()
		{
			m_dispatcher = SynchronizationContext.Current;
		}


		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Gets a value indicating whether this instance is busy.
		/// </summary>
		public Boolean IsBusy
		{
			get
			{
				return m_commands.Values.Any( x => x.IsBusy );
			}
		}


		/// <summary>
		/// Marks this instance as busy. While busy, none of the commands is allowed to be executed.
		/// </summary>
		public IDisposable MarkAsBusy()
		{
			Check.Invariant( m_commands.Count > 0, "The command source doesn't contain any commands." );

			// Disabling any of the commands will disable all of them
			return m_commands.Values.First().Disable();
		}


		/// <summary>
		/// Binds the provided command source to this instance making them mutually exclusive when one of them is busy.
		/// </summary>
		public IDisposable Bind( RxCommandSource other )
		{
			Check.NotNull( other, "other" );

			RxCommand dummy	= new RxCommand( _ => { } );
			String dummyKey	= Guid.NewGuid().ToString();

			// Adding the same command in both command sources acts as a bridge for the IsBusy property.
			other.Add( dummyKey, dummy );
			this.Add( dummyKey, dummy );

			return Disposable.Create( () =>
			{
				other.Remove( dummyKey );
				this.Remove( dummyKey );
			} );
		}


		public void Dispose()
		{
			m_dispatcher = null;

			foreach ( var command in m_commands )
			{
				command.Value.Dispose();
			}

			m_commands.Clear();
		}


		/// <summary>
		/// Executes the provided task asynchronously. While executing the task, this instance is marked as busy and no commands will be allowed to be executed.
		/// </summary>
		public async Task ExecuteAsync( Func<Task> task )
		{
			Check.NotNull( task, "task" );

			using ( this.MarkAsBusy() )
			{
				try
				{
					await task();
				}
				catch ( Exception ex )
				{
					RxApp.PublishError( ex );
				}
			}
		}


		/// <summary>
		/// Executes the specified command.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		/// <returns>Returns True if the command has been found and was allowed the execution.</returns>
		public Boolean Execute( String commandName )
		{
			return this.Execute( commandName, null );
		}


		/// <summary>
		/// Executes the specified command.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns>Returns True if the command has been found and was allowed the execution.</returns>
		public Boolean Execute( String commandName, Object parameter )
		{
			RxCommand command;

			if ( m_commands.TryGetValue( commandName, out command ) && command.CanExecute( parameter ) )
			{
				command.Execute( parameter );

				return true;
			}

			return false;
		}


		/// <summary>
		/// Gets the specified command.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		/// <exception cref="System.IndexOutOfRangeException">When the command is not found.</exception>
		public RxCommand Get( String commandName )
		{
			Check.NotEmpty( commandName, "commandName" );

			RxCommand command;

			if ( !m_commands.TryGetValue( commandName, out command ) )
			{
				throw new IndexOutOfRangeException( String.Format( "Command with name {0} doesn't exist.", commandName ) );
			}

			return command;
		}


		/// <summary>
		/// Finds the specified command. If the command doesn't exist, the method will return null.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		public RxCommand Find( String commandName )
		{
			Check.NotEmpty( commandName, "commandName" );

			RxCommand command;
			m_commands.TryGetValue( commandName, out command );

			return command;
		}


		public RxCommand Add( String commandName, Action<Object> handler )
		{
			return this.Add( commandName, handler, Observable.Return( true ) );
		}


		public RxCommand AddTask( String commandName, Func<Object, Task> handler )
		{
			return this.AddTask( commandName, handler, Observable.Return( true ) );
		}


		public RxCommand Add( String commandName, Action<Object> handler, IObservable<Boolean> canExecute )
		{
			Check.NotEmpty( commandName, "commandName" );
			Check.NotNull( handler, "handler" );

			RxCommand command;

			if ( !m_commands.TryGetValue( commandName, out command ) )
			{
				command = new RxCommand( handler, canExecute );
				command.PropertyChanged += OnCommandPropertyChanged;

				m_commands.Add( commandName, command );
			}
			else
			{
				throw new InvalidOperationException( String.Format( "Command with name {0} has already been added.", commandName ) );
			}

			this.RaisePropertyChanged( commandName );

			return command;
		}


		public RxCommand AddTask( String commandName, Func<Object, Task> handler, IObservable<Boolean> canExecute )
		{
			Check.NotEmpty( commandName, "commandName" );
			Check.NotNull( handler, "handler" );

			RxCommand command;

			if ( !m_commands.TryGetValue( commandName, out command ) )
			{
				command = new RxCommand( handler, canExecute );
				command.PropertyChanged += OnCommandPropertyChanged;

				m_commands.Add( commandName, command );
			}
			else
			{
				throw new InvalidOperationException( String.Format( "Command with name {0} has already been added.", commandName ) );
			}

			this.RaisePropertyChanged( commandName );

			return command;
		}


		public void Add( String commandName, RxCommand command )
		{
			Check.NotEmpty( commandName, "commandName" );
			Check.NotNull( command, "command" );

			if ( !m_commands.ContainsKey( commandName ) )
			{
				command.PropertyChanged += OnCommandPropertyChanged;

				m_commands.Add( commandName, command );
			}
			else
			{
				throw new InvalidOperationException( String.Format( "Command with name {0} has already been added.", commandName ) );
			}
		}


		/// <summary>
		/// Removes the specified command.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		/// <returns>Returns True if the command has been successfully removed or False when the command doesn't exist.</returns>
		public Boolean Remove( String commandName )
		{
			RxCommand command;

			if ( m_commands.TryGetValue( commandName, out command ) )
			{
				command.PropertyChanged -= OnCommandPropertyChanged;
				command.Dispose();
			}

			Boolean success = m_commands.Remove( commandName );

			if ( success )
			{
				this.RaisePropertyChanged( commandName );
			}

			return success;
		}


		public override IEnumerable<String> GetDynamicMemberNames()
		{
			return m_commands.Keys;
		}


		public override Boolean TryGetMember( GetMemberBinder binder, out Object result )
		{
			RxCommand command;

			Boolean success = m_commands.TryGetValue( binder.Name, out command );
			result = command;

			return success;
		}


		private void OnCommandPropertyChanged( Object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsBusy" )
			{
				if ( ( m_pending == null ) && ( (RxCommand)sender ).IsBusy )
				{
					m_pending = (RxCommand)sender;

					foreach ( var command in m_commands.Values.Where( x => !Object.ReferenceEquals( x, m_pending ) ) )
					{
						m_pendingDisabled.Add( command.Disable() );
					}

					this.RaisePropertyChanged( "IsBusy" );
				}
				else if ( Object.ReferenceEquals( sender, m_pending ) && !m_pending.IsBusy )
				{
					m_pendingDisabled.ForEach( x => x.Dispose() );
					m_pendingDisabled.Clear();

					m_pending = null;
					this.RaisePropertyChanged( "IsBusy" );
				}
			}
		}


		private void RaisePropertyChanged( String propertyName )
		{
			var e = this.PropertyChanged;

			if ( e != null )
			{
				if ( ( m_dispatcher != null ) && ( SynchronizationContext.Current != m_dispatcher ) )
				{
					m_dispatcher.Send( _ => e( this, new PropertyChangedEventArgs( propertyName ) ), null );
				}
				else
				{
					e( this, new PropertyChangedEventArgs( propertyName ) );
				}
			}
		}


		private SynchronizationContext					m_dispatcher;
		private RxCommand								m_pending			= null;
		private readonly List<IDisposable>				m_pendingDisabled	= new List<IDisposable>();
		private readonly Dictionary<String, RxCommand>	m_commands			= new Dictionary<String, RxCommand>( StringComparer.OrdinalIgnoreCase );
	}
}
