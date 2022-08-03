using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace NBigNumerics;

public class BigNumber : IBigNumber
{
    private static readonly char DECIMAL_SEPARATOR = '#';
    private static readonly ICollection<char> ALLOWED_DIGITS = new List<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
    private static readonly ICollection<char> ALLOWED_SIGNING = new List<char> { '+', '-' };

    private readonly ImmutableList<int> integerPart;
    private readonly ImmutableList<int> decimalPart;
    private readonly bool isPositiv;

    public BigNumber(ImmutableList<int> integerPart, ImmutableList<int> decimalPart, bool isPositiv)
    {
        this.integerPart = integerPart;
        this.decimalPart = decimalPart;
        this.isPositiv = isPositiv;
    }

    public static BigNumber Create(string value)
    {
        return Create(value, CultureInfo.CurrentCulture);
    }

    public static BigNumber Create(string value, CultureInfo cultureInfo)
    {
        string integerPartAsString;
        string? decimalPartAsString;
        bool isPositive;

        ValueGuard(value, cultureInfo, out integerPartAsString, out decimalPartAsString, out isPositive);

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
        value = value.Replace(cultureInfo.NumberFormat.NumberDecimalSeparator, DECIMAL_SEPARATOR.ToString());

        string[] parts = value.Split(DECIMAL_SEPARATOR);

        if (parts.Length > 2)
        {
            throw new ArgumentException("Too many decimal separators.", nameof(value));
        }

        if (parts.Length == 2)
        {
            integerPart = parts[0];
            decimalPart = parts[1];
        }
        else
        {
            integerPart = parts[0];
            decimalPart = String.Empty;
        }

        if (!ALLOWED_DIGITS.Contains(integerPart[0]) && ALLOWED_SIGNING.Contains(integerPart[0]))
        {
            isPositive = integerPart[0] == '+';
            integerPart = integerPart.Substring(1);
        }
        else
        {
            isPositive = true;
        }

        if (integerPart.Any(character => !ALLOWED_DIGITS.Contains(character)))
        {
            throw new ArgumentException("Unable to parse value. Unexpected digit(s) in integer part.", nameof(value));
        }

        if (decimalPart.Any(character => !ALLOWED_DIGITS.Contains(character)))
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
        return part.Select(character => int.Parse(character.ToString()))
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

        foreach (int digit in integerPart.Reverse())
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

        if (decimalPart.Count > 0)
        {
            StringBuilder decimalAsString = new StringBuilder();

            foreach (int digit in decimalPart.Reverse())
            {
                decimalAsString.Insert(0, digit);
            }

            result.Append(decimalSeparator);
            result.Append(decimalAsString);
        }

        if (!isPositiv)
        {
            result.Insert(0, '-');
        }

        return result.ToString();
    }
}
