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
// builder.Services.Configure<JsonOptions>(option =>
//     option.JsonSerializerOptions.PropertyNamingPolicy = new UpperCaseNamingPolicy());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapPost("/com/", (Dictionary<string, double> comCalculated) => {
    var rgx = new Regex("(['^$.|?*+()\\\\_])");
    var gnuPlotStringBuilder = new StringBuilder();

    gnuPlotStringBuilder.AppendLine("$data << EOD");
    gnuPlotStringBuilder.AppendLine("Region Weighted");
    foreach (var (region, value) in comCalculated) {
        var replace = rgx.Replace(region, "\\\\\\$1");
        var weighted = value.ToString(CultureInfo.InvariantCulture);
        gnuPlotStringBuilder.AppendLine($@"{replace} {weighted}");
    }

    gnuPlotStringBuilder.AppendLine("EOD");

    gnuPlotStringBuilder.AppendLine($"set title \"COM\"");
    gnuPlotStringBuilder.AppendLine("set datafile separator \" \"");
    gnuPlotStringBuilder.AppendLine("set terminal svg size 700,500 fname 'Verdana'");
    gnuPlotStringBuilder.AppendLine("set output 'introduction.svg'");
    gnuPlotStringBuilder.AppendLine("set style data histogram");
    gnuPlotStringBuilder.AppendLine("set offsets 1,1");
    gnuPlotStringBuilder.AppendLine("set grid");
    gnuPlotStringBuilder.AppendLine("set key outside");
    gnuPlotStringBuilder.AppendLine("set auto x");
    gnuPlotStringBuilder.AppendLine("set xtics border in rotate by -45 offset character 0, 0, 0 norangelimit");
    gnuPlotStringBuilder.AppendLine("set boxwidth 0.5");
    gnuPlotStringBuilder.AppendLine("set style fill solid");

    gnuPlotStringBuilder.AppendLine(
        "plot $data using 2:xtic(1) ti col fc rgb \"#99ffff\"");

    return Results.Text(gnuPlotStringBuilder.ToString(), "text/plain", Encoding.UTF8);
});

app.MapPost("/{type}/{units}", (string type, string units, RegionCalculated[] wells) => {
    var plottable = wells
        .GroupBy(well => well.Region)
        .Select(grouped => new RegionPlotable {
            Region = grouped.Key,
            Result = grouped.Sum(item => item.Result),
            ResultUw = grouped.Sum(item => item.ResultUw)
        });

    var rgx = new Regex("(['^$.|?*+()\\\\_])");
    var gnuPlotStringBuilder = new StringBuilder();

    gnuPlotStringBuilder.AppendLine("$data << EOD");
    gnuPlotStringBuilder.AppendLine("Region Weighted Unweighted");
    foreach (var region in plottable) {
        var replace = rgx.Replace(region.Region, "\\\\\\$1");
        var weighted = region.Result.ToString(CultureInfo.InvariantCulture);
        var unweighted = region.ResultUw.ToString(CultureInfo.InvariantCulture);
        gnuPlotStringBuilder.AppendLine(
            $@"{replace} {weighted} {unweighted}");
    }

    gnuPlotStringBuilder.AppendLine("EOD");

    gnuPlotStringBuilder.AppendLine($"set title \"{type.ToUpper()}\"");
    gnuPlotStringBuilder.AppendLine("set datafile separator \" \"");
    gnuPlotStringBuilder.AppendLine("set terminal svg size 700,500 fname 'Verdana'");
    gnuPlotStringBuilder.AppendLine("set output 'introduction.svg'");
    gnuPlotStringBuilder.AppendLine("set style data histogram");
    gnuPlotStringBuilder.AppendLine("set offsets 1,1");
    gnuPlotStringBuilder.AppendLine("set grid");
    gnuPlotStringBuilder.AppendLine("set key outside");
    gnuPlotStringBuilder.AppendLine("set auto x");
    gnuPlotStringBuilder.AppendLine("set xtics border in rotate by -45 offset character 0, 0, 0 norangelimit");
    gnuPlotStringBuilder.AppendLine("set boxwidth 0.5");
    gnuPlotStringBuilder.AppendLine($"set ylabel \"{units}\"");
    gnuPlotStringBuilder.AppendLine("set style fill solid");

    gnuPlotStringBuilder.AppendLine(
        "plot $data using 2:xtic(1) ti col fc rgb \"#99ffff\", '' u 3 ti col fc rgb \"#4671d5\"");

    return Results.Text(gnuPlotStringBuilder.ToString(), "text/plain", Encoding.UTF8);
});

app.Run();