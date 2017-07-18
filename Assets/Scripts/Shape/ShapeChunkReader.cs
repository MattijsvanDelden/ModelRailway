using System.IO;

internal abstract class ShapeChunkReader
{
	protected readonly Stream m_stream;
	
	protected ShapeChunkReader(Stream stream)
	{
		m_stream = stream;
	}

	public bool EndOfStream { get { return false; } }

	public void Close()
	{
		m_stream.Close();
	}

	public abstract ShapeChunkType ReadChunkHeader(out string chunkName);

	public abstract int ReadInt();

	public abstract float ReadFloat();

	public abstract void SkipRemainingChunk();

	public abstract void EndChunk(bool readChunkEnd);

	public abstract string ReadString();

	public abstract int ReadColour();
}
