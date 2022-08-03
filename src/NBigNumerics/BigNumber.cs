using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace NBigNumerics;

public sealed class BigNumber : IBigNumber
{
    private static readonly ICollection<char> AllowedDigits = new List<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
    private static readonly ICollection<char> AllowedSigning = new List<char> { '+', '-' };

    private readonly ImmutableList<int> _integerPart;
    private readonly ImmutableList<int> _decimalPart;
    private readonly bool _isPositive;

    private BigNumber(ImmutableList<int> integerPart, ImmutableList<int> decimalPart, bool isPositive)
    {
        _integerPart = integerPart;
        _decimalPart = decimalPart;
        _isPositive = isPositive;
    }

    public static BigNumber Create(string value)
    {
        return Create(value, CultureInfo.CurrentCulture);
    }

    public static BigNumber Create(string value, CultureInfo cultureInfo)
    {
        ValueGuard(value, cultureInfo, out string integerPartAsString, out string decimalPartAsString, out bool isPositive);

        integerPartAsString = RemoveLeadingZeros(integerPartAsString);

        ImmutableList<int> integerPart = CreateImmutableList(integerPartAsString);
        ImmutableList<int> decimalPart = CreateImmutableList(decimalPartAsString);

        return new BigNumber(integerPart, decimalPart, isPositive);
    }

    private static void ValueGuard(string value, CultureInfo cultureInfo, out string integerPart, out string decimalPart,
        out bool isPositive)
    {
        value = value.Trim();
        value = value.Replace(cultureInfo.NumberFormat.NumberGroupSeparator, String.Empty);

        string[] parts = value.Split(cultureInfo.NumberFormat.NumberDecimalSeparator);

        switch (parts.Length)
        {
            case > 2:
            {
                throw new ArgumentException("Too many decimal separators.", nameof(value));
            }
            case 2:
            {
                integerPart = parts[0];
                decimalPart = parts[1];
                break;
            }
            default:
            {
                integerPart = parts[0];
                decimalPart = string.Empty;
                break;
            }
        }

        if (!AllowedDigits.Contains(integerPart[0]) && AllowedSigning.Contains(integerPart[0]))
        {
            isPositive = integerPart[0] == '+';
            integerPart = integerPart.Substring(1);
        }
        else
        {
            isPositive = true;
        }

        if (integerPart.Any(character => !AllowedDigits.Contains(character)))
        {
            throw new ArgumentException("Unable to parse value. Unexpected digit(s) in integer part.", nameof(value));
        }

        if (decimalPart.Any(character => !AllowedDigits.Contains(character)))
        {
            throw new ArgumentException("Unable to parse value. Unexpected digit(s) in decimal part.", nameof(value));
        }
    }

    private static string RemoveLeadingZeros(string input)
    {
        string result = input;

        while (result.StartsWith('0'))
        {
            result = result.Substring(1);
        }

        return result;
    }

    private static ImmutableList<int> CreateImmutableList(string part)
    {
        return part.Select(character => character.ToString())
            .Select(int.Parse)
            .ToImmutableList();
    }

    public string Format()
    {
        return Format(CultureInfo.CurrentCulture);
    }

    public string Format(CultureInfo cultureInfo)
    {
        string decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;
        string groupSeparator = cultureInfo.NumberFormat.NumberGroupSeparator;
        int groupSize = 3; // todo use cultureInfo.NumberFormat.NumberGroupSize

        StringBuilder result = new StringBuilder();
        StringBuilder integerAsString = new StringBuilder();

        int groupCounter = 0;

        foreach (int digit in _integerPart.Reverse())
        {
            groupCounter++;

            if (groupCounter == groupSize + 1)
            {
                integerAsString.Insert(0, groupSeparator);
                groupCounter = 1;
            }

            integerAsString.Insert(0, digit);
        }

        result.Append(integerAsString);

        if (_decimalPart.Count > 0)
        {
            StringBuilder decimalAsString = new StringBuilder();

            foreach (int digit in _decimalPart.Reverse())
            {
                decimalAsString.Insert(0, digit);
            }

            result.Append(decimalSeparator);
            result.Append(decimalAsString);
        }

        if (!_isPositive)
        {
            result.Insert(0, '-');
        }

        return result.ToString();
    }
}
