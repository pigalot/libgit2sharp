using System;
using System.Runtime.InteropServices;

namespace LibGit2Sharp.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitConfigBackend
    {
        static GitConfigBackend()
        {
            GCHandleOffset = MarshalPortable.OffsetOf<GitConfigBackend>(nameof(GCHandle)).ToInt32();
        }

        public uint Version;

        public int ReadOnly;

#pragma warning disable 169

        /// <summary>
        /// This field is populated by libgit2 at backend addition time, and exists for its
        /// use only. From this side of the interop, it is unreferenced.
        /// </summary>
        private readonly IntPtr Cfg;

#pragma warning restore 169

        public open_callback Open;
        public get_callback Get;
        public set_callback Set;
        public set_multivar_callback SetMultivar;
        public del_callback Del;
        public del_multivar_callback DelMultivar;
        public iterator_callback Iterator;
        public snapshot_callback Snapshot;
        public lock_callback Lock;
        public unlock_callback Unlock;
        public free_callback Free;

        /* The libgit2 structure definition ends here. Subsequent fields are for libgit2sharp bookkeeping. */

        public IntPtr GCHandle;

        public static int GCHandleOffset;

        public delegate int open_callback(
            IntPtr backend,
            uint level);

        public delegate int get_callback(
            IntPtr backend,
            string key,
            out GitConfigEntry entry);

        public delegate int set_callback(
            IntPtr backend,
            string key,
            string value);

        public delegate int set_multivar_callback(
            IntPtr backend,
            string name,
            string regexp,
            string value);

        public delegate int del_callback(
            IntPtr backend,
            string key);

        public delegate int del_multivar_callback(
            IntPtr backend,
            string name,
            string regexp);

        public delegate int iterator_callback(
            out IntPtr iterator,
            IntPtr backend);

        public delegate int snapshot_callback(
            out IntPtr snapshot,
            IntPtr backend);

        public delegate int lock_callback(
            IntPtr backend);

        public delegate int unlock_callback(
            IntPtr backend,
            out int success);

        public delegate void free_callback(
            IntPtr backend);
    }
}
