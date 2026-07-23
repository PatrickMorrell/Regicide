namespace Regicide.Extensions;

public static class StringExtension
{
	public static bool StringEquals(this string a, string b)
	{
		return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
	}
}
