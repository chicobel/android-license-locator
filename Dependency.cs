namespace LicenseLocator;

public struct Dependency  {
	public string GroupId {get; set;}
	public string ArtifactId {get; set;}
	public string Version {get; set;}
	public string Scope {get; set;}
	public string Optional {get; set;}
	/* Indirect dependencies */
	public bool Transitive {get; set;}

    public static Dependency FromString(string mavenFormatedPackage)
    {
        var arry = mavenFormatedPackage.Split(":");
        return new Dependency{ GroupId= arry[0], ArtifactId = arry[1], Version= arry[2]};
    }
    public readonly string GetVersion()
    {
        var clean = Version.Replace("[]","");
        var tokens = clean.Split(",");
        Array.Sort(tokens);
        return tokens[tokens.Length-1];
    }
    public readonly string GroupIdAsPath() => GroupId.Replace(".","/");
    public readonly string PomPath() => $"{GroupIdAsPath()}/{ArtifactId}/{GetVersion()}/{ArtifactId}-{GetVersion()}.pom";
}