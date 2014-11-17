using System;
using System.Reactive.Concurrency;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DotNetEx.Reactive
{
	public static class RxApp
	{
		static RxApp()
		{
			MainScheduler = DefaultScheduler.Instance;
		}


		/// <summary>
		/// Gets or sets the main scheduler.
		/// </summary>
		public static IScheduler MainScheduler { get; set; }


		/// <summary>
		/// Gets the errors observable.
		/// </summary>
		public static IObservable<Exception> Errors
		{
			get
			{
				return m_errors.ObserveOn( RxApp.MainScheduler );
			}
		}


		internal static void PublishError( Exception error )
		{
			if ( error != null )
			{
				m_errors.OnNext( error );
			}
		}


		private static readonly ISubject<Exception, Exception> m_errors = Subject.Synchronize( new Subject<Exception>() );
	}
}
