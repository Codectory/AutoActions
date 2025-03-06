using CodectoryCore.Logging;
using System;

namespace AutoActions.Profiles.Actions
{
    public interface IAction
    {
        EventHandler<LogEntry> NewLog { get; set; }
        string ActionDescription { get; }
        string ActionTypeName { get; }

        bool CanSave { get; }
        string CannotSaveMessage { get; }

        ActionEndResult RunAction(ApplicationChangedType applicationChangedType);

    }
}

