
using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;

namespace ExtractCurrentTfsIteration
{
    public class TfsTeamConfigurationExtractor
    {

        public string ExtractMajorMinorReleaseValue(int prefixValue)
        {
#pragma warning disable 612,618
            TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer("http://rsaklfsvrgendev:8080/tfs");
#pragma warning restore 612,618

            // It was a special trick to get this ;)
            // USE : http://blogs.microsoft.co.il/shair/2012/05/28/tfs-api-part-47-vs11-manage-iterations-dates/ 
            // Build and debug to get the project uri string below

            var settingConfigService = tfs.GetService<TeamSettingsConfigurationService>();
            var configs = settingConfigService.GetTeamConfigurationsForUser(new[] { @"vstfs:///Classification/TeamProject/aab89ee8-3c29-45aa-89dc-bf412c73bbd9" });

            var iterationPath = string.Empty;

            foreach(TeamConfiguration config in configs)
            {
                // Access the actual configuration settings.
                TeamSettings ts = config.TeamSettings;

                iterationPath = ts.CurrentIterationPath;
            }

            // DEV2 SCRUM Project\Release 4\Sprint 1
            var parts = iterationPath.Split('\\');

            var major = 0;
            var minor = 0;

            if(parts.Length == 3)
            {
                major = ExtractNumber(parts[1]);
                minor = ExtractNumber(parts[2]);
            }

            // ie 0.4.1
            return prefixValue + "." + major + "." + minor;
        }

        private int ExtractNumber(string part)
        {
            var pos = part.IndexOf(" ", StringComparison.Ordinal);
            var tmp = part.Substring(pos + 1);
            if(!string.IsNullOrEmpty(tmp))
            {
                int result;
                Int32.TryParse(tmp, out result);
                return result;
            }

            return 0;
        }

    }
}
