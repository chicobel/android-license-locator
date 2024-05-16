namespace LicenseLocator;

public static class Repos
{
    public static string[] DefaultRepos {get; set;} = [
	"https://repo.maven.apache.org/maven2",
	"https://dl.google.com/dl/android/maven2",
	"https://plugins.gradle.org/m2",
	"https://jitpack.io",
    ];

     public static string[] GoogePreferredRepos {get; set;} = [
        "https://dl.google.com/dl/android/maven2",
	    "https://repo.maven.apache.org/maven2",
	    "https://plugins.gradle.org/m2",
	    "https://jitpack.io",
    ];

}