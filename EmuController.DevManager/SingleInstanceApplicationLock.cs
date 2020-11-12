// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
