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
    private readonly Signing _signing;

    private BigNumber(ImmutableList<int> integerPart, ImmutableList<int> decimalPart, Signing signing)
    {
        _integerPart = integerPart;
        _decimalPart = decimalPart;
        _signing = signing;
    }

    public static BigNumber Create(string value)
    {
        return Create(value, CultureInfo.CurrentCulture);
    }

    public static BigNumber Create(string value, CultureInfo cultureInfo)
    {
        RawBigNumber raw = CreateRaw(value, cultureInfo);

#pragma warning disable AV1522
        raw = raw with { IntegerPart = RemoveLeadingZeros(raw.IntegerPart) };
#pragma warning restore AV1522

        return raw.ToBigNumber();
    }

    private static RawBigNumber CreateRaw(string value, CultureInfo cultureInfo)
    {
        value = value.Trim()
            .Replace(cultureInfo.NumberFormat.NumberGroupSeparator, String.Empty);

        string[] parts = value.Split(cultureInfo.NumberFormat.NumberDecimalSeparator);

        if (parts.Length > 2)
        {
            throw new ArgumentException("Too many decimal separators.", nameof(value));
        }

        RawBigNumber raw = CreateRawFromParts(parts);

        if (raw.IntegerPart.Any(character => !AllowedDigits.Contains(character)))
        {
            throw new ArgumentException("Unable to parse value. Unexpected digit(s) in integer part.", nameof(value));
        }

        if (raw.DecimalPart.Any(character => !AllowedDigits.Contains(character)))
        {
            throw new ArgumentException("Unable to parse value. Unexpected digit(s) in decimal part.", nameof(value));
        }

        return raw;
    }

    private static RawBigNumber CreateRawFromParts(string[] parts)
    {
        Signing signing;
        string integerPart = parts[0];

        if (!AllowedDigits.Contains(integerPart[0]) && AllowedSigning.Contains(integerPart[0]))
        {
            signing = integerPart[0] == '+' ? Signing.Positive : Signing.Negative;
            integerPart = integerPart.Substring(1);
        }
        else
        {
            signing = Signing.Positive;
        }

        return new RawBigNumber(integerPart, parts.Length == 2 ? parts[1] : string.Empty, signing);
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

    public string Format()
    {
        return Format(CultureInfo.CurrentCulture);
    }

    public string Format(CultureInfo cultureInfo)
    {
        string decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;

        string integerAsString = FormatIntegerPart(cultureInfo);

        switch (_decimalPart.Count)
        {
            case > 0:
            {
                string decimalAsString = FormatDecimalPart();
                return $"{integerAsString}{decimalSeparator}{decimalAsString}";
            }
            default:
            {
                return integerAsString;
            }
        }
    }

    private string FormatIntegerPart(CultureInfo cultureInfo)
    {
        int groupSize = 3; // todo use cultureInfo.NumberFormat.NumberGroupSize

        StringBuilder integerAsString = new StringBuilder();

        int groupCounter = 0;

        foreach (int digit in _integerPart.Reverse())
        {
            groupCounter++;

            if (groupCounter == groupSize + 1)
            {
                integerAsString.Insert(0, cultureInfo.NumberFormat.NumberGroupSeparator);
                groupCounter = 1;
            }

            integerAsString.Insert(0, digit);
        }

        if (_signing == Signing.Negative)
        {
            integerAsString.Insert(0, '-');
        }

        return integerAsString.ToString();
    }

    private string FormatDecimalPart()
    {
        return _decimalPart.Aggregate(string.Empty, (seed, digit) => seed + digit);
    }

    private enum Signing
    {
        Positive,
        Negative
    }

    private record RawBigNumber(string IntegerPart, string DecimalPart, Signing Signing)
    {
        public BigNumber ToBigNumber()
        {
            ImmutableList<int> immutableIntegerPart = CreateImmutableList(IntegerPart);
            ImmutableList<int> immutableDecimalPart = CreateImmutableList(DecimalPart);

            return new BigNumber(immutableIntegerPart, immutableDecimalPart, Signing);
        }

        private static ImmutableList<int> CreateImmutableList(string part)
        {
            return part.Select(character => character.ToString())
                .Select(int.Parse)
                .ToImmutableList();
        }
    }
}
