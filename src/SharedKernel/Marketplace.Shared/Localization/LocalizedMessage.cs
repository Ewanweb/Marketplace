using System.Globalization;

namespace Marketplace.Shared.Localization;

public static class LocalizedMessage
{
    public static string Get(string english, string dari, string pashto)
    {
        var culture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();

        if (culture.StartsWith("ps"))
        {
            return pashto;
        }

        if (culture.StartsWith("prs") || culture.StartsWith("fa"))
        {
            return dari;
        }

        return english;
    }
}
