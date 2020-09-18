using System;
using System.IO;
using System.Text;

namespace Core.IO
{
    //taken from https://github.com/dotnet/coreclr/blob/release/3.1/src/System.Private.CoreLib/shared/System/IO/Path.cs
    //and https://github.com/dotnet/coreclr/blob/release/3.1/src/System.Private.CoreLib/shared/System/IO/PathInternal.Windows.cs
    //and modified to fit old .net framework

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    
    public static class PathCore
    {        
        /// <summary>
        /// Returns true if the path is fixed to a specific drive or UNC path. This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// Returns false if the path specified is relative to the current drive or working directory.
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths <see cref="Path.IsPathRooted(string)"/> are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="path"/> is null.
        /// </exception>
        public static bool IsPathFullyQualified(string path) {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return !PathInternal.IsPartiallyQualified(path);
        }

        /// <summary>
        /// Create a relative path from one path to another. Paths will be resolved before calculating the difference.
        /// Default path comparison for the active platform will be used (OrdinalIgnoreCase for Windows or Mac, Ordinal for Unix).
        /// </summary>
        /// <param name="relativeTo">The source path the output should be relative to. This path is always considered to be a directory.</param>
        /// <param name="path">The destination path.</param>
        /// <returns>The relative path or <paramref name="path"/> if the paths don't share the same root.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeTo"/> or <paramref name="path"/> is <c>null</c> or an empty string.</exception>
        public static string GetRelativePath(string relativeTo, string path) {
            return GetRelativePath(relativeTo, path, StringComparison);
        }

        private static string GetRelativePath(string relativeTo, string path, StringComparison comparisonType) {
            if (relativeTo == null)
                throw new ArgumentNullException(nameof(relativeTo));

            if (PathInternal.IsEffectivelyEmpty(relativeTo))
                throw new ArgumentException("SR.Arg_PathEmpty", nameof(relativeTo));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (PathInternal.IsEffectivelyEmpty(path))
                throw new ArgumentException("SR.Arg_PathEmpty", nameof(path));

            //Debug.Assert(comparisonType == StringComparison.Ordinal || comparisonType == StringComparison.OrdinalIgnoreCase);

            relativeTo = Path.GetFullPath(relativeTo);
            path = Path.GetFullPath(path);

            // Need to check if the roots are different- if they are we need to return the "to" path.
            if (!PathInternal.AreRootsEqual(relativeTo, path, comparisonType))
                return path;

            int commonLength = PathInternal.GetCommonPathLength(relativeTo, path, ignoreCase: comparisonType == StringComparison.OrdinalIgnoreCase);

            // If there is nothing in common they can't share the same root, return the "to" path as is.
            if (commonLength == 0)
                return path;

            // Trailing separators aren't significant for comparison
            int relativeToLength = relativeTo.Length;
            if (EndsInDirectorySeparator(relativeTo))
                relativeToLength--;

            bool pathEndsInSeparator = EndsInDirectorySeparator(path);
            int pathLength = path.Length;
            if (pathEndsInSeparator)
                pathLength--;

            // If we have effectively the same path, return "."
            if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".";

            // We have the same root, we need to calculate the difference now using the
            // common Length and Segment count past the length.
            //
            // Some examples:
            //
            //  C:\Foo C:\Bar L3, S1 -> ..\Bar
            //  C:\Foo C:\Foo\Bar L6, S0 -> Bar
            //  C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar
            //  C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar

            StringBuilder sb = StringBuilderCache.Acquire(Math.Max(relativeTo.Length, path.Length));

            // Add parent segments for segments past the common on the "from" path
            if (commonLength < relativeToLength) {
                sb.Append("..");

                for (int i = commonLength + 1; i < relativeToLength; i++) {
                    if (PathInternal.IsDirectorySeparator(relativeTo[i])) {
                        sb.Append(PathInternal.DirectorySeparatorChar);
                        sb.Append("..");
                    }
                }
            }
            else if (PathInternal.IsDirectorySeparator(path[commonLength])) {
                // No parent segments and we need to eat the initial separator
                //  (C:\Foo C:\Foo\Bar case)
                commonLength++;
            }

            // Now add the rest of the "to" path, adding back the trailing separator
            int differenceLength = pathLength - commonLength;
            if (pathEndsInSeparator)
                differenceLength++;

            if (differenceLength > 0) {
                if (sb.Length > 0) {
                    sb.Append(PathInternal.DirectorySeparatorChar);
                }

                sb.Append(path, commonLength, differenceLength);
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }


        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        public static bool EndsInDirectorySeparator(string path)
              => path != null && path.Length > 0 && PathInternal.IsDirectorySeparator(path[path.Length - 1]);

        /// <summary>Returns a comparison that can be used to compare file and directory names for equality.</summary>
        internal static StringComparison StringComparison {
            get {
               return StringComparison.OrdinalIgnoreCase;
            }
        }

        /// <summary>Provide a cached reusable instance of stringbuilder per thread.</summary>
        internal static class StringBuilderCache
        {
            // The value 360 was chosen in discussion with performance experts as a compromise between using
            // as litle memory per thread as possible and still covering a large part of short-lived
            // StringBuilder creations on the startup path of VS designers.
            internal const int MaxBuilderSize = 360;
            private const int DefaultCapacity = 16; // == StringBuilder.DefaultCapacity

            // WARNING: We allow diagnostic tools to directly inspect this member (t_cachedInstance).
            // See https://github.com/dotnet/corert/blob/master/Documentation/design-docs/diagnostics/diagnostics-tools-contract.md for more details. 
            // Please do not change the type, the name, or the semantic usage of this member without understanding the implication for tools. 
            // Get in touch with the diagnostics team if you have questions.
            [ThreadStatic]
            private static StringBuilder t_cachedInstance;

            /// <summary>Get a StringBuilder for the specified capacity.</summary>
            /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
            public static StringBuilder Acquire(int capacity = DefaultCapacity) {
                if (capacity <= MaxBuilderSize) {
                    StringBuilder sb = t_cachedInstance;
                    if (sb != null) {
                        // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                        // when the requested size is larger than the current capacity
                        if (capacity <= sb.Capacity) {
                            t_cachedInstance = null;
                            sb.Clear();
                            return sb;
                        }
                    }
                }

                return new StringBuilder(capacity);
            }

            /// <summary>Place the specified builder in the cache if it is not too big.</summary>
            public static void Release(StringBuilder sb) {
                if (sb.Capacity <= MaxBuilderSize) {
                    t_cachedInstance = sb;
                }
            }

            /// <summary>ToString() the stringbuilder, Release it to the cache, and return the resulting string.</summary>
            public static string GetStringAndRelease(StringBuilder sb) {
                string result = sb.ToString();
                Release(sb);
                return result;
            }
        }

        protected static class PathInternal
        {
            internal const char DirectorySeparatorChar = '\\';
            internal const char AltDirectorySeparatorChar = '/';
            internal const char VolumeSeparatorChar = ':';
            // \\?\, \\.\, \??\
            internal const int DevicePrefixLength = 4;
            // \\
            internal const int UncPrefixLength = 2;
            // \\?\UNC\, \\.\UNC\
            internal const int UncExtendedPrefixLength = 8;

            /// <summary>
            /// Returns true if the path specified is relative to the current drive or working directory.
            /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
            /// validation of the path (URIs will be returned as relative as a result).
            /// </summary>
            /// <remarks>
            /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
            /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
            /// "C:a" is drive relative- meaning that it will be resolved against the current directory
            /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
            /// will not be used to modify the path).
            /// </remarks>
            internal static bool IsPartiallyQualified(string path) {
                if (path.Length < 2) {
                    // It isn't fixed, it must be relative.  There is no way to specify a fixed
                    // path with one character (or less).
                    return true;
                }

                if (IsDirectorySeparator(path[0])) {
                    // There is no valid way to specify a relative path with two initial slashes or
                    // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                    return !(path[1] == '?' || IsDirectorySeparator(path[1]));
                }

                // The only way to specify a fixed path that doesn't begin with two slashes
                // is the drive, colon, slash format- i.e. C:\
                return !((path.Length >= 3)
                    && (path[1] == VolumeSeparatorChar)
                    && IsDirectorySeparator(path[2])
                    // To match old behavior we'll check the drive character for validity as the path is technically
                    // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                    && IsValidDriveChar(path[0]));
            }

            /// <summary>
            /// True if the given character is a directory separator.
            /// </summary>
            internal static bool IsDirectorySeparator(char c) {
                return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
            }

            /// <summary>
            /// Returns true if the given character is a valid drive letter
            /// </summary>
            internal static bool IsValidDriveChar(char value) {
                return ((value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z'));
            }

            /// <summary>
            /// Returns true if the path is effectively empty for the current OS.
            /// For unix, this is empty or null. For Windows, this is empty, null, or 
            /// just spaces ((char)32).
            /// </summary>
            internal static bool IsEffectivelyEmpty(string path) {
                if (String.IsNullOrEmpty(path))
                    return true;

                foreach (char c in path) {
                    if (c != ' ')
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Returns true if the two paths have the same root
            /// </summary>
            internal static bool AreRootsEqual(string first, string second, StringComparison comparisonType) {
                int firstRootLength = GetRootLength(first);
                int secondRootLength = GetRootLength(second);

                return firstRootLength == secondRootLength
                    && string.Compare(
                        strA: first,
                        indexA: 0,
                        strB: second,
                        indexB: 0,
                        length: firstRootLength,
                        comparisonType: comparisonType) == 0;
            }

            /// <summary>
            /// Gets the length of the root of the path (drive, share, etc.).
            /// </summary>
            internal static int GetRootLength(string path) {
                int pathLength = path.Length;
                int i = 0;

                bool deviceSyntax = IsDevice(path);
                bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

                if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0])) {
                    // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                    if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1]))) {
                        // UNC (\\?\UNC\ or \\), scan past server\share

                        // Start past the prefix ("\\" or "\\?\UNC\")
                        i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                        // Skip two separators at most
                        int n = 2;
                        while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                            i++;
                    }
                    else {
                        // Current drive rooted (e.g. "\foo")
                        i = 1;
                    }
                }
                else if (deviceSyntax) {
                    // Device path (e.g. "\\?\.", "\\.\")
                    // Skip any characters following the prefix that aren't a separator
                    i = DevicePrefixLength;
                    while (i < pathLength && !IsDirectorySeparator(path[i]))
                        i++;

                    // If there is another separator take it, as long as we have had at least one
                    // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                    if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                        i++;
                }
                else if (pathLength >= 2
                    && path[1] == VolumeSeparatorChar
                    && IsValidDriveChar(path[0])) {
                    // Valid drive specified path ("C:", "D:", etc.)
                    i = 2;

                    // If the colon is followed by a directory separator, move past it (e.g "C:\")
                    if (pathLength > 2 && IsDirectorySeparator(path[2]))
                        i++;
                }

                return i;
            }

            /// <summary>
            /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
            /// </summary>
            internal static bool IsDevice(string path) {
                // If the path begins with any two separators is will be recognized and normalized and prepped with
                // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
                return IsExtended(path)
                    ||
                    (
                        path.Length >= DevicePrefixLength
                        && IsDirectorySeparator(path[0])
                        && IsDirectorySeparator(path[1])
                        && (path[2] == '.' || path[2] == '?')
                        && IsDirectorySeparator(path[3])
                    );
            }

            /// <summary>
            /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
            /// </summary>
            internal static bool IsDeviceUNC(string path) {
                return path.Length >= UncExtendedPrefixLength
                    && IsDevice(path)
                    && IsDirectorySeparator(path[7])
                    && path[4] == 'U'
                    && path[5] == 'N'
                    && path[6] == 'C';
            }

            /// <summary>
            /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
            /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
            /// and path length checks.
            /// </summary>
            internal static bool IsExtended(string path) {
                // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
                // Skipping of normalization will *only* occur if back slashes ('\') are used.
                return path.Length >= DevicePrefixLength
                    && path[0] == '\\'
                    && (path[1] == '\\' || path[1] == '?')
                    && path[2] == '?'
                    && path[3] == '\\';
            }

            /// <summary>
            /// Get the common path length from the start of the string.
            /// </summary>
            internal static int GetCommonPathLength(string first, string second, bool ignoreCase) {
                int commonChars = EqualStartingCharacterCount(first, second, ignoreCase: ignoreCase);

                // If nothing matches
                if (commonChars == 0)
                    return commonChars;

                // Or we're a full string and equal length or match to a separator
                if (commonChars == first.Length
                    && (commonChars == second.Length || IsDirectorySeparator(second[commonChars])))
                    return commonChars;

                if (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))
                    return commonChars;

                // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
                while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
                    commonChars--;

                return commonChars;
            }

            /// <summary>
            /// Gets the count of common characters from the left optionally ignoring case
            /// </summary>
            internal static unsafe int EqualStartingCharacterCount(string first, string second, bool ignoreCase) {
                if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) return 0;

                int commonChars = 0;

                fixed (char* f = first)
                fixed (char* s = second) {
                    char* l = f;
                    char* r = s;
                    char* leftEnd = l + first.Length;
                    char* rightEnd = r + second.Length;

                    while (l != leftEnd && r != rightEnd
                        && (*l == *r || (ignoreCase && char.ToUpperInvariant((*l)) == char.ToUpperInvariant((*r))))) {
                        commonChars++;
                        l++;
                        r++;
                    }
                }

                return commonChars;
            }

        }

    }




}
