using Microsoft.Extensions.Options;
using TechnicalTest_Profescipta.Common.ConfigurationModel;

namespace TechnicalTest_Profescipta.Common.Library
{
    public class AppServicesHelper
    {
        static IServiceProvider services = null;

        public static IServiceProvider Services
        {
            get { return services; }
            set
            {
                if (services != null)
                {
                    throw new Exception("Can't set once a value has already been set.");
                }
                services = value;
            }
        }

        public static AppConnectionString getConnetionString
        {
            get
            {
                //This works to get file changes.
                var s = services.GetService(typeof(IOptionsMonitor<AppConnectionString>)) as IOptionsMonitor<AppConnectionString>;
                AppConnectionString config = s.CurrentValue;

                return config;
            }
        }
    }
}
