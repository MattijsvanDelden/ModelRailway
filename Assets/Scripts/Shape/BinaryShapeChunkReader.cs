using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

internal class BinaryShapeChunkReader : ShapeChunkReader
{
	private readonly BinaryReader m_binaryReader;

	private readonly Stack<int> m_remainingChunkByteCounts = new Stack<int>();

	private int m_remainingLeafChunkByteCount = -1;


	public BinaryShapeChunkReader(Stream stream) : base(stream)
	{
		m_binaryReader = new BinaryReader(stream);
	}



	public override int ReadInt()
	{
		m_remainingLeafChunkByteCount -= 4;
		return m_binaryReader.ReadInt32();
	}



	public override float ReadFloat()
	{
		m_remainingLeafChunkByteCount -= 4;
		return m_binaryReader.ReadSingle();	
	}



	public override ShapeChunkType ReadChunkHeader(out string chunkName)
	{
		if (m_remainingLeafChunkByteCount == 0)
		{
			chunkName = "";
			return ShapeChunkType.EndOfChunk;
		}

		ShapeChunkType chunkType = (ShapeChunkType) m_binaryReader.ReadUInt16();

		m_binaryReader.ReadUInt16();	// Skip chunk flags
		int chunkContentSize = (int) m_binaryReader.ReadUInt32();

		int chunkNameLength = m_binaryReader.ReadByte();
		if (chunkNameLength == 0)
		{
			chunkName = "";
		}
		else
		{
			byte[] buffer = m_binaryReader.ReadBytes(chunkNameLength * 2);
			chunkName = Encoding.Unicode.GetString(buffer, 0, chunkNameLength * 2);
		}
		m_remainingChunkByteCounts.Push(m_remainingLeafChunkByteCount - chunkContentSize - 8);
		m_remainingLeafChunkByteCount = chunkContentSize - 1 - 2 * chunkNameLength;
		return chunkType;
	}



	public override void SkipRemainingChunk()
	{
		m_binaryReader.ReadBytes(m_remainingLeafChunkByteCount);
		m_remainingLeafChunkByteCount = 0;
	}



	public override void EndChunk(bool readChunkEnd)
	{
		Debug.Assert(m_remainingLeafChunkByteCount == 0);
		m_remainingLeafChunkByteCount = m_remainingChunkByteCounts.Pop();
	}



	public override string ReadString()
	{

		int stringLength = m_binaryReader.ReadUInt16();
		if (stringLength == 0)
		{
			return "";
		}
		m_remainingLeafChunkByteCount -= 2 + stringLength * 2;
        byte[] buffer = m_binaryReader.ReadBytes(stringLength * 2);
        return Encoding.Unicode.GetString(buffer, 0, stringLength * 2);
	}



	public override int ReadColour()
	{
		m_remainingLeafChunkByteCount -= 4;
		return m_binaryReader.ReadInt32();
	}
}
