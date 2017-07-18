using System;
using System.IO;
using UnityEngine;



namespace ModelRailway
{
	static class MSTSHelpers
	{
		public static float ParseMeasurement(string s)
		{
			float factor = 1;
			if (s.EndsWith("deg"))
			{
				s = s.Substring(0, s.Length - 3);
				factor = (float) (Math.PI / 180.0);
			}
			else if (s.EndsWith("cm"))
			{
				s = s.Substring(0, s.Length - 2);
				factor = 0.01f;
			}
			else if (s.EndsWith("m"))
			{
				s = s.Substring(0, s.Length - 1);
			}
			else if (s.EndsWith("in"))
			{
				s = s.Substring(0, s.Length - 2);
				factor = 2.54f / 100;
			}
			float value = 0;
			try
			{
				value = float.Parse(s);
			}
			catch (FormatException fe)
			{
				Debug.LogError(fe.Message);
			}
			return factor * value;
		}



		public static void SkipChunkContents(StreamReader reader)
		{
			int depth = 0;
			for(;;)
			{
				var c = (char) reader.Peek();
				if (c == '(')
				{
					depth++;
				}
				else if (c == ')')
				{
					if (depth == 0)
					{
						break;
					}
					depth--;
				}
				reader.Read();
			}
		}

	}
}
