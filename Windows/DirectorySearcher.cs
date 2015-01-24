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
    /// Provides values to indicate why a directory search being carried out by a DirectorySearcher instance has come to an end.
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

    /// <summary>
    /// Provides values to indicate what events should be raised by the a DirectorySearcher instance during a search.
    /// </summary>
    [Flags]
    public enum DirectorySearchEventMask
    {
        /// <summary>
        /// Raise events for files.
        /// </summary>
        Files = 1,
        /// <summary>
        /// Raise Events for directories.
        /// </summary>
        Directores = 2,
        /// <summary>
        /// Raise events for both files and directories.
        /// </summary>
        Both = Directores | Files
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides values to indicate what action should be taken in the event that a symbolic link is encountered by a DirectorySearcher instance during a search.
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
        private string m_Directory;

        /// <summary>
        /// Initializes a new instance of the SearchErrorEventArgs class whith the specified message, exception and directory
        /// </summary>
        /// <param name="error">The text of the error event that this instance will provide data for.</param>
        /// <param name="exception">The Exception that caused the error event that this instance will provide data for.</param>
        /// <param name="directory">The directory that was being searched when the error event that this instance will provide data for occurred.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal SearchErrorEventArgs(string error, Exception exception, string directory, object clientData)
        {
            m_Cancel = false;
            m_Error = error;
            m_Exception = exception;
            m_Directory = directory;
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
        /// The directory that the DirectorySearcher instance was searching, or attempting to search when this error occurred.
        /// </summary>
        public string Directory
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
        private bool m_Cancelled;
        private DirectorySearchEventMask m_EventMask;
        private long m_DirectoriesSearched;
        private long m_FilesFound;
        private bool m_SearchRunning;

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
            m_SearchPath = Path.GetFullPath(path);
            m_ClientData = clientData;
            m_LinkToDirectoryAction = SymbolicLinkBehaviour.Ignore;
            m_LinkToFileAction = SymbolicLinkBehaviour.Ignore;
            m_Cancelled = false;
            m_EventMask = DirectorySearchEventMask.Both;
            m_SearchRunning = false;
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
            m_FilesFound++;
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

        /// <summary>
        /// Gets or sets the path to be used as the starting point of the search.
        /// </summary>
        /// <remarks>
        /// The value assigned to this property must be a valid path to an existing and accessible directory. If this is not the case an exception will be thrown.
        /// Changing this property while a search is in progress will have no effect until the search is stopped and restarted.
        /// </remarks>
        public string SearchPath
        {
            get
            {
                return m_SearchPath;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "The value assigned to this property cannot be null");
                if (String.IsNullOrWhiteSpace(value) == true) throw new ArgumentException("The value assigned to this property must be a valid path to a directory.", "value");
                if (Directory.Exists(value) == false) throw new DirectoryNotFoundException(String.Format("The path '{0}' does not exist.", value));
                m_SearchPath = Path.GetFullPath(value);
            }
        }

        /// <summary>
        /// Gets or sets an enumeration value to indicate what events this instance should raise during a search.
        /// </summary>
        public DirectorySearchEventMask EventMask
        {
            get
            {
                return m_EventMask;
            }
            set
            {
                m_EventMask = value;
            }
        }

        /// <summary>
        /// Gets a flag to indicate if this instance is currently running a search.
        /// </summary>
        public bool SearchRunning
        {
            get
            {
                return m_SearchRunning;
            }
        }

        /// <summary>
        /// Get the number of directories successfully searched.
        /// </summary>
        public long DirectoriesSearched
        {
            get
            {
                return m_DirectoriesSearched;
            }
        }

        /// <summary>
        /// Gets the number of files found in this search.
        /// </summary>
        public long FilesFound
        {
            get
            {
                return m_FilesFound;
            }
        }

        /// <summary>
        /// Starts a synchronous (blocking) search of the given directory.
        /// </summary>
        public void StartSearch()
        {
            if (m_SearchRunning == true) return;
            m_SearchRunning = true;
            m_DirectoriesSearched = 0;
            m_FilesFound = 0;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("ClientData", m_ClientData);
            paramList.Add("SearchOptions", m_SearchOption);
            paramList.Add("DirectorySymlinkAction", m_LinkToDirectoryAction);
            paramList.Add("FileSymlinkAction", m_LinkToFileAction);
            paramList.Add("EventMask", m_EventMask);
            SearchEndedEventArgs e;
            DirectrorySearchEndReason reason = DirectrorySearchEndReason.Finished;

            // If this call returns false, then it was a non-starter.
            if (this.SearchDirectory(m_SearchPath, paramList) == false)
            {
                reason = (m_Cancelled == true) ? DirectrorySearchEndReason.Cancelled : DirectrorySearchEndReason.FatalError;
            }
            else
            {
                reason = DirectrorySearchEndReason.Finished;
            }

            e = new SearchEndedEventArgs(reason, paramList["ClientData"]);
            this.OnSearchEndedEvent(e);
            m_SearchRunning = false;
        }

        /// <summary>
        /// Starts an asynchronous (non-blocking) search of the given directory in a new thread.
        /// </summary>
        public void StartSearchAsync()
        {
            if (m_SearchRunning == true) return;
            m_SearchRunning = true;
            m_DirectoriesSearched = 0;
            m_FilesFound = 0;
            Thread thread = new Thread(new ParameterizedThreadStart(this.StartThread));
            thread.Start(m_SearchPath);
        }

        /// <summary>
        /// Stops an asynchronous search.
        /// </summary>
        public void StopSearch()
        {
            m_Cancelled = true;
        }

        internal void StartThread(object param)
        {
            string searchPath = (string)param;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            SearchEndedEventArgs e;
            DirectrorySearchEndReason reason = DirectrorySearchEndReason.Finished;

            // Obtain a lock, pack the parameters up and start
            lock (lockThis)
            {
                paramList.Add("ClientData", m_ClientData);
                paramList.Add("SearchOptions", m_SearchOption);
                paramList.Add("DirectorySymlinkAction", m_LinkToDirectoryAction);
                paramList.Add("FileSymlinkAction", m_LinkToFileAction);
                paramList.Add("EventMask", m_EventMask);
            }

            // If this call returns false, then it was a non-starter.
            if (this.SearchDirectory(searchPath, paramList) == false)
            {
                reason = (m_Cancelled == true) ? DirectrorySearchEndReason.Cancelled : DirectrorySearchEndReason.FatalError;
            }
            else
            {
                reason = DirectrorySearchEndReason.Finished;
            }

            e = new SearchEndedEventArgs(reason, paramList["ClientData"]);
            this.OnSearchEndedEvent(e);
            m_SearchRunning = false;
        }

        private bool SearchDirectory(string path, Dictionary<string, object> paramList)
        {
            if (this.Cancelled == true) return false;
            object clientData = paramList["ClientData"];
            SearchOption searchOption = (SearchOption)paramList["SearchOptions"];
            SymbolicLinkBehaviour dirLinkAction = (SymbolicLinkBehaviour)paramList["DirectorySymlinkAction"];
            SymbolicLinkBehaviour fileLinkAction = (SymbolicLinkBehaviour)paramList["FileSymlinkAction"];
            DirectorySearchEventMask mask = (DirectorySearchEventMask)paramList["EventMask"];
            
            NativeMethods.WIN32_FIND_DATA win32_fd;
            string rPath = path;
            IntPtr hFind = NativeMethods.FindFirstFile(Path.Combine(path, "*.*"), out win32_fd);

            if (hFind == NativeMethods.INVALID_HANDLE_VALUE)
            {
                Win32Exception we = new Win32Exception(Marshal.GetLastWin32Error());
                SearchErrorEventArgs e = new SearchErrorEventArgs(we.Message, we, rPath, clientData);
                this.OnSearchErrorEvent(e);
                return false;
            }
            else
            {
                m_DirectoriesSearched++;
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
                                switch (dirLinkAction)
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
                            if (mask.HasFlag(DirectorySearchEventMask.Directores) == true)
                            {
                                DirectoryFoundEventArgs e = new DirectoryFoundEventArgs(di, clientData);
                                this.OnDirectoryFoundEvent(e);
                                if (e.Cancel == true)
                                {
                                    m_Cancelled = true;
                                    SearchEndedEventArgs ec = new SearchEndedEventArgs(DirectrorySearchEndReason.Cancelled, clientData);
                                    this.OnSearchEndedEvent(ec);
                                    return false;
                                }
                            }
                            // Check if we are recursing.
                            if (searchOption == SearchOption.AllDirectories) this.SearchDirectory(rPath, paramList);
                        }
                        else
                        {
                            // The entry is some kind of file.
                            FileInfo fi = new FileInfo(rPath);
                            // Check if this file is a junction or symbolic link.
                            if (fi.Attributes.HasFlag(FileAttributes.ReparsePoint) == true)
                            {
                                switch (fileLinkAction)
                                {
                                    case SymbolicLinkBehaviour.Ignore:
                                        continue;
                                    case SymbolicLinkBehaviour.Follow:
                                        break;
                                    case SymbolicLinkBehaviour.Return:
                                        break;
                                }
                            }
                            if (mask.HasFlag(DirectorySearchEventMask.Files) == true)
                            {
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
                }
                while ((NativeMethods.FindNextFile(hFind, out win32_fd) == true) && (this.Cancelled == false));
            }
            catch (Exception ex)
            {
                SearchErrorEventArgs e = new SearchErrorEventArgs(ex.Message, ex, rPath, clientData);
                this.OnSearchErrorEvent(e);
                return false;
            }
            NativeMethods.FindClose(hFind);

            return !m_Cancelled;
        }
    }
}
