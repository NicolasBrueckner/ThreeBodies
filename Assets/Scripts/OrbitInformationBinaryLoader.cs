#region

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

#endregion

public static class OrbitInformationBinaryLoader
{
	public static OrbitInformation CurrentOrbitInfo;
	public static float[] TimesBuffer = Array.Empty<float>();

	//formatting: x0, y0, x1, y1, x2, y2
	public static float[] PositionsBuffer = Array.Empty<float>();
	public static Dictionary<string, int> CurrentOrbitsDict;

	/// <summary>
	///     Reads through metadata-only .bin (one record per orbit) and
	///     builds a dictionary mapping each orbit's name and its start offset.
	///     File layout assumed:
	///     [M: float(4)][t: float(M*4)][p: float(6*M*4)]                                       - 4 + 28M
	///     [name: uint, string(4+uint)][year: uint, string(4+uint)][g: uint, string(4+uint)]
	///     [T: float(4)][E: float(4)][L: float(4)][m1: float(4)][m2: float(4)][m3: float(4)]   - 36 + 3uint
	/// </summary>
	public static void FillOrbitsDict( string sequenceFileName )
	{
		CurrentOrbitsDict = new();

		TextAsset asset = Resources.Load<TextAsset>( sequenceFileName );
		byte[] bytes = asset.bytes;
		ReadOnlySpan<byte> span = new( bytes );
		int offset = 0;

		while( offset + 100 <= span.Length )
		{
			//get starting offset for orbit
			int o = offset;

			//skip time and position arrays
			int m = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
			offset += 4 + 7 * 4 * m;

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
			offset += 6 * 4;

			if( !string.IsNullOrEmpty( s ) )
				CurrentOrbitsDict.TryAdd( s, o );
		}
	}

	public static void LoadOrbitInfo( string orbitName, string sequenceFileName )
	{
		TextAsset asset = Resources.Load<TextAsset>( sequenceFileName );
		byte[] bytes = asset.bytes;
		ReadOnlySpan<byte> span = new( bytes );
		int offset = CurrentOrbitsDict[ orbitName ];

		int m = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		offset += 4;

		ReadOnlySpan<float> timeSpan = MemoryMarshal.Cast<byte, float>( span.Slice( offset, 4 * m ) );
		TimesBuffer = timeSpan.ToArray();
		offset += 4 * m;

		int posCount = 6 * m;

		ReadOnlySpan<float> positionSpan = MemoryMarshal.Cast<byte, float>( span.Slice( offset, 4 * posCount ) );
		PositionsBuffer = positionSpan.ToArray();

		offset += 4 * posCount;

		int strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		offset += 4 + strLen;
		strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		string year = Encoding.UTF8.GetString( span.Slice( offset + 4, strLen ) );
		offset += 4 + strLen;
		strLen = MemoryMarshal.Read<int>( span.Slice( offset, 4 ) );
		string G = Encoding.UTF8.GetString( span.Slice( offset + 4, strLen ) );
		offset += 4 + strLen;

		float T = MemoryMarshal.Read<float>( span.Slice( offset, 4 ) );
		float E = MemoryMarshal.Read<float>( span.Slice( offset + 4, 4 ) );
		float L = MemoryMarshal.Read<float>( span.Slice( offset + 8, 4 ) );
		float m1 = MemoryMarshal.Read<float>( span.Slice( offset + 12, 4 ) );
		float m2 = MemoryMarshal.Read<float>( span.Slice( offset + 16, 4 ) );
		float m3 = MemoryMarshal.Read<float>( span.Slice( offset + 20, 4 ) );

		Vector2[] initialPositions = new Vector2[ 3 ];
		Vector2[] initialVelocities = new Vector2[ 3 ];
		CurrentOrbitInfo = new OrbitInformation
		{
			orbitName = orbitName,
			year = year,
			freeGroupElement = G,
			period = T,
			energy = E,
			angularMomentum = L,
			initialPositions = initialPositions,
			initialVelocities = initialVelocities, //TODO: write and read those
			m1 = m1,
			m2 = m2,
			m3 = m3,
		};
	}
}