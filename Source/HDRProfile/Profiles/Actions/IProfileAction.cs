namespace AutoHDR.Profiles.Actions
{
    public interface IProfileAction
    {
        string ActionDescription { get;}
        string ActionTypeName { get; }
        ActionEndResult RunAction(params object[] parameter);
    }
}
