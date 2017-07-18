public static class StringExtensionMethods
{
	public static string RemoveExtension(this string str)
	{
		int dotIndex = str.LastIndexOf('.');
		if (dotIndex < 0)
			return str;
		return str.Substring(0, dotIndex);
	}



	/// <summary>
	/// Compares a string with another string. The other string can have wildcards ('*' and/or '?').
	/// </summary>
	/// <remarks>
	/// Nicked from http://www.codeproject.com/string/wildcmp.asp (thanks to Jack Handy).
	/// </remarks>
	/// <param name="str">String to compare</param>
	/// <param name="wild">String with wildcards ('*' and/or '?') to compare to</param>
	/// <returns>True if the strings match, false otherwise</returns>
	public static bool IsEqualRegular(this string str, string wild)
	{
		int strIndex = 0;
		int wildIndex = 0;
		int cpIndex = 0;
		int mpIndex = 0;
		int strLength = str.Length;
		int wildLength = wild.Length;

		if (wild == "")
		{
			return str == "";
		}

		while (strIndex < strLength && wild[wildIndex] != '*')
		{
			if (wild[wildIndex] != str[strIndex] && wild[wildIndex] != '?')
			{
				return false;
			}
			strIndex++;
			wildIndex++;
			if (wildIndex >= wildLength && !(strIndex >= strLength))
			{
				return false;
			}
		}

		while (strIndex < strLength)
		{
			if (wild[wildIndex] == '*')
			{
				wildIndex++;
				if (wildIndex >= wildLength)
				{
					return true;
				}
				mpIndex = wildIndex;
				cpIndex = strIndex + 1;
			}
			else if (wild[wildIndex] == str[strIndex] || wild[wildIndex] == '?')
			{
				strIndex++;
				wildIndex++;
			}
			else
			{
				wildIndex = mpIndex;
				strIndex = cpIndex;
				cpIndex++;
			}
		}

		while (wildIndex < wildLength && wild[wildIndex] == '*')
			wildIndex++;

		return wildIndex >= wildLength;
	}
}
