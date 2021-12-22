using System.Threading;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    public static class LockExtensions
    {


        public static bool AcquireWriteLockNonReEntrant(this ReaderWriterLockSlim @lock)
        {
            //Lock is already held, so signal that the caller should not release the lock
            if (@lock.IsWriteLockHeld)
                return false;

            @lock.EnterWriteLock();

            return true;
        }

        public static bool AcquireReadLockNonReEntrant(this ReaderWriterLockSlim @lock)
        {
            //Lock is already held, so signal that the caller should not release the lock
            if (@lock.IsReadLockHeld)
                return false;

            @lock.EnterReadLock();

            return true;
        }

    }
}
