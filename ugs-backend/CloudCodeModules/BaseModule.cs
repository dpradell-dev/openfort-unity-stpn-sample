using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudCodeModules;

public abstract class BaseModule
{
    public class ModuleConfig : ICloudCodeSetup
    {
        public virtual void Setup(ICloudCodeConfig config)
        {
            config.Dependencies.AddSingleton(GameApiClient.Create());
            config.Dependencies.AddSingleton(PushClient.Create());
        }
    }
}