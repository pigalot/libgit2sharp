using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
#if DESKTOP
using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;
using System.Threading;
using LibGit2Sharp.Core.Handles;

// ReSharper disable InconsistentNaming
namespace LibGit2Sharp.Core
{
    [OfferFriendlyInteropOverloads]
    internal static partial class NativeMethods
    {
        public const uint GIT_PATH_MAX = 4096;
        private const string libgit2 = NativeDllName.Name;

        // An object tied to the lifecycle of the NativeMethods static class.
        // This will handle initialization and shutdown of the underlying
        // native library.
        #pragma warning disable 0414
        private static readonly NativeShutdownObject shutdownObject;
        #pragma warning restore 0414

        static NativeMethods()
        {
            if (Platform.OperatingSystem == OperatingSystemType.Windows)
            {
                string nativeLibraryPath = GlobalSettings.GetAndLockNativeLibraryPath();

                string path = Path.Combine(nativeLibraryPath, Platform.ProcessorArchitecture);

                const string pathEnvVariable = "PATH";
                Environment.SetEnvironmentVariable(pathEnvVariable,
                    String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", path, Path.PathSeparator, Environment.GetEnvironmentVariable(pathEnvVariable)));
            }

            LoadNativeLibrary();
            shutdownObject = new NativeShutdownObject();
        }

        // Avoid inlining this method because otherwise mono's JITter may try
        // to load the library _before_ we've configured the path.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadNativeLibrary()
        {
            // Configure the OpenSSL locking on the true initialization
            // of the library.
            if (git_libgit2_init() == 1)
            {
                git_openssl_set_locking();
            }
        }

        // Shutdown the native library in a finalizer.
        private sealed class NativeShutdownObject
        {
            ~NativeShutdownObject()
            {
                git_libgit2_shutdown();
            }
        }

        [DllImport(libgit2)]
        internal static extern unsafe GitError* giterr_last();

        [DllImport(libgit2)]
        private static unsafe extern void giterr_set_str(
            GitErrorCategory error_class,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* errorString);

        [DllImport(libgit2)]
        internal static extern void giterr_set_oom();

        [DllImport(libgit2)]
        internal static extern unsafe UInt32 git_blame_get_hunk_count(git_blame* blame);

        [DllImport(libgit2)]
        internal static extern unsafe git_blame_hunk* git_blame_get_hunk_byindex(
            git_blame* blame, UInt32 index);

        [DllImport(libgit2)]
        private static extern unsafe int git_blame_file(
            out git_blame* blame,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path,
            git_blame_options options);

        [DllImport(libgit2)]
        internal static extern unsafe void git_blame_free(git_blame* blame);

        [DllImport(libgit2)]
        private static extern unsafe int git_blob_create_fromdisk(
            ref GitOid id,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path);

        [DllImport(libgit2)]
        private static extern unsafe int git_blob_create_fromworkdir(
            ref GitOid id,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* relative_path);

        [DllImport(libgit2)]
        private static extern unsafe int git_blob_create_fromstream(
            out IntPtr stream,
            git_repository* repositoryPtr,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* hintpath);

        [DllImport(libgit2)]
        internal static extern unsafe int git_blob_create_fromstream_commit(
            ref GitOid oid,
            IntPtr stream);

        [DllImport(libgit2)]
        private static extern unsafe int git_blob_filtered_content(
            GitBuf buf,
            git_object* blob,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* as_path,
            [MarshalAs(UnmanagedType.Bool)] bool check_for_binary_data);

        [DllImport(libgit2)]
        internal static extern unsafe IntPtr git_blob_rawcontent(git_object* blob);

        [DllImport(libgit2)]
        internal static extern unsafe Int64 git_blob_rawsize(git_object* blob);

        [DllImport(libgit2)]
        private static extern unsafe int git_branch_create_from_annotated(
            out git_reference* ref_out,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* branch_name,
            git_annotated_commit* target,
            [MarshalAs(UnmanagedType.Bool)] bool force);

        [DllImport(libgit2)]
        internal static extern unsafe int git_branch_delete(
            git_reference* reference);

        internal delegate int branch_foreach_callback(
            IntPtr branch_name,
            GitBranchType branch_type,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern void git_branch_iterator_free(
            IntPtr iterator);

        [DllImport(libgit2)]
        internal static extern int git_branch_iterator_new(
            out IntPtr iter_out,
            IntPtr repo,
            GitBranchType branch_type);

        [DllImport(libgit2)]
        private static extern unsafe int git_branch_move(
            out git_reference* ref_out,
            git_reference* reference,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* new_branch_name,
            [MarshalAs(UnmanagedType.Bool)] bool force);

        [DllImport(libgit2)]
        internal static extern int git_branch_next(
            out IntPtr ref_out,
            out GitBranchType type_out,
            IntPtr iter);

        [DllImport(libgit2)]
        private static extern unsafe int git_branch_remote_name(
            GitBuf buf,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* canonical_branch_name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_init(
            out git_rebase* rebase,
            git_repository* repo,
            git_annotated_commit* branch,
            git_annotated_commit* upstream,
            git_annotated_commit* onto,
            GitRebaseOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_open(
            out git_rebase* rebase,
            git_repository* repo,
            GitRebaseOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_rebase_operation_entrycount(
            git_rebase* rebase);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_rebase_operation_current(
            git_rebase* rebase);

        [DllImport(libgit2)]
        internal static extern unsafe git_rebase_operation* git_rebase_operation_byindex(
            git_rebase* rebase,
            UIntPtr index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_next(
            out git_rebase_operation* operation,
            git_rebase* rebase);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_commit(
            ref GitOid id,
            git_rebase* rebase,
            git_signature* author,
            git_signature* committer,
            IntPtr message_encoding,
            IntPtr message);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_abort(
            git_rebase* rebase);

        [DllImport(libgit2)]
        internal static extern unsafe int git_rebase_finish(
            git_rebase* repo,
            git_signature* signature);

        [DllImport(libgit2)]
        internal static extern unsafe void git_rebase_free(git_rebase* rebase);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_rename(
            ref GitStrArray problems,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* old_name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* new_name);

        private unsafe delegate int git_remote_rename_problem_cb(
            [CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))] byte* problematic_refspec,
            IntPtr payload);

        [DllImport(libgit2)]
        private static extern unsafe int git_branch_upstream_name(
            GitBuf buf,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* referenceName);

        [DllImport(libgit2)]
        internal static extern void git_buf_free(GitBuf buf);

        [DllImport(libgit2)]
        internal static extern unsafe int git_checkout_tree(
            git_repository* repo,
            git_object* treeish,
            ref GitCheckoutOpts opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_checkout_index(
            git_repository* repo,
            git_object* treeish,
            ref GitCheckoutOpts opts);

        [DllImport(libgit2)]
        private static extern unsafe int git_clone(
            out git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* origin_url,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* workdir_path,
            ref GitCloneOptions opts);

        [DllImport(libgit2)]
        internal static extern unsafe git_signature* git_commit_author(git_object* commit);

        [DllImport(libgit2)]
        internal static extern unsafe git_signature* git_commit_committer(git_object* commit);

        [DllImport(libgit2)]
        private static extern unsafe int git_commit_create_from_ids(
            out GitOid id,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* updateRef,
            git_signature* author,
            git_signature* committer,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* encoding,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message,
            ref GitOid tree,
            UIntPtr parentCount,
            [MarshalAs(UnmanagedType.LPArray)] [In] IntPtr[] parents);

        [DllImport(libgit2)]
        private static extern unsafe int git_commit_create_buffer(
            GitBuf res,
            git_repository* repo,
            git_signature* author,
            git_signature* committer,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* encoding,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message,
            git_object* tree,
            UIntPtr parent_count,
            IntPtr* parents /* git_commit** originally */);

        [DllImport(libgit2)]
        private static extern unsafe int git_commit_create_with_signature(
            out GitOid id,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* commit_content,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* signature,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* signature_field);

        [DllImport(libgit2, EntryPoint = "git_commit_message")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_commit_message_(git_object* commit);

        [DllImport(libgit2, EntryPoint = "git_commit_summary")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_commit_summary_(git_object* commit);

        [DllImport(libgit2, EntryPoint = "git_commit_message_encoding")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_commit_message_encoding_(git_object* commit);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_commit_parent_id(git_object* commit, uint n);

        [DllImport(libgit2)]
        internal static extern unsafe uint git_commit_parentcount(git_object* commit);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_commit_tree_id(git_object* commit);

        [DllImport(libgit2)]
        private static extern unsafe int git_commit_extract_signature(
            GitBuf signature,
            GitBuf signed_data,
            git_repository* repo,
            ref GitOid commit_id,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* field);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_delete_entry(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_lock(out IntPtr txn, git_config* config);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_delete_multivar(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* regexp);

        [DllImport(libgit2)]
        internal static extern int git_config_find_global(GitBuf global_config_path);

        [DllImport(libgit2)]
        internal static extern int git_config_find_system(GitBuf system_config_path);

        [DllImport(libgit2)]
        internal static extern int git_config_find_xdg(GitBuf xdg_config_path);

        [DllImport(libgit2)]
        internal static extern int git_config_find_programdata(GitBuf programdata_config_path);

        [DllImport(libgit2)]
        internal static extern unsafe void git_config_free(git_config* cfg);

        [DllImport(libgit2)]
        internal static extern unsafe void git_config_entry_free(GitConfigEntry* entry);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_get_entry(
            out GitConfigEntry* entry,
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_add_file_ondisk(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path,
            uint level,
            [MarshalAs(UnmanagedType.Bool)] bool force);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_new(out git_config* cfg);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_add_backend(git_config* cfg, IntPtr backend, uint level, git_repository* repo,
            int force);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_open_level(
            out git_config* cfg,
            git_config* parent,
            uint level);

        [DllImport(libgit2)]
        private static unsafe extern int git_config_parse_bool(
            [MarshalAs(UnmanagedType.Bool)] out bool value,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* valueToParse);

        [DllImport(libgit2)]
        private static unsafe extern int git_config_parse_int32(
            [MarshalAs(UnmanagedType.I4)] out int value,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* valueToParse);

        [DllImport(libgit2)]
        private static unsafe extern int git_config_parse_int64(
            [MarshalAs(UnmanagedType.I8)] out long value,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* valueToParse);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_set_bool(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [MarshalAs(UnmanagedType.Bool)] bool value);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_set_int32(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            int value);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_set_int64(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            long value);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_set_string(
            git_config* cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* value);

        internal delegate int config_foreach_callback(
            IntPtr entry,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_foreach(
            git_config* cfg,
            config_foreach_callback callback,
            IntPtr payload);

        [DllImport(libgit2)]
        private static extern unsafe int git_config_iterator_glob_new(
            out IntPtr iter,
            IntPtr cfg,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* regexp);

        [DllImport(libgit2)]
        internal static extern int git_config_next(
            out IntPtr entry,
            IntPtr iter);

        [DllImport(libgit2)]
        internal static extern void git_config_iterator_free(IntPtr iter);

        [DllImport(libgit2)]
        internal static extern unsafe int git_config_snapshot(out git_config* @out, git_config* config);

        // Ordinarily we would decorate the `url` parameter with the StrictUtf8Marshaler like we do everywhere
        // else, but apparently doing a native->managed callback with the 64-bit version of CLR 2.0 can
        // sometimes vomit when using a custom IMarshaler.  So yeah, don't do that.  If you need the url,
        // call StrictUtf8Marshaler.FromNative manually.  See the discussion here:
        // http://social.msdn.microsoft.com/Forums/en-US/netfx64bit/thread/1eb746c6-d695-4632-8a9e-16c4fa98d481
        internal delegate int git_cred_acquire_cb(
            out IntPtr cred,
            IntPtr url,
            IntPtr username_from_url,
            GitCredentialType allowed_types,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern int git_cred_default_new(out IntPtr cred);

        [DllImport(libgit2)]
        private static unsafe extern int git_cred_userpass_plaintext_new(
            out IntPtr cred,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* username,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* password);

        [DllImport(libgit2)]
        internal static extern void git_cred_free(IntPtr cred);

        [DllImport(libgit2)]
        internal static extern unsafe int git_describe_commit(
            out git_describe_result* describe,
            git_object* committish,
            ref GitDescribeOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_describe_format(
            GitBuf buf,
            git_describe_result* describe,
            ref GitDescribeFormatOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe void git_describe_result_free(git_describe_result* describe);

        [DllImport(libgit2)]
        internal static extern unsafe void git_diff_free(git_diff* diff);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_tree_to_tree(
            out git_diff* diff,
            git_repository* repo,
            git_object* oldTree,
            git_object* newTree,
            GitDiffOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_tree_to_index(
            out git_diff* diff,
            git_repository* repo,
            git_object* oldTree,
            git_index* index,
            GitDiffOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_merge(
            git_diff* onto,
            git_diff* from);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_index_to_workdir(
            out git_diff* diff,
            git_repository* repo,
            git_index* index,
            GitDiffOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_tree_to_workdir(
            out git_diff* diff,
            git_repository* repo,
            git_object* oldTree,
            GitDiffOptions options);

        internal unsafe delegate int git_diff_file_cb(
            [In] git_diff_delta* delta,
            float progress,
            IntPtr payload);

        internal unsafe delegate int git_diff_hunk_cb(
            [In] git_diff_delta* delta,
            [In] GitDiffHunk hunk,
            IntPtr payload);

        internal unsafe delegate int git_diff_line_cb(
            [In] git_diff_delta* delta,
            [In] GitDiffHunk hunk,
            [In] GitDiffLine line,
            IntPtr payload);

        internal unsafe delegate int git_diff_binary_cb(
            [In] git_diff_delta* delta,
            [In] GitDiffBinary binary,
            IntPtr payload);

        [DllImport(libgit2)]
        private static extern unsafe int git_diff_blobs(
            git_object* oldBlob,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* old_as_path,
            git_object* newBlob,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* new_as_path,
            GitDiffOptions options,
            git_diff_file_cb fileCallback,
            git_diff_binary_cb binaryCallback,
            git_diff_hunk_cb hunkCallback,
            git_diff_line_cb lineCallback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_foreach(
            git_diff* diff,
            git_diff_file_cb fileCallback,
            git_diff_binary_cb binaryCallback,
            git_diff_hunk_cb hunkCallback,
            git_diff_line_cb lineCallback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_diff_find_similar(
            git_diff* diff,
            GitDiffFindOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_diff_num_deltas(git_diff* diff);

        [DllImport(libgit2)]
        internal static extern unsafe git_diff_delta* git_diff_get_delta(git_diff* diff, UIntPtr idx);

        [DllImport(libgit2)]
        private static unsafe extern int git_filter_register(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            IntPtr gitFilter, int priority);

        [DllImport(libgit2)]
        private static extern unsafe int git_filter_unregister(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_filter_source_mode(git_filter_source* source);

        [DllImport(libgit2)]
        internal static extern int git_libgit2_features();

        #region git_libgit2_opts

        // Bindings for git_libgit2_opts(int option, ...):
        // Currently only GIT_OPT_GET_SEARCH_PATH and GIT_OPT_SET_SEARCH_PATH are supported,
        // but other overloads could be added using a similar pattern.
        // CallingConvention.Cdecl is used to allow binding the the C varargs signature, and each possible call signature must be enumerated.
        // __argslist was an option, but is an undocumented feature that should likely not be used here.

        // git_libgit2_opts(GIT_OPT_GET_SEARCH_PATH, int level, git_buf *buf)
        [DllImport(libgit2, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int git_libgit2_opts(int option, uint level, GitBuf buf);

        // git_libgit2_opts(GIT_OPT_SET_SEARCH_PATH, int level, const char *path)
        [DllImport(libgit2, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int git_libgit2_opts(int option, uint level,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path);

        // git_libgit2_opts(GIT_OPT_ENABLE_*, int enabled)
        [DllImport(libgit2, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int git_libgit2_opts(int option, int enabled);
        #endregion

        [DllImport(libgit2)]
        internal static extern unsafe int git_graph_ahead_behind(out UIntPtr ahead, out UIntPtr behind, git_repository* repo, ref GitOid one, ref GitOid two);

        [DllImport(libgit2)]
        internal static extern unsafe int git_graph_descendant_of(
            git_repository* repo,
            ref GitOid commit,
            ref GitOid ancestor);

        [DllImport(libgit2)]
        private static extern unsafe int git_ignore_add_rule(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* rules);

        [DllImport(libgit2)]
        internal static extern unsafe int git_ignore_clear_internal_rules(git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_ignore_path_is_ignored(
            out int ignored,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path);

        [DllImport(libgit2)]
        private static extern unsafe int git_index_add_bypath(
            git_index* index,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_add(
            git_index* index,
            git_index_entry* entry);

        [DllImport(libgit2)]
        private static extern unsafe int git_index_conflict_get(
            out git_index_entry* ancestor,
            out git_index_entry* ours,
            out git_index_entry* theirs,
            git_index* index,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_conflict_iterator_new(
            out git_index_conflict_iterator* iterator,
            git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_conflict_next(
            out git_index_entry* ancestor,
            out git_index_entry* ours,
            out git_index_entry* theirs,
            git_index_conflict_iterator* iterator);

        [DllImport(libgit2)]
        internal static extern unsafe void git_index_conflict_iterator_free(
            git_index_conflict_iterator* iterator);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_index_entrycount(git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_entry_stage(git_index_entry* indexentry);

        [DllImport(libgit2)]
        internal static extern unsafe void git_index_free(git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe git_index_entry* git_index_get_byindex(git_index* index, UIntPtr n);

        [DllImport(libgit2)]
        private static extern unsafe git_index_entry* git_index_get_bypath(
            git_index* index,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path,
            int stage);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_has_conflicts(git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_index_name_entrycount(git_index* handle);

        [DllImport(libgit2)]
        internal static extern unsafe git_index_name_entry* git_index_name_get_byindex(git_index* handle, UIntPtr n);

        [DllImport(libgit2)]
        private static extern unsafe int git_index_open(
            out git_index* index,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* indexpath);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_read(
            git_index* index,
            [MarshalAs(UnmanagedType.Bool)] bool force);

        [DllImport(libgit2)]
        private static extern unsafe int git_index_remove_bypath(
            git_index* index,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path);


        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_index_reuc_entrycount(git_index* handle);

        [DllImport(libgit2)]
        internal static extern unsafe git_index_reuc_entry* git_index_reuc_get_byindex(git_index* handle, UIntPtr n);

        [DllImport(libgit2)]
        private static extern unsafe git_index_reuc_entry* git_index_reuc_get_bypath(
            git_index* handle,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* path);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_write(git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_write_tree(out GitOid treeOid, git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_write_tree_to(out GitOid treeOid, git_index* index, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_read_tree(git_index* index, git_object* tree);

        [DllImport(libgit2)]
        internal static extern unsafe int git_index_clear(git_index* index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_merge_base_many(
            out GitOid mergeBase,
            git_repository* repo,
            int length,
            [In] GitOid[] input_array);

        [DllImport(libgit2)]
        internal static extern unsafe int git_merge_base_octopus(
            out GitOid mergeBase,
            git_repository* repo,
            int length,
            [In] GitOid[] input_array);

        [DllImport(libgit2)]
        internal static extern unsafe int git_annotated_commit_from_ref(
            out git_annotated_commit* annotatedCommit,
            git_repository* repo,
            git_reference* reference);

        [DllImport(libgit2)]
        private static extern unsafe int git_annotated_commit_from_fetchhead(
            out git_annotated_commit* annotatedCommit,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* branch_name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote_url,
            ref GitOid oid);

        [DllImport(libgit2)]
        private static extern unsafe int git_annotated_commit_from_revspec(
            out git_annotated_commit* annotatedCommit,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* revspec);

        [DllImport(libgit2)]
        internal static extern unsafe int git_annotated_commit_lookup(
            out git_annotated_commit* annotatedCommit,
            git_repository* repo,
            ref GitOid id);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_annotated_commit_id(
            git_annotated_commit* annotatedCommit);

        [DllImport(libgit2)]
        internal static extern unsafe int git_merge(
            git_repository* repo,
            [In] IntPtr[] their_heads,
            UIntPtr their_heads_len,
            ref GitMergeOpts merge_opts,
            ref GitCheckoutOpts checkout_opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_merge_commits(
            out git_index* index,
            git_repository* repo,
            git_object* our_commit,
            git_object* their_commit,
            ref GitMergeOpts merge_opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_merge_analysis(
            out GitMergeAnalysis status_out,
            out GitMergePreference preference_out,
            git_repository* repo,
            [In] IntPtr[] their_heads,
            int their_heads_len);

        [DllImport(libgit2)]
        internal static extern unsafe void git_annotated_commit_free(git_annotated_commit* commit);

        [DllImport(libgit2)]
        private static unsafe extern int git_message_prettify(
            GitBuf buf,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message,
            [MarshalAs(UnmanagedType.Bool)] bool strip_comments,
            sbyte comment_char);

        [DllImport(libgit2)]
        private static extern unsafe int git_note_create(
            out GitOid noteOid,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* notes_ref,
            git_signature* author,
            git_signature* committer,
            ref GitOid oid,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* note,
            int force);

        [DllImport(libgit2)]
        internal static extern unsafe void git_note_free(git_note* note);

        [DllImport(libgit2, EntryPoint = "git_note_message")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_note_message_(git_note* note);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_note_id(git_note* note);

        [DllImport(libgit2)]
        private static extern unsafe int git_note_read(
            out git_note* note,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* notes_ref,
            ref GitOid oid);

        [DllImport(libgit2)]
        private static extern unsafe int git_note_remove(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* notes_ref,
            git_signature* author,
            git_signature* committer,
            ref GitOid oid);

        [DllImport(libgit2)]
        internal static extern unsafe int git_note_default_ref(
            GitBuf notes_ref,
            git_repository* repo);

        internal delegate int git_note_foreach_cb(
            ref GitOid blob_id,
            ref GitOid annotated_object_id,
            IntPtr payload);

        [DllImport(libgit2)]
        private static extern unsafe int git_note_foreach(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* notes_ref,
            git_note_foreach_cb cb,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_add_backend(git_odb* odb, IntPtr backend, int priority);

        [DllImport(libgit2)]
        internal static extern IntPtr git_odb_backend_malloc(IntPtr backend, UIntPtr len);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_exists(git_odb* odb, ref GitOid id);

        internal delegate int git_odb_foreach_cb(
            IntPtr id,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_foreach(
            git_odb* odb,
            git_odb_foreach_cb cb,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_open_wstream(out git_odb_stream* stream, git_odb* odb, Int64 size, GitObjectType type);

        [DllImport(libgit2)]
        internal static extern unsafe void git_odb_free(git_odb* odb);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_read_header(out UIntPtr len_out, out GitObjectType type, git_odb* odb, ref GitOid id);

        [DllImport(libgit2)]
        internal static extern unsafe void git_object_free(git_object* obj);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_stream_write(git_odb_stream* Stream, IntPtr Buffer, UIntPtr len);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_stream_finalize_write(out GitOid id, git_odb_stream* stream);

        [DllImport(libgit2)]
        internal static extern unsafe void git_odb_stream_free(git_odb_stream* stream);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_write(out GitOid id, git_odb* odb, byte* data, UIntPtr len, GitObjectType type);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_object_id(git_object* obj);

        [DllImport(libgit2)]
        internal static extern unsafe int git_object_lookup(out git_object* obj, git_repository* repo, ref GitOid id, GitObjectType type);

        [DllImport(libgit2)]
        internal static extern unsafe int git_object_peel(
            out git_object* peeled,
            git_object* obj,
            GitObjectType type);

        [DllImport(libgit2)]
        internal static extern unsafe int git_object_short_id(
            GitBuf buf,
            git_object* obj);

        [DllImport(libgit2)]
        internal static extern unsafe GitObjectType git_object_type(git_object* obj);

        [DllImport(libgit2)]
        internal static extern unsafe int git_patch_from_diff(out git_patch* patch, git_diff* diff, UIntPtr idx);

        [DllImport(libgit2)]
        internal static extern unsafe int git_patch_print(git_patch* patch, git_diff_line_cb print_cb, IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_patch_line_stats(
            out UIntPtr total_context,
            out UIntPtr total_additions,
            out UIntPtr total_deletions,
            git_patch* patch);

        [DllImport(libgit2)]
        internal static extern unsafe void git_patch_free(git_patch* patch);

        /* Push network progress notification function */
        internal delegate int git_push_transfer_progress(uint current, uint total, UIntPtr bytes, IntPtr payload);
        internal delegate int git_packbuilder_progress(int stage, uint current, uint total, IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe void git_packbuilder_free(git_packbuilder* packbuilder);

        [DllImport(libgit2)]
        private static extern unsafe int git_packbuilder_insert(
            git_packbuilder* packbuilder,
            ref GitOid id,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_packbuilder_insert_commit(
            git_packbuilder* packbuilder,
            ref GitOid id);

        [DllImport(libgit2)]
        private static extern unsafe int git_packbuilder_insert_recur(
            git_packbuilder* packbuilder,
            ref GitOid id,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_packbuilder_insert_tree(
            git_packbuilder* packbuilder,
            ref GitOid id);

        [DllImport(libgit2)]
        internal static extern unsafe int git_packbuilder_new(out git_packbuilder* packbuilder, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_packbuilder_object_count(git_packbuilder* packbuilder);

        [DllImport(libgit2)]
        internal static extern unsafe UInt32 git_packbuilder_set_threads(git_packbuilder* packbuilder, UInt32 numThreads);

        [DllImport(libgit2)]
        private static extern unsafe int git_packbuilder_write(
            git_packbuilder* packbuilder,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path,
            uint mode,
            IntPtr progressCallback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_packbuilder_written(git_packbuilder* packbuilder);

        [DllImport(libgit2)]
        internal static extern unsafe int git_refdb_set_backend(git_refdb* refdb, IntPtr backend);

        [DllImport(libgit2)]
        internal static extern unsafe int git_refdb_compress(git_refdb* refdb);

        [DllImport(libgit2)]
        internal static extern unsafe int git_refdb_open(out git_refdb* refdb, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe void git_refdb_free(git_refdb* refdb);

        [DllImport(libgit2)]
        internal static extern unsafe void git_refdb_free(IntPtr refDb);

        [DllImport(libgit2)]
        private static extern unsafe IntPtr git_reference__alloc(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            IntPtr oid,
            IntPtr peel);

        [DllImport(libgit2)]
        private static extern unsafe IntPtr git_reference__alloc_symbolic(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* target);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_create(
            out git_reference* reference,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            ref GitOid oid,
            [MarshalAs(UnmanagedType.Bool)] bool force,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_symbolic_create(
            out git_reference* reference,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* target,
            [MarshalAs(UnmanagedType.Bool)] bool force,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        internal delegate int ref_glob_callback(
            IntPtr reference_name,
            IntPtr payload);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_foreach_glob(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* glob,
            ref_glob_callback callback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe void git_reference_free(git_reference* reference);

        [DllImport(libgit2)]
        private static unsafe extern int git_reference_is_valid_name(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refname);

        [DllImport(libgit2)]
        internal static extern unsafe int git_reference_list(out GitStrArray array, git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_lookup(
            out git_reference* reference,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2, EntryPoint = "git_reference_name")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_reference_name_(git_reference* reference);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_remove(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_reference_target(git_reference* reference);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_rename(
            out git_reference* ref_out,
            git_reference* reference,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* newName,
            [MarshalAs(UnmanagedType.Bool)] bool force,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_set_target(
            out git_reference* ref_out,
            git_reference* reference,
            ref GitOid id,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_symbolic_set_target(
            out git_reference* ref_out,
            git_reference* reference,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* target,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        [DllImport(libgit2, EntryPoint = "git_reference_symbolic_target")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_reference_symbolic_target_(git_reference* reference);

        [DllImport(libgit2)]
        internal static extern unsafe GitReferenceType git_reference_type(git_reference* reference);

        [DllImport(libgit2)]
        private static extern unsafe int git_reference_ensure_log(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refname);

        [DllImport(libgit2)]
        internal static extern unsafe void git_reflog_free(git_reflog* reflog);

        [DllImport(libgit2)]
        private static extern unsafe int git_reflog_read(
            out git_reflog* ref_out,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_reflog_entrycount
            (git_reflog* reflog);

        [DllImport(libgit2)]
        internal static extern unsafe git_reflog_entry* git_reflog_entry_byindex(
            git_reflog* reflog,
            UIntPtr idx);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_reflog_entry_id_old(
            git_reflog_entry* entry);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_reflog_entry_id_new(
            git_reflog_entry* entry);

        [DllImport(libgit2)]
        internal static extern unsafe git_signature* git_reflog_entry_committer(
            git_reflog_entry* entry);

        [DllImport(libgit2, EntryPoint = "git_reflog_entry_message")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_reflog_entry_message_(git_reflog_entry* entry);

        [DllImport(libgit2)]
        private static unsafe extern int git_refspec_transform(
            GitBuf buf,
            IntPtr refspec,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);


        [DllImport(libgit2)]
        private static unsafe extern int git_refspec_rtransform(
            GitBuf buf,
            IntPtr refspec,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2, EntryPoint = "git_refspec_string")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_refspec_string_(
            IntPtr refSpec);

        [DllImport(libgit2)]
        internal static extern unsafe RefSpecDirection git_refspec_direction(IntPtr refSpec);

        [DllImport(libgit2, EntryPoint = "git_refspec_dst")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_refspec_dst_(
            IntPtr refSpec);

        [DllImport(libgit2, EntryPoint = "git_refspec_src")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_refspec_src_(
            IntPtr refspec);

        [DllImport(libgit2)]
        internal static extern bool git_refspec_force(IntPtr refSpec);

        [DllImport(libgit2)]
        private static extern unsafe bool git_refspec_src_matches(
            IntPtr refspec,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* reference);

        [DllImport(libgit2)]
        private static extern unsafe bool git_refspec_dst_matches(
            IntPtr refspec,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* reference);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_autotag(git_remote* remote);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_connect(
            git_remote* remote,
            GitDirection direction,
            ref GitRemoteCallbacks callbacks,
            ref GitProxyOptions proxy_options,
            ref GitStrArray custom_headers);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_create(
            out git_remote* remote,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_create_anonymous(
            out git_remote* remote,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);


        [DllImport(libgit2)]
        private static extern unsafe int git_remote_create_with_fetchspec(
            out git_remote* remote,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refspec);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_delete(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_fetch(
            git_remote* remote,
            ref GitStrArray refspecs,
            GitFetchOptions fetch_opts,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* log_message);

        [DllImport(libgit2)]
        internal static extern unsafe void git_remote_free(git_remote* remote);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_get_fetch_refspecs(out GitStrArray array, git_remote* remote);

        [DllImport(libgit2)]
        internal static extern unsafe git_refspec* git_remote_get_refspec(git_remote* remote, UIntPtr n);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_get_push_refspecs(out GitStrArray array, git_remote* remote);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_push(
            git_remote* remote,
            ref GitStrArray refSpecs,
            GitPushOptions opts);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_remote_refspec_count(git_remote* remote);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_set_url(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_add_fetch(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_set_pushurl(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_add_push(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        private static unsafe extern int git_remote_is_valid_name(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* remote_name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_list(out GitStrArray array, git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_remote_lookup(
            out git_remote* remote,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        internal static extern unsafe int git_remote_ls(out git_remote_head** heads, out UIntPtr size, git_remote* remote);

        [DllImport(libgit2, EntryPoint = "git_remote_name")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_remote_name_(git_remote* remote);

        [DllImport(libgit2, EntryPoint = "git_remote_url")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_remote_url_(git_remote* remote);

        [DllImport(libgit2, EntryPoint = "git_remote_pushurl")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_remote_pushurl_(git_remote* remote);

        [DllImport(libgit2)]
        private static extern unsafe void git_remote_set_autotag(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            TagFetchMode option);

        internal delegate int remote_progress_callback(IntPtr str, int len, IntPtr data);

        internal delegate int remote_completion_callback(RemoteCompletionType type, IntPtr data);

        internal delegate int remote_update_tips_callback(
            IntPtr refName,
            ref GitOid oldId,
            ref GitOid newId,
            IntPtr data);

        internal delegate int push_negotiation_callback(
            IntPtr updates,
            UIntPtr len,
            IntPtr payload);

        internal delegate int push_update_reference_callback(
            IntPtr refName,
            IntPtr status,
            IntPtr data
            );

        [DllImport(libgit2)]
        private static unsafe extern int git_repository_discover(
            GitBuf buf,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* start_path,
            [MarshalAs(UnmanagedType.Bool)] bool across_fs,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* ceiling_dirs);

        internal delegate int git_repository_fetchhead_foreach_cb(
            IntPtr remote_name,
            IntPtr remote_url,
            ref GitOid oid,
            [MarshalAs(UnmanagedType.Bool)] bool is_merge,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_fetchhead_foreach(
            git_repository* repo,
            git_repository_fetchhead_foreach_cb cb,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe void git_repository_free(git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_head_detached(IntPtr repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_head_unborn(IntPtr repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_ident(
            [CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))] out byte* name,
            [CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))] out byte* email,
            git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_index(out git_index* index, git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_init_ext(
            out git_repository* repository,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path,
            GitRepositoryInitOptions options);

        [DllImport(libgit2)]
        internal static extern int git_repository_is_bare(IntPtr handle);

        [DllImport(libgit2)]
        internal static extern int git_repository_is_shallow(IntPtr repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_state_cleanup(git_repository* repo);

        internal delegate int git_repository_mergehead_foreach_cb(
            ref GitOid oid,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_mergehead_foreach(
            git_repository* repo,
            git_repository_mergehead_foreach_cb cb,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_message(
            GitBuf buf,
            git_repository* repository);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_new(
            out git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe void git_repository_set_odb(git_repository* repo, git_odb* odb);

        [DllImport(libgit2)]
        internal static extern unsafe int git_odb_new(out git_odb* odb);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_odb(out git_odb* odb, git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_open(
            out git_repository* repository,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_open_ext(
            out git_repository* repository,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* path,
            RepositoryOpenFlags flags,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* ceilingDirs);

        [DllImport(libgit2, EntryPoint = "git_repository_path")]
        [return: CustomMarshaler(typeof(LaxFilePathNoCleanupMarshaler), typeof(FilePath))]
        private static extern unsafe byte* git_repository_path_(git_repository* repository);

        [DllImport(libgit2)]
        internal static extern unsafe void git_repository_set_refdb(git_repository* repo, git_refdb* refdb);

        [DllImport(libgit2)]
        internal static extern unsafe int git_refdb_new(out git_refdb* refdb, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_refdb(out git_refdb* refdb, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe void git_repository_set_config(
            git_repository* repository,
            git_config* config);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_config(out git_config* config, git_repository* repo);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_set_ident(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* email);


        [DllImport(libgit2)]
        internal static extern unsafe void git_repository_set_index(
            git_repository* repository,
            git_index* index);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_set_workdir(
            git_repository* repository,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* workdir,
            [MarshalAs(UnmanagedType.Bool)] bool update_gitlink);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_set_head_detached(
            git_repository* repo,
            ref GitOid commitish);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_set_head_detached_from_annotated(
            git_repository* repo,
            git_annotated_commit* commit);

        [DllImport(libgit2)]
        private static extern unsafe int git_repository_set_head(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refname);

        [DllImport(libgit2)]
        internal static extern unsafe int git_repository_state(
            git_repository* repository);

        [DllImport(libgit2, EntryPoint = "git_repository_workdir")]
        [return: CustomMarshaler(typeof(LaxFilePathNoCleanupMarshaler), typeof(FilePath))]
        private static extern unsafe byte* git_repository_workdir_(git_repository* repository);

        [DllImport(libgit2, EntryPoint = "git_repository_workdir")]
        [return: CustomMarshaler(typeof(LaxFilePathNoCleanupMarshaler), typeof(FilePath))]
        private static extern unsafe byte* git_repository_workdir_(IntPtr repository);

        [DllImport(libgit2)]
        internal static extern unsafe int git_reset(
            git_repository* repo,
            git_object* target,
            ResetMode reset_type,
            ref GitCheckoutOpts opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revert(
            git_repository* repo,
            git_object* commit,
            GitRevertOpts opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revert_commit(
            out git_index* index,
            git_repository* repo,
            git_object* revert_commit,
            git_object* our_commit,
            uint mainline,
            ref GitMergeOpts opts);

        [DllImport(libgit2)]
        private static extern unsafe int git_revparse_ext(
            out git_object* obj,
            out git_reference* reference,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* spec);

        [DllImport(libgit2)]
        internal static extern unsafe void git_revwalk_free(git_revwalk* walker);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revwalk_hide(git_revwalk* walker, ref GitOid commit_id);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revwalk_new(out git_revwalk* walker, git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revwalk_next(out GitOid id, git_revwalk* walker);

        [DllImport(libgit2)]
        internal static extern unsafe int git_revwalk_push(git_revwalk* walker, ref GitOid id);

        [DllImport(libgit2)]
        internal static extern unsafe void git_revwalk_reset(git_revwalk* walker);

        [DllImport(libgit2)]
        internal static extern unsafe void git_revwalk_sorting(git_revwalk* walk, CommitSortStrategies sort);

        [DllImport(libgit2)]
        internal static extern unsafe void git_revwalk_simplify_first_parent(git_revwalk* walk);

        [DllImport(libgit2)]
        internal static extern unsafe void git_signature_free(git_signature* signature);

        [DllImport(libgit2)]
        private static extern unsafe int git_signature_new(
            out git_signature* signature,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* email,
            long time,
            int offset);

        [DllImport(libgit2)]
        private static extern unsafe int git_signature_now(
            out git_signature* signature,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* email);

        [DllImport(libgit2)]
        internal static extern unsafe int git_signature_dup(out git_signature* dest, git_signature* sig);

        [DllImport(libgit2)]
        private static extern unsafe int git_stash_save(
            out GitOid id,
            git_repository* repo,
            git_signature* stasher,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message,
            StashModifiers flags);

        internal delegate int git_stash_cb(
            UIntPtr index,
            IntPtr message,
            ref GitOid stash_id,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_stash_foreach(
            git_repository* repo,
            git_stash_cb callback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_stash_drop(git_repository* repo, UIntPtr index);

        [DllImport(libgit2)]
        internal static extern unsafe int git_stash_apply(
            git_repository* repo,
            UIntPtr index,
            GitStashApplyOpts opts);

        [DllImport(libgit2)]
        internal static extern unsafe int git_stash_pop(
            git_repository* repo,
            UIntPtr index,
            GitStashApplyOpts opts);

        [DllImport(libgit2)]
        private static extern unsafe int git_status_file(
            out FileStatus statusflags,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* filepath);


        [DllImport(libgit2)]
        internal static extern unsafe int git_status_list_new(
            out git_status_list* git_status_list,
            git_repository* repo,
            GitStatusOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_status_list_entrycount(
            git_status_list* statusList);

        [DllImport(libgit2)]
        internal static extern unsafe git_status_entry* git_status_byindex(
            git_status_list* list,
            UIntPtr idx);

        [DllImport(libgit2)]
        internal static extern unsafe void git_status_list_free(
            git_status_list* statusList);

        [DllImport(libgit2)]
        internal static extern void git_strarray_free(
            ref GitStrArray array);

        [DllImport(libgit2)]
        private static extern unsafe int git_submodule_lookup(
            out git_submodule* reference,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name);

        [DllImport(libgit2)]
        private static extern unsafe int git_submodule_resolve_url(
            GitBuf buf,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* url);

        [DllImport(libgit2)]
        internal static extern unsafe int git_submodule_update(
            git_submodule* sm,
            [MarshalAs(UnmanagedType.Bool)] bool init,
            ref GitSubmoduleUpdateOptions submoduleUpdateOptions);

        internal delegate int submodule_callback(
            IntPtr sm,
            IntPtr name,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_submodule_foreach(
            git_repository* repo,
            submodule_callback callback,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_submodule_add_to_index(
            git_submodule* submodule,
            [MarshalAs(UnmanagedType.Bool)] bool write_index);

        [DllImport(libgit2)]
        internal static extern unsafe void git_submodule_free(git_submodule* submodule);

        [DllImport(libgit2, EntryPoint = "git_submodule_path")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_submodule_path_(
            git_submodule* submodule);

        [DllImport(libgit2, EntryPoint = "git_submodule_url")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_submodule_url_(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_submodule_index_id(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_submodule_head_id(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_submodule_wd_id(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe SubmoduleIgnore git_submodule_ignore(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe SubmoduleUpdate git_submodule_update_strategy(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe SubmoduleRecurse git_submodule_fetch_recurse_submodules(
            git_submodule* submodule);

        [DllImport(libgit2)]
        internal static extern unsafe int git_submodule_reload(
            git_submodule* submodule,
            [MarshalAs(UnmanagedType.Bool)] bool force);

        [DllImport(libgit2)]
        private static extern unsafe int git_submodule_status(
            out SubmoduleStatus status,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictFilePathMarshaler), typeof(FilePath))] byte* name,
            GitSubmoduleIgnore ignore);

        [DllImport(libgit2)]
        internal static extern unsafe int git_submodule_init(
            git_submodule* submodule,
            [MarshalAs(UnmanagedType.Bool)] bool overwrite);

        [DllImport(libgit2)]
        private static extern unsafe int git_tag_annotation_create(
            out GitOid oid,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            git_object* target,
            git_signature* signature,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message);

        [DllImport(libgit2)]
        private static extern unsafe int git_tag_create(
            out GitOid oid,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            git_object* target,
            git_signature* signature,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* message,
            [MarshalAs(UnmanagedType.Bool)]
            bool force);

        [DllImport(libgit2)]
        private static extern unsafe int git_tag_create_lightweight(
            out GitOid oid,
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* name,
            git_object* target,
            [MarshalAs(UnmanagedType.Bool)]
            bool force);

        [DllImport(libgit2)]
        private static extern unsafe int git_tag_delete(
            git_repository* repo,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* tagName);

        [DllImport(libgit2)]
        internal static extern unsafe int git_tag_list(out GitStrArray array, git_repository* repo);

        [DllImport(libgit2, EntryPoint = "git_tag_message")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_tag_message_(git_object* tag);

        [DllImport(libgit2, EntryPoint = "git_tag_name")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_tag_name_(git_object* tag);

        [DllImport(libgit2)]
        internal static extern unsafe git_signature* git_tag_tagger(git_object* tag);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_tag_target_id(git_object* tag);

        [DllImport(libgit2)]
        internal static extern unsafe GitObjectType git_tag_target_type(git_object* tag);

        [DllImport(libgit2)]
        internal static extern int git_libgit2_init();

        [DllImport(libgit2)]
        internal static extern int git_libgit2_shutdown();

        [DllImport(libgit2)]
        internal static extern int git_openssl_set_locking();

        internal delegate void git_trace_cb(LogLevel level, IntPtr message);

        [DllImport(libgit2)]
        internal static extern int git_trace_set(LogLevel level, git_trace_cb trace_cb);

        internal delegate int git_transfer_progress_callback(ref GitTransferProgress stats, IntPtr payload);

        internal delegate int git_transport_cb(out IntPtr transport, IntPtr remote, IntPtr payload);

        internal unsafe delegate int git_transport_certificate_check_cb(git_certificate* cert, int valid, IntPtr hostname, IntPtr payload);

        [DllImport(libgit2)]
        internal static extern unsafe int git_transaction_new(
            out git_transaction* transaction,
            git_repository* repo);

        [DllImport(libgit2)]
        internal static extern unsafe int git_transaction_lock_ref(
            git_transaction* transaction,
            string refName);

        [DllImport(libgit2)]
        private static extern unsafe int git_transaction_set_target(
            git_transaction* tx,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refName,
            ref GitOid target, // const git_oid*
            git_signature* sig,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* msg);

        [DllImport(libgit2)]
        private static extern unsafe int git_transaction_set_symbolic_target(
            git_transaction* tx,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refName,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* target,
            git_signature* sig,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* msg
            );

        [DllImport(libgit2)]
        private static extern unsafe int git_transaction_set_reflog(
            git_transaction* tx,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refName,
            IntPtr reflog // const git_reflog* reflog
            );

        [DllImport(libgit2)]
        private static extern unsafe int git_transaction_remove(
            git_transaction* tx,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* refName
            );

        [DllImport(libgit2)]
        internal static extern unsafe int git_transaction_commit(git_transaction* tx);

        [DllImport(libgit2)]
        internal static extern void git_transaction_free(IntPtr tx);

        [DllImport(libgit2)]
        internal static extern unsafe void git_transaction_free(git_transaction* tx);

        [DllImport(libgit2)]
        internal static extern int git_transaction_commit(IntPtr txn);

        [DllImport(libgit2)]
        private static extern unsafe int git_transport_register(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* prefix,
            IntPtr transport_cb,
            IntPtr payload);

        [DllImport(libgit2)]
        internal static extern int git_transport_smart(
            out IntPtr transport,
            IntPtr remote,
            IntPtr definition);

        [DllImport(libgit2)]
        private static unsafe extern int git_transport_smart_certificate_check(
            IntPtr transport,
            IntPtr cert,
            int valid,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* hostname);

        [DllImport(libgit2)]
        private static unsafe extern int git_transport_smart_credentials(
            out IntPtr cred_out,
            IntPtr transport,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* user,
            int methods);

        [DllImport(libgit2)]
        private static unsafe extern int git_transport_unregister(
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* prefix);

        [DllImport(libgit2)]
        internal static extern unsafe uint git_tree_entry_filemode(git_tree_entry* entry);

        [DllImport(libgit2)]
        internal static extern unsafe git_tree_entry* git_tree_entry_byindex(git_object* tree, UIntPtr idx);

        [DllImport(libgit2)]
        private static extern unsafe int git_tree_entry_bypath(
            out git_tree_entry* tree,
            git_object* root,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* treeentry_path);

        [DllImport(libgit2)]
        internal static extern unsafe void git_tree_entry_free(git_tree_entry* treeEntry);

        [DllImport(libgit2)]
        internal static extern unsafe git_oid* git_tree_entry_id(git_tree_entry* entry);

        [DllImport(libgit2, EntryPoint = "git_tree_entry_name")]
        [return: CustomMarshaler(typeof(LaxUtf8NoCleanupMarshaler), typeof(string))]
        private static extern unsafe byte* git_tree_entry_name_(git_tree_entry* entry);

        [DllImport(libgit2)]
        internal static extern unsafe GitObjectType git_tree_entry_type(git_tree_entry* entry);

        [DllImport(libgit2)]
        internal static extern unsafe UIntPtr git_tree_entrycount(git_object* tree);

        [DllImport(libgit2)]
        internal static extern unsafe int git_treebuilder_new(out git_treebuilder* builder, git_repository* repo, IntPtr src);

        [DllImport(libgit2)]
        private static extern unsafe int git_treebuilder_insert(
            IntPtr entry_out,
            git_treebuilder* builder,
            [CustomMarshaler(typeof(StrictUtf8Marshaler), typeof(string))] byte* treeentry_name,
            ref GitOid id,
            uint attributes);

        [DllImport(libgit2)]
        internal static extern unsafe int git_treebuilder_write(out GitOid id, git_treebuilder* bld);

        [DllImport(libgit2)]
        internal static extern unsafe void git_treebuilder_free(git_treebuilder* bld);

        [DllImport(libgit2)]
        internal static extern unsafe int git_blob_is_binary(git_object* blob);

        [DllImport(libgit2)]
        internal static extern unsafe int git_cherrypick(git_repository* repo, git_object* commit, GitCherryPickOptions options);

        [DllImport(libgit2)]
        internal static extern unsafe int git_cherrypick_commit(out git_index* index,
            git_repository* repo,
            git_object* cherrypick_commit,
            git_object* our_commit,
            uint mainline,
            ref GitMergeOpts options);
    }
}
// ReSharper restore InconsistentNaming
