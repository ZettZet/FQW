using System.Globalization;
using System.Net.Mime;
using GnuPlotAdapter.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


string Escape(string str) {
    var rgx = new Regex("(['^$.|?*+()\\\\_])");
    return rgx.Replace(str, "\\\\\\$1");
}

app.MapPost("/com/{units}", (string units, Dictionary<string, double> comCalculated) => {
    var gnuPlotStringBuilder = new StringBuilder();

    gnuPlotStringBuilder.AppendLine("$data << EOD");
    gnuPlotStringBuilder.AppendLine("Region \"С учётом КУ\" ");
    foreach (var (region, value) in comCalculated) {
        var replace = Escape(region);
        var weighted = value.ToString(CultureInfo.InvariantCulture);
        gnuPlotStringBuilder.AppendLine($@"{replace} {weighted}");
    }

    var fileName = $"com-{DateTime.Now:s}.svg";

    gnuPlotStringBuilder.AppendLine("EOD");

    gnuPlotStringBuilder.AppendLine("set title \"Компенсация\"");
    gnuPlotStringBuilder.AppendLine("set terminal svg size 700,500 fname 'Verdana'");
    gnuPlotStringBuilder.AppendLine($"system \"touch {fileName}\"");
    gnuPlotStringBuilder.AppendLine($"set output '{fileName}'");
    gnuPlotStringBuilder.AppendLine("set style data histogram");
    gnuPlotStringBuilder.AppendLine("set offsets 1,1");
    gnuPlotStringBuilder.AppendLine("set grid");
    gnuPlotStringBuilder.AppendLine("set key outside");
    gnuPlotStringBuilder.AppendLine("set key width -11");
    gnuPlotStringBuilder.AppendLine("set auto x");
    gnuPlotStringBuilder.AppendLine("set xtics border in rotate by -45 offset character 0, 0, 0 norangelimit");
    gnuPlotStringBuilder.AppendLine("set boxwidth 0.5");
    gnuPlotStringBuilder.AppendLine("set style fill solid");

    gnuPlotStringBuilder.AppendLine(
        "plot $data using 2:xtic(1) title columnheader(2) fc rgb \"#99ffff\"");

    return Results.Text(gnuPlotStringBuilder.ToString(), "text/plain", Encoding.UTF8);
});

string GetCorrectName(string microservice) {
    return microservice switch {
        "zak" => "Закачка воды",
        "oil" => "Добыча нефти",
        "liq" => "Добыча жидкости",
        _ => throw new ArgumentOutOfRangeException(nameof(microservice), microservice, "Invalid microservice call")
    };
}

app.MapPost("/{type}/{units}", (string type, string units, RegionCalculated[] wells) => {
    var plottable = wells
        .GroupBy(well => well.Region)
        .Select(grouped => new RegionPlotable {
            Region = grouped.Key,
            Result = grouped.Sum(item => item.Result),
            ResultUw = grouped.Sum(item => item.ResultUw)
        });

    var gnuPlotStringBuilder = new StringBuilder();
    var fileName = $"{type}-{DateTime.Now:s}.svg";

    gnuPlotStringBuilder.AppendLine("$data << EOD");
    gnuPlotStringBuilder.AppendLine("Region \"С учётом КУ\" \"Без учёта КУ\"");
    foreach (var region in plottable) {
        var replace = Escape(region.Region);
        var weighted = region.Result.ToString(CultureInfo.InvariantCulture);
        var unweighted = region.ResultUw.ToString(CultureInfo.InvariantCulture);
        gnuPlotStringBuilder.AppendLine(
            $@"{replace} {weighted} {unweighted}");
    }

    gnuPlotStringBuilder.AppendLine("EOD");

    gnuPlotStringBuilder.AppendLine($"set title \"{GetCorrectName(type)}\"");
    // gnuPlotStringBuilder.AppendLine("set datafile separator \" \"");
    gnuPlotStringBuilder.AppendLine("set terminal svg size 800,500 fname 'Verdana'");
    gnuPlotStringBuilder.AppendLine($"system \"touch {fileName}\"");
    gnuPlotStringBuilder.AppendLine($"set output '{fileName}'");
    gnuPlotStringBuilder.AppendLine("set style data histogram");
    gnuPlotStringBuilder.AppendLine("set offsets 1,1");
    gnuPlotStringBuilder.AppendLine("set grid");
    gnuPlotStringBuilder.AppendLine("set key outside");
    gnuPlotStringBuilder.AppendLine("set key width -13");
    gnuPlotStringBuilder.AppendLine("set auto x");
    gnuPlotStringBuilder.AppendLine("set xtics border in rotate by -45 offset character 0, 0, 0 norangelimit");
    gnuPlotStringBuilder.AppendLine("set boxwidth 0.5");
    gnuPlotStringBuilder.AppendLine($"set ylabel \"{units}\"");
    gnuPlotStringBuilder.AppendLine("set style fill solid");

    gnuPlotStringBuilder.AppendLine(
        "plot $data using 2:xtic(1) title columnheader(2) fc rgb \"#99ffff\", '' u 3 title columnheader(3) fc rgb \"#4671d5\"");

    return Results.Text(gnuPlotStringBuilder.ToString(), "text/plain", Encoding.UTF8);
});

app.Run();