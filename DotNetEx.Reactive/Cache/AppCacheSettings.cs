using System;
using System.Diagnostics.Contracts;
using System.IO;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public sealed class AppCacheSettings
	{
		public AppCacheSettings( String applicationName )
		{
			Check.NotEmpty( applicationName, "applicationName" );

			this.ApplicationName		= applicationName;
			this.InMemoryMaxSize		= 500;
			this.LocalMachineMaxSize	= 2048;
			this.CurrentUserMaxSize		= 2048;
			this.CacheLocation			= Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), applicationName, "Cache" );
		}


		/// <summary>
		/// Gets or sets the cache location.
		/// </summary>
		public String CacheLocation { get; set; }


		/// <summary>
		/// Returns the application name used as unique identifier on the local machine
		/// </summary>
		public String ApplicationName { get; private set; }


		/// <summary>
		/// Gets or sets the in memory max cache size in megabytes. Default is 500MB;
		/// </summary>
		public Int32 InMemoryMaxSize { get; set; }


		/// <summary>
		/// Gets or sets the local machine max cache size in megabytes. Default is 2GB.
		/// </summary>
		public Int32 LocalMachineMaxSize { get; set; }


		/// <summary>
		/// Gets or sets the current user max cache size in megabytes. Default is 2GB.
		/// </summary>
		public Int32 CurrentUserMaxSize { get; set; }
	}
}
