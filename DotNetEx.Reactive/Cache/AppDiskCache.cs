using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DotNetEx.Internal;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	internal sealed class AppDiskCache : IAppCache
	{
		public AppDiskCache( String directory )
		{
			Directory.CreateDirectory( directory );

			m_directory = directory;
		}


		public Boolean Contains( String key )
		{
			return this.Contains( key, null );
		}


		public Boolean Contains( String key, String regionName )
		{
			String cacheKey = this.ComputeCacheKey( key );

			return File.Exists( Path.Combine( m_directory, regionName ?? String.Empty, cacheKey ) );
		}


		public T Get<T>( String key )
		{
			return this.Get<T>( key, null );
		}


		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
		public T Get<T>( String key, String regionName )
		{
			String cacheKey = this.ComputeCacheKey( key );
			String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );
			
			using ( var access = this.CreateAccessGate( filePath ) )
			{
				access.WaitOne();

				if ( File.Exists( filePath ) )
				{
					try
					{
						using ( var stream = File.OpenRead( filePath ) )
						using ( var compression = new GZipStream( stream, CompressionMode.Decompress ) )
						{
							Byte[] time = new Byte[ 8 ];

							compression.Read( time, 0, time.Length );

							if ( DateTimeOffset.FromFileTime( BitConverter.ToInt64( time, 0 ) ) > DateTimeOffset.Now )
							{
								DataContractSerializer serializer = new DataContractSerializer( typeof( T ) );

								return (T)serializer.ReadObject( compression );
							}
						}
					}
					catch
					{
					}
				}
			}

			return default( T );
		}


		public Task<T> GetAsync<T>( String key )
		{
			return this.GetAsync<T>( key, null );
		}


		public async Task<T> GetAsync<T>( String key, String regionName )
		{
			String cacheKey = this.ComputeCacheKey( key );
			String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

			using ( var access = this.CreateAccessGate( filePath ) )
			{
				access.WaitOne();

				if ( File.Exists( filePath ) )
				{
					try
					{
						using ( var stream = File.OpenRead( filePath ) )
						using ( var compression = new GZipStream( stream, CompressionMode.Decompress ) )
						{
							Byte[] time = new Byte[ 8 ];

							compression.Read( time, 0, time.Length );

							if ( DateTimeOffset.FromFileTime( BitConverter.ToInt64( time, 0 ) ) > DateTimeOffset.Now )
							{
								return await Task.Run<T>( () =>
								{
									DataContractSerializer serializer = new DataContractSerializer( typeof( T ) );
								
									return (T)serializer.ReadObject( compression );
								} );
							}
						}
					}
					catch
					{
					}
				}
			}

			return default( T );
		}


		public T GetOrAdd<T>( String key, Func<Object, AppCacheItem<T>> factory )
		{
			return this.GetOrAdd<T>( key, null, factory );
		}


		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
		public T GetOrAdd<T>( String key, String regionName, Func<Object, AppCacheItem<T>> factory )
		{
			Check.NotNull( factory, "factory" );

			String cacheKey = this.ComputeCacheKey( key );
			String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

			using ( var access = this.CreateAccessGate( filePath ) )
			{
				access.WaitOne();

				if ( File.Exists( filePath ) )
				{
					try
					{
						using ( var stream = File.OpenRead( filePath ) )
						using ( var compression = new GZipStream( stream, CompressionMode.Decompress ) )
						{
							Byte[] time = new Byte[ 8 ];

							compression.Read( time, 0, time.Length );

							if ( DateTimeOffset.FromFileTime( BitConverter.ToInt64( time, 0 ) ) > DateTimeOffset.Now )
							{
								DataContractSerializer serializer = new DataContractSerializer( typeof( T ) );
							
								return (T)serializer.ReadObject( compression );
							}
						}
					}
					catch
					{
					}
				}

				// We need to create the value
				var newValue = factory( key );

				using ( var stream = File.OpenWrite( filePath ) )
				using ( var compression = new GZipStream( stream, CompressionLevel.Fastest ) )
				{
					stream.SetLength( 0 );
					compression.Write( BitConverter.GetBytes( newValue.Expires.ToFileTime() ), 0, sizeof( Int64 ) );

					DataContractSerializer formatter = new DataContractSerializer( typeof( T ) );

					formatter.WriteObject( compression, newValue.Value );
				}

				return newValue.Value;
			}
		}


		public Task<T> GetOrAddAsync<T>( String key, Func<Object, Task<AppCacheItem<T>>> factory )
		{
			return this.GetOrAddAsync<T>( key, null, factory );
		}


		public Task<T> GetOrAddAsync<T>( String key, String regionName, Func<Object, Task<AppCacheItem<T>>> factory )
		{
			Check.NotNull( factory, "factory" );
			
			return Task.Run<T>( async () =>
			{
				String cacheKey = this.ComputeCacheKey( key );
				String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

				using ( var access = this.CreateAccessGate( filePath ) )
				{
					access.WaitOne();

					if ( File.Exists( filePath ) )
					{
						try
						{
							using ( var stream = File.OpenRead( filePath ) )
							using ( var compression = new GZipStream( stream, CompressionMode.Decompress ) )
							{
								Byte[] time = new Byte[ 8 ];

								compression.Read( time, 0, time.Length );

								if ( DateTimeOffset.FromFileTime( BitConverter.ToInt64( time, 0 ) ) > DateTimeOffset.Now )
								{
									DataContractSerializer serializer = new DataContractSerializer( typeof( T ) );
								
									return (T)serializer.ReadObject( compression );
								}
							}
						}
						catch
						{
						}
					}

					// We need to create the value
					var newValue = await factory( key );

					using ( var stream = File.OpenWrite( filePath ) )
					using ( var compression = new GZipStream( stream, CompressionLevel.Fastest ) )
					{
						stream.SetLength( 0 );
						compression.Write( BitConverter.GetBytes( newValue.Expires.ToFileTime() ), 0, sizeof( Int64 ) );

						DataContractSerializer formatter = new DataContractSerializer( typeof( T ) );

						formatter.WriteObject( compression, newValue.Value );
					}

					return newValue.Value;
				}
			} );
		}


		public void Remove( String key )
		{
			this.Remove( key, null );
		}


		public void Remove( String key, String regionName )
		{
			String cacheKey = this.ComputeCacheKey( key );
			String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

			using ( var access = this.CreateAccessGate( filePath ) )
			{
				access.WaitOne();

				File.Delete( filePath );
			}
		}


		public Task RemoveAsync( String key )
		{
			return this.RemoveAsync( key, null );
		}


		public Task RemoveAsync( String key, String regionName )
		{
			Check.NotNull( key, "key" );
			
			return Task.Run( () =>
			{
				String cacheKey = this.ComputeCacheKey( key );
				String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

				using ( var access = this.CreateAccessGate( filePath ) )
				{
					access.WaitOne();

					File.Delete( filePath );
				}
			} );
		}


		public void AddOrUpdate<T>( String key, T value, DateTimeOffset expires )
		{
			this.AddOrUpdate( key, value, null, expires );
		}


		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
		public void AddOrUpdate<T>( String key, T value, String regionName, DateTimeOffset expires )
		{
			String cacheKey = this.ComputeCacheKey( key );
			String filePath = Path.Combine( m_directory, regionName ?? String.Empty, cacheKey );

			using ( var access = this.CreateAccessGate( filePath ) )
			{
				access.WaitOne();

				using ( var stream = File.OpenWrite( filePath ) )
				using ( var compression = new GZipStream( stream, CompressionLevel.Fastest ) )
				{
					stream.SetLength( 0 );
					compression.Write( BitConverter.GetBytes( expires.ToFileTime() ), 0, sizeof( Int64 ) );

					DataContractSerializer formatter = new DataContractSerializer( typeof( T ) );

					formatter.WriteObject( compression, value );
				}
			}
		}


		private String ComputeCacheKey( String value )
		{
			Check.NotNull( value, "value" );

			return value.ToString( StringOptions.LettersAndNumbers );
		}


		private Mutex CreateAccessGate( String filePath )
		{
			Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );

			return new Mutex( false, ComputeCacheKey( filePath ) );
		}


		private readonly String	m_directory;
	}
}