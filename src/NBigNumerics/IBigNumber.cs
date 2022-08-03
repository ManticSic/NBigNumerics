using System.Globalization;

namespace NBigNumerics;

public interface IBigNumber
{
    public string Format();

    public string Format(CultureInfo cultureInfo);
}
