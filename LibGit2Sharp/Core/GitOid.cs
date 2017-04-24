using System;
using System.Runtime.InteropServices;

namespace LibGit2Sharp.Core
{
    internal struct git_oid
    {
        public const int Size = 20;
        public unsafe fixed byte Id[20];
    }

    /// <summary>
    /// Represents a unique id in git which is the sha1 hash of this id's content.
    /// </summary>
    internal struct GitOid
    {
        /// <summary>
        /// Number of bytes in the Id.
        /// </summary>
        public const int Size = 20;

        /// <summary>
        /// The raw binary 20 byte Id.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Size)]
        public byte[] Id;

        public static implicit operator ObjectId(GitOid oid)
        {
            return new ObjectId(oid);
        }

        public static implicit operator ObjectId(GitOid? oid)
        {
            return oid == null ? null : new ObjectId(oid.Value);
        }

        internal static unsafe GitOid BuildFromPtr(IntPtr ptr)
        {
            return BuildFromPtr((git_oid*)ptr.ToPointer());
        }

        internal static unsafe GitOid BuildFromPtr(git_oid* id)
        {
            return id == null? Empty : new GitOid(id->Id);
        }

        internal unsafe GitOid(byte* rawId)
        {
            var id = new byte[Size];

            fixed(byte* p = id)
            {
                for (int i = 0; i < Size; i++)
                {
                    p[i] = rawId[i];
                }
            }

            Id = id;
        }

        /// <summary>
        /// Static convenience property to return an id (all zeros).
        /// </summary>
        public static GitOid Empty
        {
            get { return new GitOid(); }
        }
    }
}
