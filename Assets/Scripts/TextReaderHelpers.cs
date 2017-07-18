using System.IO;
using System.Text;



public static class TextReaderHelpers
{
	// Use extension method to StringReader for reading words
	public static string ReadWord(this TextReader reader)
	{
		var builder = new StringBuilder();
		bool readingLeadingWhiteSpace = true;
		for (;;)
		{
			int i =  reader.Read();
			if (i < 0)
			{
				break;
			}
			var c = (char) i;
			if (char.IsWhiteSpace(c))
			{
				if (!readingLeadingWhiteSpace)
				{
					break;
				}
			}
			else
			{
				readingLeadingWhiteSpace = false;
				builder.Append(c);
			}
		}
		return builder.ToString();
	}
}