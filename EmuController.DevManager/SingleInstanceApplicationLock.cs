using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace EmuController.DevManager
{
    //https://codereview.stackexchange.com/a/141141
    sealed class SingleInstanceApplicationLock : IDisposable
    {
        ~SingleInstanceApplicationLock()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryAcquireExclusiveLock()
        {
            try
            {
                if (!Mutex.WaitOne(1000, false))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Multiple instances
                // may be executed in this condition...
            }

            return HasAcquiredExclusiveLock = true;
        }

        private const string MutexId = @"Local\{E03FD5E9-C97A-426E-BD6A-E56E5D7AC5B6}";
        private readonly Mutex Mutex = CreateMutex();
        private bool HasAcquiredExclusiveLock, Disposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !Disposed && Mutex != null)
            {
                try
                {
                    if (HasAcquiredExclusiveLock)
                        Mutex.ReleaseMutex();

                    Mutex.Dispose();
                }
                finally
                {
                    Disposed = true;
                }
            }
        }

        private static Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var allowEveryoneRule = new MutexAccessRule(sid,
                MutexRights.FullControl, AccessControlType.Allow);

            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            var mutex = new Mutex(false, MutexId);
            mutex.SetAccessControl(securitySettings);

            return mutex;
        }
    }
}
