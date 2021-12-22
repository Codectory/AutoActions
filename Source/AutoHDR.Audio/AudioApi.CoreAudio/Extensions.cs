using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    public static class Extensions
    {
        public const string GUID_REGEX = @"([a-fA-F0-9]{8}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{12})";

        internal static EDataFlow AsEDataFlow(this DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Playback:
                    return EDataFlow.Render;
                case DeviceType.Capture:
                    return EDataFlow.Capture;
                case DeviceType.All:
                    return EDataFlow.All;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        internal static DeviceType AsDeviceType(this EDataFlow dataFlow)
        {
            switch (dataFlow)
            {
                case EDataFlow.Render:
                    return DeviceType.Playback;
                case EDataFlow.Capture:
                    return DeviceType.Capture;
                case EDataFlow.All:
                    return DeviceType.All;
                default:
                    throw new ArgumentOutOfRangeException("dataFlow");
            }
        }

        internal static DeviceState AsDeviceState(this EDeviceState deviceState)
        {
            switch (deviceState)
            {
                case EDeviceState.Active:
                    return DeviceState.Active;
                case EDeviceState.Disabled:
                    return DeviceState.Disabled;
                case EDeviceState.NotPresent:
                    return DeviceState.NotPresent;
                case EDeviceState.Unplugged:
                    return DeviceState.Unplugged;
                case EDeviceState.All:
                    return DeviceState.All;
                default:
                    throw new ArgumentOutOfRangeException("deviceState");
            }
        }

        internal static EDeviceState AsEDeviceState(this DeviceState deviceState)
        {
            switch (deviceState)
            {
                case DeviceState.Active:
                    return EDeviceState.Active;
                case DeviceState.Disabled:
                    return EDeviceState.Disabled;
                case DeviceState.NotPresent:
                    return EDeviceState.NotPresent;
                case DeviceState.Unplugged:
                    return EDeviceState.Unplugged;
                case DeviceState.All:
                    return EDeviceState.All;
                default:
                    throw new ArgumentOutOfRangeException("deviceState");
            }
        }

        internal static ERole AsERole(this Role role)
        {
            switch (role)
            {
                case Role.Console:
                    return ERole.Console;
                case Role.Multimedia:
                    return ERole.Multimedia;
                case Role.Communications:
                    return ERole.Communications;
                default:
                    throw new ArgumentOutOfRangeException("role");
            }
        }

        internal static Role AsRole(this ERole role)
        {
            switch (role)
            {
                case ERole.Console:
                    return Role.Console;
                case ERole.Multimedia:
                    return Role.Multimedia;
                case ERole.Communications:
                    return Role.Communications;
                default:
                    throw new ArgumentOutOfRangeException("role");
            }
        }

        internal static IEnumerable<Guid> ExtractGuids(this string str)
        {
            var r = new Regex(GUID_REGEX);
            var matches = r.Matches(str);

            if (matches.Count == 0)
                throw new FormatException("String does not contain a valid Guid");

            foreach (var match in matches)
            {
                yield return new Guid(match.ToString());
            }

        }

    }
}