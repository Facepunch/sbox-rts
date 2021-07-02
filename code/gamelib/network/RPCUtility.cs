using Sandbox;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Gamelib.Network
{
	public static class RPCUtility
	{
		public static byte[] Compress<T>( T data )
		{
			using var stream = new MemoryStream();
			using var deflate = new DeflateStream( stream, CompressionLevel.Optimal );

			var serialized = JsonSerializer.SerializeToUtf8Bytes( data );

			deflate.Write( serialized );
			deflate.Close();

			return stream.ToArray();
		}

		public static T Decompress<T>( byte[] bytes )
		{
			using var outputStream = new MemoryStream();

			using ( var compressStream = new MemoryStream( bytes ) )
			{
				using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
				deflateStream.CopyTo( outputStream );
			}

			return JsonSerializer.Deserialize<T>( outputStream.ToArray() );
		}
	}
}
