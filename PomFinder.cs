using System.Xml;

namespace LicenseLocator;

public static class PomFinder
{

    public static async Task<ReportLine> Retrieve(ReportLine r, HttpClient client)
    {
        var taskList = new List<Task<HttpResponseMessage>>();
        var likelyUnderGoogle = r.mavenPackageName.IndexOf("androidx", StringComparison.InvariantCultureIgnoreCase) >-1 || r.mavenPackageName.IndexOf("google", StringComparison.InvariantCultureIgnoreCase) > -1;
        var repoList = likelyUnderGoogle ? Repos.GoogePreferredRepos : Repos.DefaultRepos;
        foreach (var repo in repoList)
        {
            var resp = client.GetAsync(repo + "/" + r.PomPath());
            taskList.Add(resp);
        }
        await Task.WhenAll([..taskList]);
        foreach (var tsk in taskList)
        {
            if(!tsk.Result.IsSuccessStatusCode)
                continue;
            string pomData = await tsk.Result.Content.ReadAsStringAsync();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(pomData);
                if(doc.DocumentElement == null)
                    continue;
                XmlNode? licenseNode = null;
                foreach (XmlNode element in doc.DocumentElement.ChildNodes)
                {
                    if (element.Name == "licenses")
                    {
                        licenseNode = element.FirstChild;
                        break;
                    }
                }
                if (licenseNode == null)
                    continue;
                r.moduleLicense = licenseNode.ChildNodes[0]?.InnerText??"";
                r.moduleLicenseUrl = licenseNode.ChildNodes[1]?.InnerText??"";
                if (!string.IsNullOrEmpty(r.moduleLicense))
                {
                    r.pomUrl = tsk.Result.RequestMessage?.RequestUri?.ToString() ?? "";
                    return r;
                }
            }
            catch (Exception)
            {
                Console.Write("Failed to parse xml for: " + tsk.Result.RequestMessage?.RequestUri + "\n");
            }
            // Console.Write($"Response for {r.mavenPackageName}: \n {pomData} \n");
        }
        return r;
    }

}
