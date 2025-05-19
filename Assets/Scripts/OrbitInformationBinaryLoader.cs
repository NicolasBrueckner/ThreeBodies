using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class OrbitInformationBinaryLoader
{
	public static Dictionary<string, int> currentOrbitsDict;
	public static OrbitInformation currentOrbitInfo;

	/// <summary>
	/// Reads through metadata-only .bin (one record per orbit) and
	/// builds a dictionary mapping each orbit's name and its start offset.
	/// 
	/// File layout assumed:
	///[M: float(4)][t: float(M*4)][p: float(6*M*4)]                                       - 4 + 28M
	///[name: uint, string(4+uint)][year: uint, string(4+uint)][g: uint, string(4+uint)]
	///[T: float(4)][E: float(4)][L: float(4)][m1: float(4)][m2: float(4)][m3: float(4)]   - 36 + 3uint
	/// </summary>
	private static void FillOrbitsDict( string sequenceFileName )
	{
		currentOrbitsDict = new();

		TextAsset asset = Resources.Load<TextAsset>( sequenceFileName );
		byte[] bytes = asset.bytes;
		ReadOnlySpan<byte> span = new( bytes );
		int offset = 0;

		while ( offset <= span.Length )
		{
			//get starting offset for orbit
			int o = offset;

			//skip time and position arrays
			int M = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
			offset += 4 + 28 * M;

			//get orbit name
			int strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
			offset += 4;
			string s = Encoding.UTF8.GetString( span.Slice( offset, strLen ) );
			offset += strLen;

			//skip to next orbit
			strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
			offset += 4 + strLen;
			strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
			offset += 4 + strLen;
			offset += 24;

			if ( !string.IsNullOrEmpty( s ) )
				currentOrbitsDict.TryAdd( s, o );
		}
	}

	public static void LoadOrbitInfo( string orbitName, string sequenceFileName )
	{
		TextAsset asset = Resources.Load<TextAsset>( sequenceFileName );
		byte[] bytes = asset.bytes;
		ReadOnlySpan<byte> span = new( bytes );
		int offset = currentOrbitsDict[ orbitName ];

		int M = BinaryPrimitives.( span.Slice( offset, 4 ) );
		offset += 4;

		string year;
		string G;
		float T;
		float E;
		float L;
		Vector2[] initialVelocities = new Vector2[ 3 ];
		float[] t = new float[ M ];
		float[] masses = new float[ 3 ];
		Vector2[] posBody0 = new Vector2[ M ];
		Vector2[] posBody1 = new Vector2[ M ];
		Vector2[] posBody2 = new Vector2[ M ];

		ReadOnlySpan<float> timeSpan = MemoryMarshal.Cast<byte, float>( span.Slice( offset, 4 * M ) );
		timeSpan.CopyTo( t );
		offset += 4 * M;

		ReadOnlySpan<float> positionSpan = MemoryMarshal.Cast<byte, float>( span.Slice( offset, 4 * 6 * M ) );
		offset += 4 * 6 * M;

		for ( int i = 0; i < M; i++ )
		{
			int i6 = i * 6;
			posBody0[ i ] = new Vector2( positionSpan[ i6 + 0 ], positionSpan[ i6 + 1 ] );
			posBody1[ i ] = new Vector2( positionSpan[ i6 + 2 ], positionSpan[ i6 + 3 ] );
			posBody2[ i ] = new Vector2( positionSpan[ i6 + 4 ], positionSpan[ i6 + 5 ] );
		}


		int strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		offset += 4 + strLen;
		strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		year = Encoding.UTF8.GetString( span.Slice( offset + 4, strLen ) );
		offset += 4 + strLen;
		strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		G = Encoding.UTF8.GetString( span.Slice( offset + 4, strLen ) );
		offset += 4 + strLen;

		T = MemoryMarshal.Read<float>( span.Slice( offset, 4 ) );
		E = MemoryMarshal.Read<float>( span.Slice( offset + 4, 4 ) );
		L = MemoryMarshal.Read<float>( span.Slice( offset + 8, 4 ) );
		offset += 12;

		for ( int i = 0; i < 3; i++ )
			masses[ i ] = MemoryMarshal.Read<float>( span.Slice( offset + i * 4, 4 ) );
		offset += 3 * 4;

		currentOrbitInfo = new OrbitInformation
		{
			orbitName = orbitName,
			year = year,
			freeGroupElement = G,
			period = T,
			energy = E,
			angularMomentum = L,
			initialPositions = new Vector2[ 3 ] { posBody0[ 0 ], posBody1[ 0 ], posBody2[ 0 ] },
			initialVelocities = initialVelocities,//TODO: write and read those

			times = t,
			posBody0 = posBody0,
			posBody1 = posBody1,
			posBody2 = posBody2,
			masses = masses
		};
	}
}
