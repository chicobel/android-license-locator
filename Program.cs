﻿// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using LicenseLocator;

Console.WriteLine("Welcome!");
Console.WriteLine(@"Enter path to the deps.txt file generated by running  ./gradlew dependencyReport > deps.txt
e.g ./gradlew dependencyReport --configuration totalcontentblockReleaseRuntimeClasspath > deps.txt
It is recommended you filter the configuration down like so --configuration {variant}ReleaseRuntimeClasspath");
var reportPath = Console.ReadLine();
if (!File.Exists(reportPath))
{
    Console.Write("File not found. Exiting..");
    Environment.Exit(-1);
}
Console.WriteLine("Parsing file ..");
var dict = new Dictionary<string, bool>();
var pomReolvers = new List<Task<ReportLine>>();
var httpClient = new HttpClient();
using (var sr = new StreamReader(reportPath))
{
    string? line;
    while ((line = sr.ReadLine()) != null)
    {
        if (!line.Contains(':'))
            continue;
        var cleaned = line.Replace(" ", "").Replace("|", "").Replace("\\", "").Replace("+", "").Replace("---", "").Replace("(*)", "").Replace("(c)", "").Replace("(n)", "");
        var arry = cleaned.Split(":");
        if (arry[1].Contains("->"))
            continue;
        var reportLine = ReportLine.FromString(cleaned);
        if (reportLine == null)
            continue;
        if (!reportLine.moduleVersion.Any(Char.IsDigit))
            continue;
        reportLine.mavenPackageName += ":" + reportLine.moduleVersion;
        if (dict.ContainsKey(reportLine.mavenPackageName))
            continue;
        dict[reportLine.mavenPackageName] = false;
        pomReolvers.Add(PomFinder.Retrieve(reportLine, httpClient));
    }
}
Console.WriteLine("Waiting for pomResolvers ..");
Task.WaitAll([.. pomReolvers]);
Console.WriteLine("Writing report..");
using var sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "extracted.csv"));
sw.WriteLine("mavenPackageName,moduleName,moduleUrl,moduleVersion,moduleLicense,moduleLicenseUrl,pomUrl");
var list = new List<ReportLine>();
foreach (var reportTask in pomReolvers)
{
    var r = reportTask.Result;
    sw.WriteLine($"{r.mavenPackageName},{r.moduleName},,{r.moduleVersion},{r.moduleLicense},{r.moduleLicenseUrl},{r.pomUrl}");
    list.Add(r);
}

//write licenses.json file
var sorted = list.OrderBy(p => p.moduleName).ToList();
string fileName = Path.Combine(Environment.CurrentDirectory, "licenses.json");
using FileStream createStream = File.Create(fileName);
var jsonSerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true };
JsonSerializer.Serialize(createStream, sorted, options: jsonSerOptions);
Console.WriteLine("All Done");