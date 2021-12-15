using CodectoryCore.Logging;
using System;

namespace AutoHDR.Profiles.Actions
{
    public interface IProfileAction
    {
        EventHandler<LogEntry> NewLog { get; set; }

        string ActionDescription { get;}
        string ActionTypeName { get; }
        ActionEndResult RunAction(params object[] parameter);
    }
}
