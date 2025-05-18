using System;
using System.Buffers.Binary;
using System.Collections.Generic;
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
	/// [ uint32 len(name) ][ name bytes ]
	/// [ uint32 len(year) ][ year bytes ]
	/// [ uint32 len(G)	   ][ G	   bytes ]
	/// [ float32 T ][ float32 E ][ float32 L ]
	/// </summary>
	private static void FillOrbitsDict( string sequenceFileName )
	{
		currentOrbitsDict = new();

		TextAsset asset = Resources.Load<TextAsset>( sequenceFileName );
		byte[] bytes = asset.bytes;
		ReadOnlySpan<byte> span = new( bytes );
		int offset = 0;

		while ( offset + 4 <= span.Length )
		{
			int o = offset;

			int strLen = BinaryPrimitives.ReadInt32LittleEndian( span.Slice( offset, 4 ) );
			offset += 4;

			if ( offset + strLen > span.Length )
				break;

			string s = Encoding.UTF8.GetString( span.Slice( offset, strLen ) );
			offset += strLen;

			//skip to next orbit
			strLen = BinaryPrimitives.ReadInt32LittleEndian( span.Slice( offset, 4 ) );
			offset += 4 + strLen;
			strLen = BinaryPrimitives.ReadInt32LittleEndian( span.Slice( offset, 4 ) );
			offset += 4 + strLen;
			offset += 12;

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

		int strLen = BinaryPrimitives.ReadInt32LittleEndian( span.Slice( offset, 4 ) );
		offset += 4;
		string oN = Encoding.UTF8.GetString( span.Slice( offset, strLen ) );
		offset += strLen;

		currentOrbitInfo = new OrbitInformation
		{
			orbitName = oN,
			year =
		};
	}
}
