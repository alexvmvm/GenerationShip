using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public enum ToStringStyle
{
    Float,
    Integer,
    Percentage,
    PercentageInverse
}


public interface ITextGiver
{
    public string GetText(string symbol);
}

public static class TextUtils
{
    public static bool NullOrEmpty( this string str )
    {
        return string.IsNullOrEmpty(str);
    }
    
    public static string ToGarbled(this string text)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < text.Length; i++)
        {
            var app = text[i] switch
            {
                'a' => "à",
                'b' => "þ",
                'c' => "ç",
                'd' => "ð",
                'e' => "è",
                'f' => "Ƒ",
                'g' => "ğ",
                'h' => "ĥ",
                'i' => "ì",
                'j' => "ĵ",
                'k' => "к",
                'l' => "ſ",
                'm' => "ṁ",
                'n' => "ƞ",
                'o' => "ò",
                'p' => "ṗ",
                'q' => "q",
                'r' => "ṟ",
                's' => "ș",
                't' => "ṭ",
                'u' => "ù",
                'v' => "ṽ",
                'w' => "ẅ",
                'x' => "ẋ",
                'y' => "ý",
                'z' => "ž",
                _ => "*",
            };

            sb.Append(app);
        }

        return sb.ToString();
    }
    
    public static string Colorize(this string value, Color color)
		=> "<color=#" + color.ToHexString() + ">" + value + "</color>";

    public static string Bolden(this string value)
        => $"<b>{value}</b>";

    public static string Formatted(this string value, params object[] parms)
    {
        for (var i = 0; i < parms.Length; i += 2)
        {
            var key = parms[i]?.ToString();
            var val = parms[i + 1];

            if (key == null) continue;

            // Find all tokens in the string that start with this key, including ones like {KEY_label}
            var pattern = @"\{" + key + @"(?:_([^\}]+))?\}";
            value = Regex.Replace(value, pattern, match =>
            {
                var symbol = match.Groups[1].Success ? match.Groups[1].Value : null;

                if (val is ITextGiver textGiver)
                    return textGiver.GetText(symbol) ?? match.Value;

                return val?.ToString() ?? match.Value;
            });
        }

        return value;
    }

    public static string SymbolAfterUnderscore(this string text)
    {
        var split = text.Split("_");
        return split.Length <= 1 ? null : split[1];
    }

    public static string CapitalizeFirst(this string str)
	{
        if( str.NullOrEmpty() )
			return str;

		if( char.IsUpper(str[0]) )
			return str;

		if( str.Length == 1 )
			return str.ToUpper();

		return char.ToUpper(str[0]) + str.Substring(1);
	}

	public static string ToStringSigned(this float value)
		=> value >= 0 ? "+" + Mathf.Abs(value) : "-" + Mathf.Abs(value);

	public static string ToStringSigned(this int value)
		=> value >= 0 ? "+" + Mathf.Abs(value) : "-" + Mathf.Abs(value);

	public static string ToStringPercent(this float value)
		=> (Mathf.RoundToInt((value) * 100)) + "%";

    public static string ToStringMoney(this float value)
        => "$" + value.ToStringByStyle(ToStringStyle.Float);

    public static string ToStringMoney(this int value)
        => "$" + value.ToString();

	public static string ToStringByStyle(this float value, ToStringStyle style)
		=> style switch
        {
            ToStringStyle.Float 			=> value.ToString("0.##"),
            ToStringStyle.Integer 			=> ((int)value).ToString(),
            ToStringStyle.Percentage 		=> (value * 100).ToString("F0") + "%",
			ToStringStyle.PercentageInverse => (((1f + (1f - value)))*100).ToString("F0") + "%",
            _ 								=> value.ToString()
        };
    
	public static string ToStringFloatNeat(this float value)
		=> (Mathf.Abs(value) > 0.00001f) ? value.ToString("0.00") : value.ToString("0");

    public static string ToStringPower(this int power)
        => $"{power} Kwh";

    public static string ToCommaList<T>(this List<T> list, Func<T, string> textGetter)
    {
        if (list == null || list.Count == 0)
        {
            return string.Empty;
        }
        return string.Join(", ", list.Select(item => textGetter(item)));
    }

    public static string ToLineList( this IEnumerable<string> entries, string prefix = null )
    {
        var sb = new StringBuilder();
        bool first = true;
        foreach( var s in entries )
        {
            if( !first )
                sb.Append("\n");
            if( prefix != null )
                sb.Append(prefix);
            sb.Append(s);
            first = false;
        }
        return sb.ToString();
    }
}