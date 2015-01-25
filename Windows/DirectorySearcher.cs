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
    /// Walks a given directory (and optionally any sub-directories) and returns information on any entries found.
    /// </summary>
    public class DirectorySearcher : DirectoryOperation
    {
        private System.Object lockThis = new System.Object(); // Used for thread synchronization.

        /// <summary>
        /// The event that is raised when a directory is found.
        /// </summary>
        public event DirectoryFoundEventHander DirectoryFound;
        /// <summary>
        /// The event that is raised when a file is found.
        /// </summary>
        public event FileFoundEventHander FileFound;
        
        private string m_SearchPath;
        private System.IO.SearchOption m_SearchOption;
        private SymbolicLinkBehaviour m_LinkToFileAction;
        private SymbolicLinkBehaviour m_LinkToDirectoryAction;
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
        /// <param name="searchOption">Specifies whether to search just the directory or its sub-directories as well.</param>
        /// <param name="clientData">A user defined client data object that is passed to the consumers of this instance's events.</param>
        public DirectorySearcher(string path, System.IO.SearchOption searchOption, object clientData) : base(clientData)
        {
            if (path == null) throw new ArgumentNullException("path");
            if ((String.IsNullOrWhiteSpace(path) == true) || (Directory.Exists(path) == false)) throw new ArgumentException("Invalid search path", "path");
            m_SearchOption = searchOption;
            m_SearchPath = Path.GetFullPath(path);
            m_LinkToDirectoryAction = SymbolicLinkBehaviour.Ignore;
            m_LinkToFileAction = SymbolicLinkBehaviour.Ignore;
            m_EventMask = DirectorySearchEventMask.Both;
            m_SearchRunning = false;
            m_FilesFound = 0;
            m_DirectoriesSearched = 0;
        }

        /// <summary>
        /// Raises the DirectorySearcher.DirectoryFound event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnDirectoryFound(DirectoryFoundEventArgs e)
        {
            if (this.DirectoryFound != null) this.DirectoryFound(this, e);
        }

        /// <summary>
        /// Raises the DirectorySearcher.FileFound event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected void OnFileFound(FileFoundEventArgs e)
        {
            m_FilesFound++;
            if (this.FileFound != null) this.FileFound(this, e);
        }

        protected override void OnOperationError(OperationErrorEventArgs e)
        {
            m_Errors++;
            base.OnOperationError(e);
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
            m_Cancelled = false;
            m_Errors = 0;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("ClientData", m_ClientData);
            paramList.Add("SearchOptions", m_SearchOption);
            paramList.Add("DirectorySymlinkAction", m_LinkToDirectoryAction);
            paramList.Add("FileSymlinkAction", m_LinkToFileAction);
            paramList.Add("EventMask", m_EventMask);
            OperationEndedEventArgs e;
            DirectroryOperationEndReason reason = DirectroryOperationEndReason.Finished;

            // If this call returns false, then it was a non-starter.
            if (this.SearchDirectory(m_SearchPath, paramList) == false)
            {
                reason = (m_Cancelled == true) ? DirectroryOperationEndReason.Cancelled : DirectroryOperationEndReason.FatalError;
            }
            else
            {
                reason = DirectroryOperationEndReason.Finished;
            }

            e = new OperationEndedEventArgs(reason, paramList["ClientData"]);
            this.OnOperationEnded(e);
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
            m_Cancelled = false;
            m_Errors = 0;
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
            OperationEndedEventArgs e;
            DirectroryOperationEndReason reason = DirectroryOperationEndReason.Finished;

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
                reason = (m_Cancelled == true) ? DirectroryOperationEndReason.Cancelled : DirectroryOperationEndReason.FatalError;
            }
            else
            {
                reason = DirectroryOperationEndReason.Finished;
            }

            e = new OperationEndedEventArgs(reason, paramList["ClientData"]);
            this.OnOperationEnded(e);
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
                OperationErrorEventArgs e = new OperationErrorEventArgs(we.Message, we, rPath, clientData);
                this.OnOperationError(e);
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
                                this.OnDirectoryFound(e);
                                if (e.Cancel == true)
                                {
                                    m_Cancelled = true;
                                    OperationEndedEventArgs ec = new OperationEndedEventArgs(DirectroryOperationEndReason.Cancelled, clientData);
                                    this.OnOperationEnded(ec);
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
                                this.OnFileFound(e);
                                if (e.Cancel == true)
                                {
                                    m_Cancelled = true;
                                    OperationEndedEventArgs ec = new OperationEndedEventArgs(DirectroryOperationEndReason.Cancelled, clientData);
                                    this.OnOperationEnded(ec);
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
                OperationErrorEventArgs e = new OperationErrorEventArgs(ex.Message, ex, rPath, clientData);
                this.OnOperationError(e);
                return false;
            }
            NativeMethods.FindClose(hFind);

            return !m_Cancelled;
        }
    }
}
