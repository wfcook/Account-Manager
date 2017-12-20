using System.Collections.Generic;
using PokemonGoGUI.Enums;
using System;

namespace PokemonGoGUI
{
    public interface ISettings
    {
        AuthType AuthType { get; set; }
        double DefaultLatitude { get; set; }
        double DefaultLongitude { get; set; }
        double DefaultAltitude { get; set; }
        string Password { get; set; }
        string Username { get; set; }
        string DeviceId { get; set; }
        string AndroidBoardName { get; set; }
        string AndroidBootloader { get; set; }
        string DeviceBrand { get; set; }
        string DeviceModel { get; set; }
        string DeviceModelIdentifier { get; set; }
        string DeviceModelBoot { get; set; }
        string HardwareManufacturer { get; set; }
        string HardwareModel { get; set; }
        string FirmwareBrand { get; set; }
        string FirmwareTags { get; set; }
        string FirmwareType { get; set; }
        string FirmwareFingerprint { get; set; }
        string ProxyIP { get; set; }
        int ProxyPort { get; set; }
        string ProxyUsername { get; set; }
        string ProxyPassword { get; set; }
        bool SPF { get; set; }

        List<string> HashKeys { get; set; }
        bool UseOnlyOneKey { get; set; }
        string AuthAPIKey { get; set; }
        Uri HashHost { get; set; }
        string HashEndpoint { get; set; }
        string Country { get; set; }
        string Language { get; set; }
        string TimeZone { get; set; }
        string POSIX { get; set; }
    }
}