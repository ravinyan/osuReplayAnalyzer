// IMatchFinder.cs


// IMatchFinder.cs


// IMatchFinder.cs


// IMatchFinder.cs

using System;

namespace OsuFileParsers.Decoders.SevenZip.Compress.LZ
{
	interface IInWindowStream
	{
		void SetStream(Stream inStream);
		void Init();
		void ReleaseStream();
		byte GetIndexByte(int index);
		uint GetMatchLen(int index, uint distance, uint limit);
		uint GetNumAvailableBytes();
	}

	interface IMatchFinder : IInWindowStream
	{
		void Create(uint historySize, uint keepAddBufferBefore,
				uint matchMaxLen, uint keepAddBufferAfter);
		uint GetMatches(uint[] distances);
		void Skip(uint num);
	}
}
