using System;
using System.Runtime.InteropServices;

namespace LibGit2Sharp.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitConfigIterator
    {
        static GitConfigIterator()
        {
            GCHandleOffset = Marshal.OffsetOf(typeof(GitConfigIterator), "GCHandle").ToInt32();
        }

        public GitConfigBackend Backend;
        public uint Flags;

        public next_callback Next;
        public free_callback Free;

        /* The libgit2 structure definition ends here. Subsequent fields are for libgit2sharp bookkeeping. */

        public IntPtr GCHandle;

        public static int GCHandleOffset;

        public delegate int next_callback(
            out GitConfigEntry configEntry,
            IntPtr iterator);

        public delegate void free_callback(
            IntPtr iterator);
    }
}
