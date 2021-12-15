namespace AutoHDR.Threading
{
    public interface IManagedThread
    {
        bool ManagedThreadIsActive { get; }
        void StartManagedThread();
        void StopManagedThread();
    }
}