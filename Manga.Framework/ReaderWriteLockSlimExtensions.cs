using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Manga.Framework
{
    public static class ReaderWriteLockExtensions
    {
        private struct Disposable : IDisposable
        {
            private readonly Action m_action;
            private Sentinel m_sentinel;

            public Disposable(Action action)
            {
                m_action = action;
                m_sentinel = new Sentinel();
            }

            public void Dispose()
            {
                m_action();
                GC.SuppressFinalize(m_sentinel);
            }
        }

        private class Sentinel
        {
            ~Sentinel()
            {
                throw new InvalidOperationException("Lock not properly disposed.");
            }
        }

        public static IDisposable AcquireReadLock(this ReaderWriterLockSlim _lock)
        {
            _lock.EnterReadLock();
            return new Disposable(_lock.ExitReadLock);
        }

        public static IDisposable AcquireUpgradableReadLock(this ReaderWriterLockSlim _lock)
        {
            _lock.EnterUpgradeableReadLock();
            return new Disposable(_lock.ExitUpgradeableReadLock);
        }

        public static IDisposable AcquireWriteLock(this ReaderWriterLockSlim _lock)
        {
            _lock.EnterWriteLock();
            return new Disposable(_lock.ExitWriteLock);
        }
    }
}
