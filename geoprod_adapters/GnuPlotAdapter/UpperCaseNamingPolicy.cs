using System.Text.Json;

namespace GnuPlotAdapter; 

public class UpperCaseNamingPolicy : JsonNamingPolicy {
    public override string ConvertName(string name) =>
        name.ToUpper();
}