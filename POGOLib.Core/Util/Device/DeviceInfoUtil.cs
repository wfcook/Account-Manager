using System;
using POGOLib.Official.Extensions;
using static POGOProtos.Networking.Envelopes.Signature.Types;

namespace POGOLib.Official.Util.Device
{
    public class DeviceInfoUtil
    {
        private static readonly Random Random = new Random();


        private static readonly string[][] OsVars = { 
            new [] {"9.0", "CFNetwork/758.0.2 Darwin/15.0.0"},        // Index 0
            new [] {"9.0.1","CFNetwork/758.0.2 Darwin/15.0.0"},       // Index 1
                new [] {"9.0.2","CFNetwork/758.0.2 Darwin/15.0.0"},   // Index 2
                new [] {"9.1","CFNetwork/758.1.6 Darwin/15.0.0"},     // Index 3
                new [] {"9.2","CFNetwork/758.2.8 Darwin/15.0.0"},     // Index 4
                new [] {"9.2.1","CFNetwork/758.2.8 Darwin/15.0.0"},   // Index 5
                new [] {"9.3","CFNetwork/758.3.15 Darwin/15.4.0"},    // Index 6
                new [] {"9.3.2","CFNetwork/758.4.3 Darwin/15.5.0"},   // Index 7
                new [] {"10.3.3","CFNetwork/807.2.14 Darwin/16.3.0"}, // Index 8
                new [] {"11.1.0","CFNetwork/889.3 Darwin/17.2.0"},    // Index 9
                new [] {"11.2.0","CFNetwork/893.10 Darwin/17.3.0"}    // Index 10
        };
        // NOTEs:
        //        pos 3 = index of first valid OS
        //        pos 4 = index of last valid OS
        private static readonly string[][] Devices =
        {
            new[] {"iPad5,1", "iPad", "J96AP","0","10"},     // iPad 4 mini
            new[] {"iPad5,2", "iPad", "J97AP","0","10"},     // iPad 4 mini
            new[] {"iPad5,3", "iPad", "J81AP","0","10"},     // iPad Air 2
            new[] {"iPad5,4", "iPad", "J82AP","0","10"},     // iPad Air 2
            new[] {"iPad6,7", "iPad", "J98aAP","3","10"},    // iPad Pro (12.9-inch)
            new[] {"iPad6,8", "iPad", "J99aAP","3","10"},    // iPad Pro (12.9-inch)
            new[] {"iPhone5,1", "iPhone", "N41AP","0","8"},  // iPhone 5
            new[] {"iPhone5,2", "iPhone", "N42AP","0","8"},  // iPhone 5
            new[] {"iPhone5,3", "iPhone", "N48AP","0","8"},  // iPhone 5c
            new[] {"iPhone5,4", "iPhone", "N49AP","0","8"},  // iPhone 5c
            new[] {"iPhone6,1", "iPhone", "N51AP","0","10"}, // iPhone 5s
            new[] {"iPhone6,2", "iPhone", "N53AP","0","10"}, // iPhone 5s
            new[] {"iPhone7,1", "iPhone", "N56AP","8","10"}, // iPhone 6 Plus
            new[] {"iPhone7,2", "iPhone", "N61AP","8","10"}, // iPhone 6
            new[] {"iPhone8,1", "iPhone", "N71AP","8","10"}, // iPhone 6s
            new[] {"iPhone8,2", "iPhone", "MKTM2","8","10"}, // iPhone 6s plus
            new[] {"iPhone9,3", "iPhone", "MN9T2","8","10"}  // iPhone 7
        };



        public static DeviceWrapper GetRandomDevice()
        {
            var device = Devices[Random.Next(Devices.Length)];
            var startIndex = int.Parse(device[3]);
            var LastIndex = int.Parse(device[4]);

            var osIndex = Random.Next(startIndex, LastIndex +1);
            var firmwareType = OsVars[osIndex][0];
            var firmwareUserAgentPart = OsVars[osIndex][1];

            return new DeviceWrapper
            {
                UserAgent = $"pokemongo/1 {firmwareUserAgentPart}",
                DeviceInfo = new DeviceInfo
                {
                    DeviceId = Random.NextHexNumber(32).ToLower(),
                    DeviceBrand = "Apple",
                    DeviceModelBoot = device[0],
                    DeviceModel = device[1],
                    HardwareModel = device[2],
                    HardwareManufacturer = "Apple",
                    FirmwareBrand = "iPhone OS",
                    FirmwareType = firmwareType,
                }
            };
        }
    }
}
