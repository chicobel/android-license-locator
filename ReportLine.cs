namespace LicenseLocator;

public class ReportLine
{
    public string moduleName { get; set; }
    public string mavenPackageName { get; set; }
    public string moduleUrl { get; set; }
    public string moduleVersion { get; set; }
    public string moduleLicense { get; set; }
    public string moduleLicenseUrl { get; set; }
    public string pomUrl { get; set; }
    public string groupId, artifactId, version;

    public static ReportLine? FromString(string mavenFormatedPackage)
    {
        var arry = mavenFormatedPackage.Split(":");
        if (arry.Length < 3)
            return null;
        var v = arry.LastOrDefault()??"";
        if(v.Contains("->"))
            v = v.Split("->").Last();
        var rl = new ReportLine
        {
            groupId = arry[0],
            artifactId = arry[1],
            version = v,
            moduleName = arry[1],
            mavenPackageName = string.Join(":", arry.Take(arry.Length - 1)),
            moduleVersion = v
        };
        return rl;
    }
    string GetVersion()
    {
        var clean = version.Replace("[]", "");
        var tokens = clean.Split(",");
        Array.Sort(tokens);
        return tokens[tokens.Length - 1];
    }
    string GroupIdAsPath() => groupId.Replace(".", "/");
    public string PomPath() => $"{GroupIdAsPath()}/{artifactId}/{GetVersion()}/{artifactId}-{GetVersion()}.pom";
}