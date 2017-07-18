using System.IO;
using System.Text;

namespace ModelRailway
{
	internal static class StreamReaderExtensions
	{
		// Use extension method to StreamReader for reading words
		// '(' and ')' are separators and are returned as separate words
		public static string ReadWord(this StreamReader reader)
		{
			var builder = new StringBuilder();
			bool readingLeadingWhiteSpace = true;
			int cCount = 0;
			for (;;)
			{
				int i =  reader.Peek();
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
					reader.Read();
				}
				else
				{
					readingLeadingWhiteSpace = false;
					if (c == '(' || c == ')')
					{
						if (cCount == 0)
						{
							reader.Read();
							builder.Append(c);
						}
						break;
					}
					reader.Read();
					builder.Append(c);
					cCount++;
				}
			}
			return builder.ToString();
		}
	}
}
