using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{
    /// <summary>
    /// Provides values to indicate why a directory search has come to an end.
    /// </summary>
    public enum DirectrorySearchEndReason
    {
        /// <summary>
        /// The search of the directory completed normally.
        /// </summary>
        Finished,
        /// <summary>
        /// The search of the directory was cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// A Fatal error ocurred and it was impossible for processing to continue.
        /// </summary>
        FatalError
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides values to indicate what action should be taken in the event that a symbolic link is encountered.
    /// </summary>
    public enum SymbolicLinkBehaviour
    {
        /// <summary>
        /// Skip the link and do not raise an event. This is the default.
        /// </summary>
        Ignore,
        /// <summary>
        /// Follow the link and return its target by raising an event.
        /// </summary>
        Follow,
        /// <summary>
        /// Raise an event and return the link as it is.
        /// </summary>
        Return
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides data for the DirectorySearcher.FileFound event.
    /// </summary>
    public class FileFoundEventArgs : EventArgs
    {
        private bool m_Cancel;
        private FileInfo m_File;
        private object m_clientData;

        /// <summary>
        /// Creates a new instance of the FileFoundEventArgs class with the given file info and client data.
        /// </summary>
        /// <param name="info">A System.IO.FileInfo object for the file that this instance will provide data for.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal FileFoundEventArgs(System.IO.FileInfo info, object clientData)
        {
            m_Cancel = false;
            m_File = info;
            m_clientData = clientData;
        }

        /// <summary>
        /// Gets a System.IO.FileInfo object for the file that this instance will provide data for.
        /// </summary>
        public System.IO.FileInfo File
        {
            get
            {
                return m_File;
            }
        }

        /// <summary>
        /// Gets or sets a flag to indicate whether enumeration of the directory should be cancelled.
        /// </summary>
        public bool Cancel
        {
            get
            {
                return m_Cancel;
            }
            set
            {
                m_Cancel = value;
            }
        }

        /// <summary>
        /// Gets the client data object that was passed when the DirectorySearcher instance was created.
        /// </summary>
        public object ClientData
        {
            get
            {
                return m_clientData;
            }
        }
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides data for the DirectorySearcher.DirectoryFound event.
    /// </summary>
    public class DirectoryFoundEventArgs : EventArgs
    {
        private bool m_Cancel;
        private DirectoryInfo m_Directory;
        private object m_ClientData;

        /// <summary>
        /// Initializes a new instance of the DirectoryFoundEventArgs class with the specified directory info and client data.
        /// </summary>
        /// <param name="info">A System.IO.DirectoryInfo object for the directory that this instance will provide data for.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal DirectoryFoundEventArgs(System.IO.DirectoryInfo info, object clientData)
        {
            m_Cancel = false;
            m_Directory = info;
            m_ClientData = clientData;
        }

        /// <summary>
        /// Gets a System.IO.DirectoryInfo object for the directory that this instance will provide data for.
        /// </summary>
        public System.IO.DirectoryInfo Directory
        {
            get
            {
                return m_Directory;
            }
        }

        /// <summary>
        /// Gets or sets a flag to indicate whether enumeration of the directory should be cancelled.
        /// </summary>
        public bool Cancel
        {
            get
            {
                return m_Cancel;
            }
            set
            {
                m_Cancel = value;
            }
        }

        /// <summary>
        /// Gets the client data object that was passed when the DirectorySearcher instance was created.
        /// </summary>
        public object ClientData
        {
            get
            {
                return m_ClientData;
            }
        }
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides data for the DirectorySearcher.SearchError event.
    /// </summary>
    public class SearchErrorEventArgs : EventArgs
    {
        private bool m_Cancel;
        private string m_Error;
        private Exception m_Exception;
        private object m_ClientData;

        /// <summary>
        /// Initializes a new instance of the SearchErrorEventArgs class whith the specified message and exception.
        /// </summary>
        /// <param name="error">The text of the error event that this instance will provide data for.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal SearchErrorEventArgs(string error, Exception exception, object clientData)
        {
            m_Cancel = false;
            m_Error = error;
            m_Exception = exception;
            m_ClientData = clientData;
        }

        /// <summary>
        /// Gets a string representation of the error that this instance will provide data for.
        /// </summary>
        public string Error
        {
            get
            {
                return m_Error;
            }
        }

        /// <summary>
        /// Gets the exception that caused this error event (if there is one).
        /// </summary>
        public Exception Exception
        {
            get
            {
                return m_Exception;
            }
        }

        /// <summary>
        /// Gets or sets a flag to indicate whether enumeration of the directory should be cancelled.
        /// </summary>
        public bool Cancel
        {
            get
            {
                return m_Cancel;
            }
            set
            {
                m_Cancel = value;
            }
        }

        /// <summary>
        /// Gets the client data object that was passed when the DirectorySearcher instance was created.
        /// </summary>
        public object ClientData
        {
            get
            {
                return m_ClientData;
            }
        }
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides data for the DirectorySearcher.SearchEnded event.
    /// </summary>
    public class SearchEndedEventArgs : EventArgs
    {
        private object m_ClientData;
        private DirectrorySearchEndReason m_Reason;

        /// <summary>
        /// Initializes a new instance of the SearchEndedEventArgs class whith the specified reason.
        /// </summary>
        /// <param name="reason">A DirectrorySearchEndReason enumeration value indicating the reason for the event that this instance is providing data for.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal SearchEndedEventArgs(DirectrorySearchEndReason reason, object clientData)
        {
            m_Reason = reason;
            m_ClientData = clientData;
        }

        /// <summary>
        /// Gets the reason that this event has been raised.
        /// </summary>
        public DirectrorySearchEndReason Reason
        {
            get
            {
                return m_Reason;
            }
        }

        /// <summary>
        /// Gets the client data object that was passed when the DirectorySearcher instance was created.
        /// </summary>
        public object ClientData
        {
            get
            {
                return m_ClientData;
            }
        }
    }

    // =================================================================================================================================================

    /// <summary>
    /// Represents the method that will handle the DirectorySearcher.DirectoryFound event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A DirectoryFoundEventArgs object that contains the event data.</param>
    public delegate void DirectoryFoundEventHander(object sender, DirectoryFoundEventArgs e);
    /// <summary>
    /// Represents the method that will handle the DirectorySearcher.FileFound event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A FileFoundEventArgs object that contains the event data.</param>
    public delegate void FileFoundEventHander(object sender, FileFoundEventArgs e);
    /// <summary>
    /// Represents the method that will handle the DirectorySearcher.SearchError event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An DirectorySearchErrorEventArgs object that contains the event data.</param>
    public delegate void SearchErrorEventHandler(object sender, SearchErrorEventArgs e);
    /// <summary>
    /// Represents the method that will handle the DirectorySearcher.SearchEnded event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SearchEndedEventArgs object that contains the event data.</param>
    public delegate void SearchEndedEventHandler(object sender, SearchEndedEventArgs e);

    // =================================================================================================================================================

    /// <summary>
    /// Walks a given directory (and optionally any sub-directories) and returns information on any entries found.
    /// </summary>
    public class DirectorySearcher
    {
        private System.Object lockThis = new System.Object(); // Used for thread synchronisation.
        // The CharSet must match the CharSet of the corresponding PInvoke signature
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr hFindFile);

        /// <summary>
        /// The event that is raised when a directory is found.
        /// </summary>
        public event DirectoryFoundEventHander DirectoryFound;
        /// <summary>
        /// The event that is raised when a file is found.
        /// </summary>
        public event FileFoundEventHander FileFound;
        /// <summary>
        /// The event that is raised when an error occurs during the search.
        /// </summary>
        public event SearchErrorEventHandler SearchError;
        /// <summary>
        /// The event that is raised when the search ends.
        /// </summary>
        public event SearchEndedEventHandler SearchEnded;
        private object m_ClientData;
        private string m_SearchPath;
        private System.IO.SearchOption m_SearchOption;
        private SymbolicLinkBehaviour m_LinkToFileAction;
        private SymbolicLinkBehaviour m_LinkToDirectoryAction;
        public bool m_Cancelled;

        /// <summary>
        /// Creates a new instance of the DirectorySearcher class with the given path and default search option.
        /// </summary>
        /// <param name="path">The full path of the directory to start searching.</param>
        public DirectorySearcher(string path) : this(path, SearchOption.TopDirectoryOnly, null)
        {

        }

        /// <summary>
        /// Creates a new instance of the DirectorySearcher class with the given path, client data and default search option.
        /// </summary>
        /// <param name="path">The full path of the directory to start searching.</param>
        /// <param name="clientData">A user defined client data object that is passed to the consumers of this instance's events.</param>
        public DirectorySearcher(string path, object clientData) : this(path, SearchOption.TopDirectoryOnly, clientData)
        {

        }

        /// <summary>
        /// Creates a new instance of the DirectorySearcher class with the given path, client data and search option.
        /// </summary>
        /// <param name="path">The full path of the directory to start searching.</param>
        /// <param name="searchOption">Specifies whether to to search just the directory or its sub-directores as well.</param>
        /// <param name="clientData">A user defined client data object that is passed to the consumers of this instance's events.</param>
        public DirectorySearcher(string path, System.IO.SearchOption searchOption, object clientData)
        {
            if (path == null) throw new ArgumentNullException("path");
            if ((String.IsNullOrWhiteSpace(path) == true) || (Directory.Exists(path) == false)) throw new ArgumentException("Invalid search path", "path");
            m_SearchOption = searchOption;
            m_SearchPath = path;
            m_ClientData = clientData;
            m_LinkToDirectoryAction = SymbolicLinkBehaviour.Ignore;
            m_LinkToFileAction = SymbolicLinkBehaviour.Ignore;
            m_Cancelled = false;
        }

        /// <summary>
        /// Raises the DirectorySearcher.DirectoryFound event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnDirectoryFoundEvent(DirectoryFoundEventArgs e)
        {
            if (this.DirectoryFound != null) this.DirectoryFound(this, e);
        }

        /// <summary>
        /// Raises the DirectorySearcher.FileFound event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnFileFoundEvent(FileFoundEventArgs e)
        {
            if (this.FileFound != null) this.FileFound(this, e);
        }

        /// <summary>
        /// Raises the DirectorySearcher.SearchError event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnSearchErrorEvent(SearchErrorEventArgs e)
        {
            if (this.SearchError != null) this.SearchError(this, e);
        }

        /// <summary>
        /// Raises the DirectorySearcher.SearchEnded event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnSearchEndedEvent(SearchEndedEventArgs e)
        {
            if (this.SearchEnded != null) this.SearchEnded(this, e);
        }

        /// <summary>
        /// Get a value to indicate that the current search has been cancelled and is no longer running.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return m_Cancelled;
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate what action should be taken if a symbolic link to a directory is encountered.
        /// </summary>
        public SymbolicLinkBehaviour LinkToDirectoryAction
        {
            get
            {
                return m_LinkToDirectoryAction;
            }
            set
            {
                m_LinkToDirectoryAction = value;
            }
        }

        public void StartSearch()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("ClientData", m_ClientData);
            paramList.Add("SearchOptions", m_SearchOption);
            paramList.Add("DirectorySymlinkAction", m_LinkToDirectoryAction);
            paramList.Add("FileSymlinkAction", m_LinkToFileAction);

            // If this call returns false, then it was a non-starter.
            if (this.SearchDirectory(m_SearchPath, paramList) == false)
            {
                SearchEndedEventArgs e = new SearchEndedEventArgs(DirectrorySearchEndReason.FatalError, paramList["ClientData"]);
                this.OnSearchEndedEvent(e);
            }
        }

        public void StartSearchAsync()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(this.StartThread));
            thread.Start(m_SearchPath);
        }

        internal void StartThread(object param)
        {
            string searchPath = (string)param;
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            // Obtain a lock, pack the parameters up and start
            lock (lockThis)
            {
                paramList.Add("ClientData", m_ClientData);
                paramList.Add("SearchOptions", m_SearchOption);
                paramList.Add("DirectorySymlinkAction", m_LinkToDirectoryAction);
                paramList.Add("FileSymlinkAction", m_LinkToFileAction);
            }

            // If this call returns false, then it was a non-starter.
            if (this.SearchDirectory(searchPath, paramList) == false)
            {
                SearchEndedEventArgs e = new SearchEndedEventArgs(DirectrorySearchEndReason.FatalError, paramList["ClientData"]);
                this.OnSearchEndedEvent(e);
            }
        }

        private bool SearchDirectory(string path, Dictionary<string, object> paramList)
        {
            if (this.Cancelled == true) return false;
            object clientData = paramList["ClientData"];
            SearchOption searchOption = (SearchOption)paramList["SearchOptions"];
            SymbolicLinkBehaviour dirAction = (SymbolicLinkBehaviour)paramList["DirectorySymlinkAction"];
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            WIN32_FIND_DATA win32_fd;
            string rPath;
            IntPtr hFind = FindFirstFile(Path.Combine(path, "*.*"), out win32_fd);

            if (hFind == INVALID_HANDLE_VALUE)
            {
                Win32Exception we = new Win32Exception(Marshal.GetLastWin32Error());
                SearchErrorEventArgs e = new SearchErrorEventArgs("Error: An error occurred searching '" + path + "' - " + we.Message, we, clientData);
                this.OnSearchErrorEvent(e);
                return false;
            }

            try
            {
                do
                {
                    if ((win32_fd.cFileName == ".") || (win32_fd.cFileName == "..")) continue;
                    {
                        rPath = Path.Combine(path, win32_fd.cFileName);
                        if (File.GetAttributes(rPath).HasFlag(FileAttributes.Directory) == true)
                        {
                            // The entry is a directory.
                            DirectoryInfo di = new DirectoryInfo(rPath);
                            // Check if this directory is a junction or symbolic link.
                            if (di.Attributes.HasFlag(FileAttributes.ReparsePoint) == true)
                            {
                                switch (dirAction)
                                {
                                    case SymbolicLinkBehaviour.Ignore:
                                        continue;
                                    case SymbolicLinkBehaviour.Follow:
                                        break;
                                    case SymbolicLinkBehaviour.Return:
                                        break;
                                }
                            }
                            // Raise an event and check that the client has not cancelled the operation.
                            DirectoryFoundEventArgs e = new DirectoryFoundEventArgs(di, clientData);
                            this.OnDirectoryFoundEvent(e);
                            if (e.Cancel == true)
                            {
                                m_Cancelled = true;
                                SearchEndedEventArgs ec = new SearchEndedEventArgs(DirectrorySearchEndReason.Cancelled, clientData);
                                this.OnSearchEndedEvent(ec);
                                return false;
                            }
                            // Check if we are recursing.
                            if (searchOption == SearchOption.AllDirectories) this.SearchDirectory(rPath, paramList);
                        }
                        else
                        {
                            // The entry is some kind of file.
                            FileInfo fi = new FileInfo(rPath);
                            // Raise an event and check that the client has not cancelled the operation.
                            FileFoundEventArgs e = new FileFoundEventArgs(fi, clientData);
                            this.OnFileFoundEvent(e);
                            if (e.Cancel == true)
                            {
                                m_Cancelled = true;
                                SearchEndedEventArgs ec = new SearchEndedEventArgs(DirectrorySearchEndReason.Cancelled, clientData);
                                this.OnSearchEndedEvent(ec);
                                return false;
                            }
                        }
                    }
                }
                while ((FindNextFile(hFind, out win32_fd) == true) && (this.Cancelled == false));
            }
            catch (Exception ex)
            {
                SearchErrorEventArgs e = new SearchErrorEventArgs("Error: An error occurred searching '" + path + "' - " + ex.Message, ex, clientData);
                this.OnSearchErrorEvent(e);
                return false;
            }
            FindClose(hFind);

            return m_Cancelled;
        }
    }
}
