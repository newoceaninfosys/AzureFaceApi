
namespace Face.Core.Settings
{
    public class GlobalAppSetting
    {
        public FaceSettingOption FaceSetting { get; set; }

        public class FaceSettingOption
        {
            public string SubscriptionKey { get; set; }
            public string ApiEndpoint { get; set; }
        }
    }
}
