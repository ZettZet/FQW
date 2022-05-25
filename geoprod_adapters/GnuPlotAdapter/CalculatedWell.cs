using System.Text.Json.Serialization;

namespace GnuPlotAdapter.Models;

public record RegionCalculated {
    public string Region { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public double Result { get; set; }
    [JsonPropertyName("result_uw")]
    public double ResultUw { get; set; }
}

public record WellCalculated {
    public string Id { get; set; }
    public string Name { get; set; }
    public double Result { get; set; }
}

public record RegionPlotable {
    public string Region { get; set; }
    public double Result { get; set; }
    public double ResultUw { get; set; }
}