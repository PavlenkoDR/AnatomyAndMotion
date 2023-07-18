using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MuscleAnatomyAndMotion.Controllers
{
    public interface IRemoteConfigurationService
    {
        Task FetchAndActivateAsync();
        Task<TInput> GetAsync<TInput>(string key);
        Task<string> GetAsync(string key);
    }
}
