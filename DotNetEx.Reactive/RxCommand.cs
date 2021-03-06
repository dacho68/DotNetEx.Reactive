﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DotNetEx.Reactive
{
	/// <summary>
	/// Reactive ICommand implementation that allows synchronous and asynchronous execution. 
	/// Can execute handler is based on observable source.
	/// </summary>
	public sealed class RxCommand : ICommand, IDisposable, INotifyPropertyChanged
	{
		public RxCommand Create( Action<Object> handler, IObservable<Boolean> canExecute = null, Boolean initialState = true )
		{
			return new RxCommand( handler, canExecute ?? Observable.Return<Boolean>( true ), initialState );
		}


		public RxCommand CreateTask( Func<Object, Task> handler, IObservable<Boolean> canExecute = null, Boolean initialState = true )
		{
			return new RxCommand( handler, canExecute ?? Observable.Return<Boolean>( true ), initialState );
		}


		public RxCommand( Action<Object> handler ) :
			this( handler, Observable.Return<Boolean>( true ) )
		{
		}


		public RxCommand( Action<Object> handler, IObservable<Boolean> canExecute ) :
			this( handler, canExecute, true )
		{
		}


		public RxCommand( Action<Object> handler, IObservable<Boolean> canExecute, Boolean initialState )
		{
			Check.NotNull( handler, "handler" );
			Check.NotNull( canExecute, "canExecute" );

			canExecute = canExecute.Catch<Boolean, Exception>( ex =>
			{
				RxApp.PublishError( ex );

				return Observable.Empty<Boolean>().Publish( false );
			} );

			m_handler = handler;
			m_isAsync = false;
			m_canExecuteLatest = initialState;
			m_dispatcher = SynchronizationContext.Current;
			m_canExecuteSubscription = canExecute.Subscribe( this.ChangeCanExecute );
		}


		public RxCommand( Func<Object, Task> handler ) :
			this( handler, Observable.Return<Boolean>( true ) )
		{
		}


		public RxCommand( Func<Object, Task> handler, IObservable<Boolean> canExecute ) :
			this( handler, canExecute, true )
		{
		}


		public RxCommand( Func<Object, Task> handler, IObservable<Boolean> canExecute, Boolean initialState )
		{
			Check.NotNull( handler, "handler" );
			Check.NotNull( canExecute, "canExecute" );

			canExecute = canExecute.Catch<Boolean, Exception>( ex =>
			{
				RxApp.PublishError( ex );

				return Observable.Empty<Boolean>().Publish( false );
			} );

			m_handler = handler;
			m_isAsync = true;
			m_canExecuteLatest = initialState;
			m_dispatcher = SynchronizationContext.Current;
			m_canExecuteSubscription = canExecute.Subscribe( this.ChangeCanExecute );
		}


		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;


		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Gets a value indicating whether the current instance is executing something.
		/// </summary>
		public Boolean IsBusy
		{
			get
			{
				return m_disabled > 0;
			}
		}


		public Boolean IsExecuting
		{
			get
			{
				return m_executing;
			}
			private set
			{
				if ( m_executing != value )
				{
					m_executing = value;
					this.RaisePropertyChanged( "IsExecuting" );
				}
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance is disabled, e.g. cannot execute.
		/// </summary>
		public Boolean IsDisabled
		{
			get
			{
				return !m_canExecuteLatest || ( m_disabled > 0 );
			}
		}


		public void Dispose()
		{
			lock ( m_lock )
			{
				if ( m_canExecuteSubscription != null )
				{
					this.Disable();

					m_canExecuteSubscription.Dispose();
					m_canExecuteSubscription = null;
				}
			}
		}


		/// <summary>
		/// Disables this command until disposed is called. When called multiple times, the command will be enabled again when 
		/// the all calls have called the dispose method on the object they have received.
		/// </summary>
		public IDisposable Disable()
		{
			Boolean prevDisabled;
			Boolean prevBusy;

			lock ( m_lock )
			{
				prevDisabled = this.IsDisabled;
				prevBusy = this.IsBusy;

				++m_disabled;
			}

			if ( prevDisabled != this.IsDisabled )
			{
				this.RaisePropertyChanged( "IsDisabled" );
				this.RaiseCanExecuteChanged();
			}

			if ( prevBusy != this.IsBusy )
			{
				this.RaisePropertyChanged( "IsBusy" );
			}

			return Disposable.Create( this.Enable );
		}


		/// <summary>
		/// Determines whether this instance can execute.
		/// </summary>
		public Boolean CanExecute()
		{
			return !this.IsDisabled;
		}


		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		public Boolean CanExecute( Object parameter )
		{
			return !this.IsDisabled;
		}


		/// <summary>
		/// Executes the command.
		/// </summary>
		public void Execute()
		{
			this.Execute( null );
		}


		/// <summary>
		/// Executes the command.
		/// </summary>
		public async void Execute( Object parameter )
		{
			IDisposable protector = null;

			lock ( m_lock )
			{
				if ( !this.IsDisabled )
				{
					protector = this.Disable();
				}
			}

			if ( protector != null )
			{
				this.IsExecuting = true;
				this.RaisePropertyChanged( "IsDisabled" );
				this.RaisePropertyChanged( "IsBusy" );
				this.RaiseCanExecuteChanged();

				try
				{
					if ( m_isAsync )
					{
						await ( (Func<Object, Task>)m_handler )( parameter );
					}
					else
					{
						( (Action<Object>)m_handler )( parameter );
					}
				}
				catch ( Exception ex )
				{
					RxApp.PublishError( ex );
				}
				finally
				{
					this.IsExecuting = false;
					protector.Dispose();
				}
			}
		}


		private void Enable()
		{
			Boolean prevDisabled;
			Boolean prevBusy;

			lock ( m_lock )
			{
				prevDisabled = this.IsDisabled;
				prevBusy = this.IsBusy;

				--m_disabled;
			}

			if ( prevDisabled != this.IsDisabled )
			{
				this.RaisePropertyChanged( "IsDisabled" );
				this.RaiseCanExecuteChanged();
			}

			if ( prevBusy != this.IsBusy )
			{
				this.RaisePropertyChanged( "IsBusy" );
			}
		}


		private void ChangeCanExecute( Boolean value )
		{
			Boolean changed = false;

			lock ( m_lock )
			{
				if ( m_canExecuteLatest != value )
				{
					m_canExecuteLatest = value;
					changed = true;
				}
			}

			if ( changed )
			{
				this.RaisePropertyChanged( "IsDisabled" );
				this.RaiseCanExecuteChanged();
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


		private void RaiseCanExecuteChanged()
		{
			var e = this.CanExecuteChanged;

			if ( e != null )
			{
				if ( ( m_dispatcher != null ) && ( SynchronizationContext.Current != m_dispatcher ) )
				{
					m_dispatcher.Send( _ => e( this, EventArgs.Empty ), null );
				}
				else
				{
					e( this, EventArgs.Empty );
				}
			}
		}


		private readonly Object m_lock = new Object();
		private readonly Object m_handler;
		private readonly Boolean m_isAsync;
		private Int32 m_disabled = 0;
		private Boolean m_canExecuteLatest;
		private IDisposable m_canExecuteSubscription;
		private SynchronizationContext m_dispatcher;
		private Boolean m_executing = false;
	}
}