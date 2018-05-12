using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    public abstract class ConfigurationIterator<T> where T : class
    {
        /// <summary>
        /// Invoked by libgit2 when this backend is no longer needed.
        /// </summary>
        internal void Free()
        {
            if (nativeBackendPointer == IntPtr.Zero)
            {
                return;
            }

            GCHandle.FromIntPtr(Marshal.ReadIntPtr(nativeBackendPointer, GitOdbBackend.GCHandleOffset)).Free();
            Marshal.FreeHGlobal(nativeBackendPointer);
            nativeBackendPointer = IntPtr.Zero;
        }

        /// <summary>
        /// The implimentation of this method should out the current entry and advance the iterator.
        /// </summary>
        /// <param name="configurationEntry"></param>
        /// <returns></returns>
        public abstract int Next(out ConfigurationEntry<T> configurationEntry);

        private IntPtr nativeBackendPointer;

        internal IntPtr GitConfigBackendPointer
        {
            get
            {
                if (IntPtr.Zero == nativeBackendPointer)
                {
                    var nativeBackend = new GitConfigIterator();

                    nativeBackend.Next = IteratorEntryPoints.NextCallback;
                    nativeBackend.Free = IteratorEntryPoints.FreeCallback;

                    nativeBackend.GCHandle = GCHandle.ToIntPtr(GCHandle.Alloc(this));
                    nativeBackendPointer = Marshal.AllocHGlobal(Marshal.SizeOf(nativeBackend));
                    Marshal.StructureToPtr(nativeBackend, nativeBackendPointer, false);
                }

                return nativeBackendPointer;
            }
        }

        private static class IteratorEntryPoints
        {
            // Because our GitOdbBackend structure exists on the managed heap only for a short time (to be marshaled
            // to native memory with StructureToPtr), we need to bind to static delegates. If at construction time
            // we were to bind to the methods directly, that's the same as newing up a fresh delegate every time.
            // Those delegates won't be rooted in the object graph and can be collected as soon as StructureToPtr finishes.

            public static readonly GitConfigIterator.next_callback NextCallback = Next;
            public static readonly GitConfigIterator.free_callback FreeCallback = Free;

            private static ConfigurationIterator<T> MarshalConfigurationIterator(IntPtr iteratorPtr)
            {

                var intPtr = Marshal.ReadIntPtr(iteratorPtr, GitConfigIterator.GCHandleOffset);
                var configBackend = GCHandle.FromIntPtr(intPtr).Target as ConfigurationIterator<T>;

                if (configBackend == null)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, "Cannot retrieve the managed ConfigurationIterator.");
                    return null;
                }

                return configBackend;
            }

            private static int Next(out GitConfigEntry configEntry, IntPtr iterator)
            {
                configEntry = default(GitConfigEntry);

                var configIterator = MarshalConfigurationIterator(iterator);
                if (configIterator == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    ConfigurationEntry<T> configurationEntry;
                    configIterator.Next(out configurationEntry);

                    configEntry = configurationEntry.GetGitConfigEntry();
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                }

                return (int)GitErrorCode.Ok;
            }

            private static void Free(IntPtr iterator)
            {
                var configIterator = MarshalConfigurationIterator(iterator);
                if (configIterator == null)
                {
                    return;
                }

                try
                {
                    configIterator.Free();
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                }
            }
        }
    }
}
