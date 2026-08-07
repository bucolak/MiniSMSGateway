using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class GetSettings
    {
        public Credential Credential { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class GetSettingsResponse
    {
        public GetSettingsInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class GetSettingsInfo
    {
        public GetSettingsInfo()
        {
            Settings = new Settings();
            Status = new Status();
        }

        public Settings Settings { get; set; }

        public Status Status { get; set; }
    }
}