using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{


    /// <summary>
    /// Fetch options for the GetDirectoryEntries method.
    /// </summary>
    public enum EntryFetchOptions
    {
        /// <summary>
        /// Fetch only files.
        /// </summary>
        FilesOnly,
        /// <summary>
        /// Fetch only directories.
        /// </summary>
        DirectoriesOnly,
        /// <summary>
        /// Fetch both files and directories.
        /// </summary>
        Both
    }

    /// <summary>
    /// Exposes static methods for working with files and directories.
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Returns an enumerable collection of entries in the given path.
        /// </summary>
        /// <remarks>This implementation will not crash if an exception is thrown during the enumeration of directories under the given path.</remarks>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="patternMatch">The search string to match against file-system entries in path. If null or an empty string all entries will be fetched.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <param name="fetchOption">One of the enumeration values that specifies whether the search operation should fetch either files or directories or both.</param>
        /// <returns>An enumerable collection of file-system entries in the directory specified by path and that match the specified search pattern and options.</returns>
        public static IEnumerable<string> GetDirectoryEntries(string path, string patternMatch, SearchOption searchOption, EntryFetchOptions fetchOption)
        {
            if (String.IsNullOrWhiteSpace(patternMatch) == true) patternMatch = "*.*";
            if (String.IsNullOrWhiteSpace(path) == true) return Enumerable.Empty<string>();
            path = Path.GetFullPath(path);
            IEnumerable<string> foundFiles = Enumerable.Empty<string>(); // Start with an empty container

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(path);
                    foreach (string dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryEntries(dir, patternMatch, searchOption, fetchOption)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (Exception) { } // Incase we have an access error - we don't want to mask the rest
            }

            try
            {
                if (fetchOption == EntryFetchOptions.FilesOnly) foundFiles = foundFiles.Concat(Directory.EnumerateFiles(path, patternMatch)); // Add files from the current directory to the list
                if (fetchOption == EntryFetchOptions.DirectoriesOnly) foundFiles = foundFiles.Concat(Directory.EnumerateDirectories(path, patternMatch));
                if (fetchOption == EntryFetchOptions.Both) foundFiles = foundFiles.Concat(Directory.EnumerateFileSystemEntries(path, patternMatch));
            }
            catch (Exception) { } // In case we have an access error - we don't want to mask the rest

            return foundFiles; // This is it finally
        }

        /// <summary>
        /// Duplicates a directory, setting the folder and contained objects to the given owner, using the given options.
        /// </summary>
        /// <param name="sourcePath">The absolute path of the folder to duplicate.</param>
        /// <param name="destinationPath">The absolute path of the required duplicate.</param>
        /// <param name="owner">The required owner of the folder and all contained objects. If this parameter is null or empty the current user will own the duplicate.</param>
        /// <param name="options">One of the enumeration values that specifies additional options to use while copying.</param>
        /// <returns>true if the directory was successfully duplicated, false otherwise.</returns>
        public static bool DuplicateFolder(string sourcePath, string destinationPath, string owner, DuplicateOptions options)
        {
            if (String.IsNullOrWhiteSpace(sourcePath) == true) return false;
            if (String.IsNullOrWhiteSpace(destinationPath) == true) return false;
            if (String.IsNullOrWhiteSpace(owner) == true) owner = Environment.UserName;
            if (Directory.Exists(sourcePath) == false) return false;
            if ((DirectoryIsHiddenOrSystem(sourcePath) == true) && ((options & DuplicateOptions.SkipSystemFiles) == DuplicateOptions.SkipSystemFiles)) return false;

            IdentityReference objectOwner = new NTAccount(owner);
            IdentityReference admin = new NTAccount("Administrator");
            IdentityReference system = new NTAccount("SYSTEM");
            FileSystemAccessRule ownerRule = new FileSystemAccessRule(objectOwner, FileSystemRights.FullControl, AccessControlType.Allow);
            FileSystemAccessRule adminRule = new FileSystemAccessRule(admin, FileSystemRights.FullControl, AccessControlType.Allow);
            FileSystemAccessRule systemRule = new FileSystemAccessRule(system, FileSystemRights.FullControl, AccessControlType.Allow);
            DirectorySecurity dirSecurity = new DirectorySecurity();
            dirSecurity.SetOwner(objectOwner);
            dirSecurity.AddAccessRule(ownerRule);
            dirSecurity.AddAccessRule(adminRule);
            dirSecurity.AddAccessRule(systemRule);
            FileSecurity fileSecurity = new FileSecurity();
            fileSecurity.SetOwner(objectOwner);
            fileSecurity.AddAccessRule(ownerRule);
            fileSecurity.AddAccessRule(adminRule);
            fileSecurity.AddAccessRule(systemRule);
            FileAttributes fa1;
            FileAttributes fa2;
            MemoryStream tiffFile = null; ;

            if (Directory.Exists(destinationPath) == false)
            {
                Directory.CreateDirectory(destinationPath, dirSecurity);
            }
            else
            {
                Directory.SetAccessControl(destinationPath, dirSecurity);
            }
            string ddir;
            foreach (string dir in Directory.EnumerateDirectories(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if ((DirectoryIsHiddenOrSystem(dir) == true) && ((options & DuplicateOptions.SkipSystemFiles) == DuplicateOptions.SkipSystemFiles)) continue;
                ddir = dir.Replace(sourcePath, destinationPath);
                try
                {
                    if (Directory.Exists(ddir) == false)
                    {
                        Directory.CreateDirectory(ddir, dirSecurity);
                    }
                    else
                    {
                        Directory.SetAccessControl(ddir, dirSecurity);
                    }
                    fa1 = File.GetAttributes(dir);
                    fa2 = File.GetAttributes(ddir);
                    File.SetAttributes(ddir, (fa1 | fa2));
                    File.SetCreationTimeUtc(ddir, File.GetCreationTimeUtc(dir));
                    File.SetLastAccessTimeUtc(ddir, File.GetLastWriteTimeUtc(dir));
                    File.SetLastWriteTimeUtc(ddir, File.GetLastWriteTimeUtc(dir));
                }
                catch
                {
                    continue;
                }
            }
            if ((options & DuplicateOptions.DirectoryStructureOnly) == DuplicateOptions.DirectoryStructureOnly) return true;
            string dfile;
            foreach (string file in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if ((FileIsHiddenOrSystem(file) == true) && ((options & DuplicateOptions.SkipSystemFiles) == DuplicateOptions.SkipSystemFiles)) continue;
                dfile = file.Replace(sourcePath, destinationPath);
                string newExt;
                Random rnd = new Random();
                try
                {
                    if (File.Exists(dfile) == true)
                    {
                        if ((FileUtilities.FilesAreIdentical(file, dfile) == true) || (FileIsHiddenOrSystem(dfile) == true))
                        {
                            File.SetAccessControl(dfile, fileSecurity);
                            continue;
                        }
                        newExt = Path.GetExtension(dfile);
                        newExt = rnd.Next(0, 10000).ToString("D5") + newExt;
                        Path.ChangeExtension(dfile, newExt);
                    }
                    FileStream source = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if ((source.Length == 0) && ((options & DuplicateOptions.SkipZeroByteFiles) == DuplicateOptions.SkipZeroByteFiles))
                    {
                        source.Close();
                        continue;
                    }
                    FileStream dest;
                    //if ((options & DuplicateOptions.ConvertImagesToTIFF) == DuplicateOptions.ConvertImagesToTIFF)
                    //{
                        //tiffFile = ImageUtilities.ConvertImageStreamToTIFF(source, false);
                    //}
                    //else
                    //{
                        //tiffFile = null;
                    //}
                    if (tiffFile != null)
                    {
                        dfile = Path.ChangeExtension(dfile, "tiff");
                        if (File.Exists(dfile) == true)
                        {
                            if (FileUtilities.FileAndStreamAreIdentical(dfile, tiffFile) == true)
                            {
                                File.SetAccessControl(dfile, fileSecurity);
                                source.Close();
                                tiffFile.Close();
                                continue;
                            }
                            newExt = Path.GetExtension(dfile);
                            newExt = rnd.Next(0, 10000).ToString("D5") + newExt;
                            Path.ChangeExtension(dfile, newExt);
                        }
                        dest = File.Create(dfile, 1024, FileOptions.SequentialScan, fileSecurity);
                        tiffFile.CopyTo(dest);
                        dest.Flush();
                        tiffFile.Close();
                    }
                    else
                    {
                        dest = File.Create(dfile, 1024, FileOptions.SequentialScan, fileSecurity);
                        source.CopyTo(dest);
                        dest.Flush();
                    }
                    dest.Close();
                    source.Close();

                    fa1 = File.GetAttributes(file);
                    fa2 = File.GetAttributes(dfile);
                    File.SetAttributes(dfile, (fa1 | fa2));
                    File.SetCreationTimeUtc(dfile, File.GetCreationTimeUtc(file));
                    File.SetLastAccessTimeUtc(dfile, File.GetLastWriteTimeUtc(file));
                    File.SetLastWriteTimeUtc(dfile, File.GetLastWriteTimeUtc(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not copy {0} to {1}. {2} : {3}", file, dfile, ex.Message, ex.StackTrace);
                    continue;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs a byte for byte comparison of two files and returns <value>true</value> if they are identical.
        /// </summary>
        /// <param name="file1">The full path of the first file.</param>
        /// <param name="file2">The full path of the second file.</param>
        /// <returns>Returns <code>true</code> if the files are identical and <code>false</code> otherwise.</returns>
        public static bool FilesAreIdentical(string file1, string file2)
        {
            if ((String.IsNullOrWhiteSpace(file1) == true) || (String.IsNullOrWhiteSpace(file2) == true)) return false;

            if (File.Exists(file1) == false) return false;
            if (File.Exists(file2) == false) return false;
            byte[] array1 = new byte[1024];
            byte[] array2 = new byte[1024];

            try
            {
                FileStream fileStream1 = File.Open(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
                FileStream fileStream2 = File.Open(file2, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fileStream1.Length != fileStream2.Length)
                {
                    fileStream1.Close();
                    fileStream2.Close();
                    return false;
                }
                while (fileStream1.Position != fileStream1.Length)
                {
                    fileStream1.Read(array1, 0, 1024);
                    fileStream2.Read(array2, 0, 1024);
                    if (array1.SequenceEqual(array2) == false) return false;
                }
            }
            catch
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// Performs a byte for byte comparison of a file and a System.Stream and returns <value>true</value> if they are identical.
        /// </summary>
        /// <param name="file">The full path of the file.</param>
        /// <param name="stream">The stream to compare to the file.</param>
        /// <returns>Returns <value>true</value> if the file and stream are identical and <value>false</value> otherwise.</returns>
        public static bool FileAndStreamAreIdentical(string file, Stream stream)
        {
            if ((String.IsNullOrWhiteSpace(file) == true) || (stream == null)) return false;

            if (File.Exists(file) == false) return false;
            if (stream.Length == 0) return false;
            byte[] array1 = new byte[1024];
            byte[] array2 = new byte[1024];
            long offset = 0;

            try
            {
                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                offset = stream.Position;
                if (stream.CanSeek == true) stream.Seek(0, SeekOrigin.Begin);
                if (fileStream.Length != stream.Length)
                {
                    fileStream.Close();
                    return false;
                }
                while (fileStream.Position != fileStream.Length)
                {
                    fileStream.Read(array1, 0, 1024);
                    stream.Read(array2, 0, 1024);
                    if (array1.SequenceEqual(array2) == false) return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (stream.CanSeek == true) stream.Seek(offset, SeekOrigin.Begin);
            }


            return true;
        }

        public static bool FileIsHiddenOrSystem(string file)
        {
            if (string.IsNullOrWhiteSpace(file) == true) return false;
            if (File.Exists(file) == false) return false;
            FileAttributes fa = File.GetAttributes(file);
            if ((fa & FileAttributes.Hidden) == FileAttributes.Hidden) return true;
            if ((fa & FileAttributes.System) == FileAttributes.System) return true;

            return false;
        }

        public static bool DirectoryIsHiddenOrSystem(string file)
        {
            if (string.IsNullOrWhiteSpace(file) == true) return false;
            if (Directory.Exists(file) == false) return false;
            FileAttributes fa = File.GetAttributes(file);
            if ((fa & FileAttributes.Hidden) == FileAttributes.Hidden) return true;
            if ((fa & FileAttributes.System) == FileAttributes.System) return true;

            return false;
        }

        /// <summary>
        /// Determines if a file is located a hidden or system directory, or a directory that descends from a hidden or system directory.
        /// </summary>
        /// <param name="file">The full path of the file to check.</param>
        /// <returns><value>true</value> if the file is located in a hidden or system directory, <value>false</value> otherwise.</returns>
        public static bool FileIsInHiddenOrSystemDirectory(string file)
        {
            if (String.IsNullOrWhiteSpace(file) == true) return false;
            if (Directory.Exists(file) == true) return false;
            if (File.Exists(file) == false) return false;
            file = Path.GetFullPath(file);
            string dir = Path.GetDirectoryName(file);
            if ((file.Contains("\\.") == true) && (file.EndsWith("\\.") == false)) return true;

            while (String.IsNullOrWhiteSpace(dir) == false)
            {
                if (dir == Path.GetPathRoot(file)) return false;
                if (DirectoryIsHiddenOrSystem(dir) == true) return true;
                dir = Path.GetDirectoryName(dir);
            }

            return false;
        }
    }
}
