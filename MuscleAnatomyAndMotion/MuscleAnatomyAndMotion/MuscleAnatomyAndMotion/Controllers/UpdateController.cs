using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion.Controllers
{
    public interface IUpdateController
    {
        string getCurrentVersion();
    }
    public static class UpdateController
    {
        public static async Task<string> getUpdateTargetVersion()
        {
            return await DependencyService.Get<IRemoteConfigurationService>().GetAsync("update_target_version");
        }
        public static async Task<string> getUpdateUrl()
        {
            return await DependencyService.Get<IRemoteConfigurationService>().GetAsync("update_url");
        }
        public static async Task<string> getUpdateDescription()
        {
            return await DependencyService.Get<IRemoteConfigurationService>().GetAsync("update_description");
        }
        public static async Task<bool> Validate()
        {
            var version = await getUpdateTargetVersion();
            var currentVersion = DependencyService.Get<IUpdateController>().getCurrentVersion();
            return currentVersion != version && currentVersion != null;
        }
        public static async Task<string> getValidatedUpdateUrl()
        {
            if (await Validate())
            {
                var update_url = await getUpdateUrl();
                return update_url;
            }
            return null;
        }
        public static async Task<string> getValidatedUpdateDescription()
        {
            if (await Validate())
            {
                var update_description = await getUpdateDescription();
                return update_description;
            }
            return null;
        }
    }
}
