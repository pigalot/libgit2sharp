using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    public abstract class ConfigBackend
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
        /// In your subclass, override this member to provide the list of actions your backend supports.
        /// </summary>
        protected abstract ConfigBackendOperations SupportedOperations { get; }

        public abstract int Open(LogLevel level);

        public abstract int Get(string key, out ConfigurationEntry<string> configurationEntry);

        public abstract int Set(string key, string value);

        public abstract int SetMultivar(string Name, string regexp, string value);

        public abstract int Del(string key);

        public abstract int DelMultivar(string Name, string regexp);

        public abstract int Iterator(out object iterator); // ToDo: impliment iterator

        public abstract int Snapshot(out ConfigBackend configSnapshot);

        public abstract int Lock();

        public abstract int Unlock(out bool success);

        private IntPtr nativeBackendPointer;

        internal IntPtr GitConfigBackendPointer
        {
            get
            {
                if (IntPtr.Zero == nativeBackendPointer)
                {
                    var nativeBackend = new GitConfigBackend();
                    nativeBackend.Version = 1;

                    // The "free" entry point is always provided.
                    nativeBackend.Free = BackendEntryPoints.FreeCallback;

                    var supportedOperations = this.SupportedOperations;

                    if ((supportedOperations & ConfigBackendOperations.Open) != 0)
                    {
                        nativeBackend.Open = BackendEntryPoints.OpenCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Get) != 0)
                    {
                        nativeBackend.Get = BackendEntryPoints.GetCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Set) != 0)
                    {
                        nativeBackend.Set = BackendEntryPoints.SetCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.SetMultivar) != 0)
                    {
                        nativeBackend.SetMultivar = BackendEntryPoints.SetMultivarCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Del) != 0)
                    {
                        nativeBackend.Del = BackendEntryPoints.DelCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.DelMultivar) != 0)
                    {
                        nativeBackend.DelMultivar = BackendEntryPoints.DelMultivarCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Iterator) != 0)
                    {
                        nativeBackend.Iterator = BackendEntryPoints.IteratorCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Snapshot) != 0)
                    {
                        nativeBackend.Snapshot = BackendEntryPoints.SnapshotCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Lock) != 0)
                    {
                        nativeBackend.Lock = BackendEntryPoints.LockCallback;
                    }

                    if ((supportedOperations & ConfigBackendOperations.Unlock) != 0)
                    {
                        nativeBackend.Unlock = BackendEntryPoints.UnlockCallback;
                    }

                    nativeBackend.GCHandle = GCHandle.ToIntPtr(GCHandle.Alloc(this));
                    nativeBackendPointer = Marshal.AllocHGlobal(Marshal.SizeOf(nativeBackend));
                    Marshal.StructureToPtr(nativeBackend, nativeBackendPointer, false);
                }

                return nativeBackendPointer;
            }
        }

        private static class BackendEntryPoints
        {
            // Because our GitOdbBackend structure exists on the managed heap only for a short time (to be marshaled
            // to native memory with StructureToPtr), we need to bind to static delegates. If at construction time
            // we were to bind to the methods directly, that's the same as newing up a fresh delegate every time.
            // Those delegates won't be rooted in the object graph and can be collected as soon as StructureToPtr finishes.
            public static readonly GitConfigBackend.open_callback OpenCallback = Open;
            public static readonly GitConfigBackend.get_callback GetCallback = Get;
            public static readonly GitConfigBackend.set_callback SetCallback = Set;
            public static readonly GitConfigBackend.set_multivar_callback SetMultivarCallback = SetMultivar;
            public static readonly GitConfigBackend.del_callback DelCallback = Del;
            public static readonly GitConfigBackend.del_multivar_callback DelMultivarCallback = DelMultivar;
            public static readonly GitConfigBackend.iterator_callback IteratorCallback = Iterator;
            public static readonly GitConfigBackend.snapshot_callback SnapshotCallback = Snapshot;
            public static readonly GitConfigBackend.lock_callback LockCallback = Lock;
            public static readonly GitConfigBackend.unlock_callback UnlockCallback = Unlock;
            public static readonly GitConfigBackend.free_callback FreeCallback = Free;

            private static ConfigBackend MarshalConfigBackend(IntPtr backendPtr)
            {

                var intPtr = Marshal.ReadIntPtr(backendPtr, GitConfigBackend.GCHandleOffset);
                var configBackend = GCHandle.FromIntPtr(intPtr).Target as ConfigBackend;

                if (configBackend == null)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, "Cannot retrieve the managed ConfigBackend.");
                    return null;
                }

                return configBackend;
            }

            private static int Open(IntPtr backend, LogLevel level)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    int toReturn = configBackend.Open(level);

                    if (toReturn != 0)
                    {
                        return toReturn;
                    }
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }
            
            private static unsafe int Get(IntPtr backend, string key, out GitConfigEntry entry)
            {
                entry = default(GitConfigEntry);

                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }
                
                try
                {
                    ConfigurationEntry<string> configurationEntry;
                    int toReturn = configBackend.Get(key, out configurationEntry);

                    if (toReturn != 0)
                    {
                        return toReturn;
                    }

                    var namePtr = EncodingMarshaler.FromManaged(Encoding.UTF8, configurationEntry.Key);
                    var valuePtr = EncodingMarshaler.FromManaged(Encoding.UTF8, configurationEntry.Value);
                    var level = (uint)configurationEntry.Level;

                    entry = new GitConfigEntry { namePtr = (char*)namePtr, valuePtr = (char*)valuePtr, level = level };
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }
            
            private static int Set(IntPtr backend, string key, string value)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    return configBackend.Set(key, value);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }
            }
            
            private static int SetMultivar(IntPtr backend, string name, string regexp, string value)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    return configBackend.SetMultivar(name, regexp, value);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }
            }
            
            private static int Del(IntPtr backend, string key)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    return configBackend.Del(key);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }
            }
            
            private static int DelMultivar(IntPtr backend, string name, string regexp)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    return configBackend.DelMultivar(name, regexp);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }
            }
            
            private static int Iterator(out IntPtr iterator, IntPtr backend)
            {
                // Impliment iterator
                // https://github.com/libgit2/libgit2/blob/0d723f39decff4ff77def8a4acf3b7a656af59b2/include/git2/sys/config.h line 34
                throw new NotImplementedException();
            }
            
            private static int Snapshot(out IntPtr snapshot, IntPtr backend)
            {
                throw new NotImplementedException();
            }
            
            private static int Lock(IntPtr backend)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    return configBackend.Lock();
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }
            }
            
            private static int Unlock(IntPtr backend, out int success)
            {
                success = 0;
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    bool suc;
                    int toReturn = configBackend.Unlock(out suc);

                    if (toReturn != 0)
                    {
                        return toReturn;
                    }

                    success = suc ? 1 : 0;
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }
            
            private static void Free(IntPtr backend)
            {
                ConfigBackend configBackend = MarshalConfigBackend(backend);
                if (configBackend == null)
                {
                    return;
                }

                try
                {
                    configBackend.Free();
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Config, ex);
                }
            }
        }

        /// <summary>
        /// Flags used by subclasses of ConfigBackend to indicate which operations they support.
        /// </summary>
        [Flags]
        protected enum ConfigBackendOperations
        {
            /// <summary>
            /// This ConfigBackend declares that it supports the Open method.
            /// </summary>
            Open = 1,

            /// <summary>
            /// This ConfigBackend declares that it supports the Get method.
            /// </summary>
            Get = 2,

            /// <summary>
            /// This ConfigBackend declares that it supports the Set method.
            /// </summary>
            Set = 4,

            /// <summary>
            /// This ConfigBackend declares that it supports the SetMultivar method.
            /// </summary>
            SetMultivar = 8,

            /// <summary>
            /// This ConfigBackend declares that it supports the Del method.
            /// </summary>
            Del = 16,

            /// <summary>
            /// This ConfigBackend declares that it supports the DelMultivar method.
            /// </summary>
            DelMultivar = 32,

            /// <summary>
            /// This ConfigBackend declares that it supports the Iterator method.
            /// </summary>
            Iterator = 64,

            /// <summary>
            /// This ConfigBackend declares that it supports the Snapshot method.
            /// </summary>
            Snapshot = 128,

            /// <summary>
            /// This ConfigBackend declares that it supports the Lock method.
            /// </summary>
            Lock = 256,

            /// <summary>
            /// This ConfigBackend declares that it supports the Unlock method.
            /// </summary>
            Unlock = 512,

            /// <summary>
            /// This ConfigBackend declares that it supports the Free method.
            /// </summary>
            Free = 1024
        }
    }
}
