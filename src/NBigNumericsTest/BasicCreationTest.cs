using System;
using System.Globalization;
using NBigNumerics;
using NUnit.Framework;

namespace NBigNumericsTest;

public class BasicCreationTest
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Console.WriteLine("Set current culture to en-US.");
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
    }

    [Test]
    [TestCase("1234567890", "1,234,567,890")]
    [TestCase("+1234567890", "1,234,567,890")]
    [TestCase("-1234567890", "-1,234,567,890")]
    [TestCase("1,234,567,890", "1,234,567,890")]
    [TestCase("+1,234,567,890", "1,234,567,890")]
    [TestCase("-1,234,567,890", "-1,234,567,890")]
    public void TestCreationOfInteger(string input, string expected)
    {
        BigNumber number = BigNumber.Create(input);
        string numberAsString = number.Format();

        Assert.AreEqual(expected, numberAsString);
    }

    [Test]
    [TestCase("0123456789", "123,456,789")]
    [TestCase("+0123456789", "123,456,789")]
    [TestCase("-0123456789", "-123,456,789")]
    [TestCase("0,123,456,789", "123,456,789")]
    [TestCase("+0,123,456,789", "123,456,789")]
    [TestCase("-0,123,456,789", "-123,456,789")]
    public void TestCreationOfIntegerWithLeadingZero(string input, string expected)
    {
        BigNumber number = BigNumber.Create(input);
        string numberAsString = number.Format();

        Assert.AreEqual(expected, numberAsString);
    }

    [Test]
    [TestCase("184467440737095516150", "184,467,440,737,095,516,150")] // > ulong.MaxValue
    [TestCase("+184467440737095516150", "184,467,440,737,095,516,150")] // > ulong.MaxValue
    [TestCase("-184467440737095516150", "-184,467,440,737,095,516,150")] // > ulong.MaxValue
    [TestCase("184,467,440,737,095,516,150", "184,467,440,737,095,516,150")] // > ulong.MaxValue
    [TestCase("+184,467,440,737,095,516,150", "184,467,440,737,095,516,150")] // > ulong.MaxValue
    [TestCase("-184,467,440,737,095,516,150", "-184,467,440,737,095,516,150")] // > ulong.MaxValue
    public void TestCreationOfLargeValues(string input, string expected)
    {
        BigNumber number = BigNumber.Create(input);
        string numberAsString = number.Format();

        Assert.AreEqual(expected, numberAsString);
    }

    [Test]
    [TestCase("123456789.", "123,456,789")]
    [TestCase("+123456789.", "123,456,789")]
    [TestCase("-123456789.", "-123,456,789")]
    [TestCase("123,456,789.", "123,456,789")]
    [TestCase("+123,456,789.", "123,456,789")]
    [TestCase("-123,456,789.", "-123,456,789")]
    [TestCase("1234567.89", "1,234,567.89")]
    [TestCase("+1234567.89", "1,234,567.89")]
    [TestCase("-1234567.89", "-1,234,567.89")]
    [TestCase("1,234,567.89", "1,234,567.89")]
    [TestCase("+1,234,567.89", "1,234,567.89")]
    [TestCase("-1,234,567.89", "-1,234,567.89")]
    public void TestCreationOfDecimal(string input, string expected)
    {
        BigNumber number = BigNumber.Create(input);
        string numberAsString = number.Format();

        Assert.AreEqual(expected, numberAsString);
    }

    [Test]
    public void TestTooManyDecimalSeparators()
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BigNumber.Create("123546.78.90"));

        Assert.NotNull(exception);
        Assert.AreEqual("Too many decimal separators. (Parameter 'value')", exception!.Message);
        Assert.AreEqual("value", exception.ParamName);
    }

    [Test]
    [TestCase("123456789a")]
    [TestCase("123 456789")]
    [TestCase("/123456789")]
    public void TestInvalidDigitInIntegerPart(string input)
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BigNumber.Create(input));

        Assert.NotNull(exception);
        Assert.AreEqual("Unable to parse value. Unexpected digit(s) in integer part. (Parameter 'value')", exception!.Message);
        Assert.AreEqual("value", exception.ParamName);
    }

    [Test]
    [TestCase("123456789. 1")]
    [TestCase("123456789.a")]
    [TestCase("123456789.1/")]
    public void TestInvalidDigitInDecimalPart(string input)
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BigNumber.Create(input));

        Assert.NotNull(exception);
        Assert.AreEqual("Unable to parse value. Unexpected digit(s) in decimal part. (Parameter 'value')", exception!.Message);
        Assert.AreEqual("value", exception.ParamName);
    }

}
