using System;
using System.IO;

namespace DotNetEx.Reactive
{
	/// <summary>
	/// Application cache for storing key value pairs
	/// </summary>
	public static class AppCache
	{
		public static void Start( AppCacheSettings settings )
		{
			Check.NotNull( settings, "settings" );

			Settings = settings;
			Current	= new AppDiskCache( settings.CacheLocation );
		}


		/// <summary>
		/// Returns the current application cache settings. Returns null if Start is not called.
		/// </summary>
		public static AppCacheSettings Settings { get; private set; }


		/// <summary>
		/// Returns the cache store for the currently logged windows user. Returns null if Start is not called.
		/// </summary>
		public static IAppCache Current { get; private set; }
	}
}
