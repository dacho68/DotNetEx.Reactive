using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public sealed class RxTimer : IDisposable
	{
		public RxTimer()
		{
		}


		public void Dispose()
		{
			lock ( m_handles )
			{
				foreach ( var handle in m_handles )
				{
					handle.Value.Dispose();
				}

				m_handles.Clear();
				m_disposed = true;
			}

			m_event.Dispose();
		}


		public void Schedule( String key, Action action, TimeSpan after )
		{
			lock ( m_handles )
			{
				this.CheckNotDisposed();

				IDisposable handle = null;

				if ( m_handles.TryGetValue( key, out handle ) )
				{
					handle.Dispose();
					m_handles.Remove( key );
				}

				m_handles.Add( key, RxApp.MainScheduler.Schedule( after, () => this.Execute( key, action ) ) );
			}
		}


		public void ScheduleАsync( String key, Func<Task> task, TimeSpan after )
		{
			lock ( m_handles )
			{
				this.CheckNotDisposed();

				IDisposable handle = null;

				if ( m_handles.TryGetValue( key, out handle ) )
				{
					handle.Dispose();
					m_handles.Remove( key );
				}

				m_handles.Add( key, RxApp.MainScheduler.Schedule( after, () => this.ExecuteAsync( key, task ) ) );
			}
		}


		private void Execute( String key, Action action )
		{
			if ( !m_event.Wait( 0 ) )
			{
				this.Schedule( key, action, TimeSpan.FromMilliseconds( 50 ) );
			}

			lock ( m_handles )
			{
				if ( m_disposed ) return;
			}

			try
			{
				action();
			}
			catch ( Exception ex )
			{
				RxApp.PublishError( ex );
			}
			finally
			{
				m_event.Set();
			}
		}


		private async void ExecuteAsync( String key, Func<Task> task )
		{
			if ( !m_event.Wait( 0 ) )
			{
				this.ScheduleАsync( key, task, TimeSpan.FromMilliseconds( 50 ) );
			}

			lock ( m_handles )
			{
				if ( m_disposed ) return;
			}

			try
			{
				await task();
			}
			catch ( Exception ex )
			{
				RxApp.PublishError( ex );
			}
			finally
			{
				m_event.Set();
			}
		}


		private void CheckNotDisposed()
		{
			if ( m_disposed )
			{
				throw new ObjectDisposedException( this.GetType().FullName );
			}
		}


		private ManualResetEventSlim m_event = new ManualResetEventSlim( true );
		private readonly Dictionary<String, IDisposable> m_handles = new Dictionary<String, IDisposable>( StringComparer.OrdinalIgnoreCase );
		private Boolean m_disposed = false;
	}
}