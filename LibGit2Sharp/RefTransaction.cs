﻿using System;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp
{
    /// <summary>
    /// 
    /// </summary>
    public class RefTransaction : IDisposable
    {
        TransactionHandle transactionHandle;
        Repository repo;

        protected RefTransaction()
        { }

        internal RefTransaction(Repository repository)
        {
            repo = repository;
            transactionHandle = Proxy.git_transaction_new(repository.Handle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public virtual void LockReference(Reference reference)
        {
            if (repo.Refs[reference.CanonicalName] == null)
            {
                throw new NotFoundException(string.Format("Reference {0} no longer exists.", reference.CanonicalName));
            }

            Proxy.git_transaction_lock_ref(this.transactionHandle, reference.CanonicalName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public virtual void RemoveReference(Reference reference)
        {
            Proxy.git_transaction_remove(this.transactionHandle, reference.CanonicalName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directRef"></param>
        /// <param name="targetId"></param>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public virtual void UpdateTarget(Reference directRef, ObjectId targetId, string logMessage)
        {
            Ensure.ArgumentNotNull(directRef, "directRef");
            Ensure.ArgumentNotNull(targetId, "targetId");

            Identity ident = Proxy.git_repository_ident(repo.Handle);

            Proxy.git_transaction_set_target(this.transactionHandle, directRef.CanonicalName, targetId.Oid, ident, logMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbolicRef"></param>
        /// <param name="targetRef"></param>
        /// <param name="logMessage"></param>
        public virtual void UpdateTarget(Reference symbolicRef, Reference targetRef, string logMessage)
        {
            Identity ident = Proxy.git_repository_ident(repo.Handle);
            Proxy.git_transaction_set_symbolic_target(this.transactionHandle, symbolicRef.CanonicalName, targetRef.CanonicalName, ident, logMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Commit()
        {
            Proxy.git_transaction_commit(this.transactionHandle);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    transactionHandle.SafeDispose();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RefTransaction() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
