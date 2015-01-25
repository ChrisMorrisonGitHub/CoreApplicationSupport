using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{
    /// <summary>
    /// Provides options to specify how the DuplicateFolder class will copy files and folders.
    /// </summary>
    [Flags]
    public enum DuplicateOptions
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// If the destination directory exists, delete it and replace it with the source directory.
        /// </summary>
        MergeExistingDirectories = 1,
        /// <summary>
        /// Do not copy zero byte files.
        /// </summary>
        SkipZeroByteFiles = 2,
        /// <summary>
        /// Copy only the directory structure, do not copy any files.
        /// </summary>
        DirectoryStructureOnly = 4,
        /// <summary>
        /// Do not copy system files, i.e. those with the hidden or system bit set in their attributes.
        /// </summary>
        SkipSystemFiles = 8,
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides options to specify how the DuplicateFolder class will handle filename collisions.
    /// </summary>
    public enum CollisionAction
    {
        /// <summary>
        /// If a file with the same name exists in the destination, delete it and replace it with the file being copied.
        /// </summary>
        OverwriteExistingFiles,
        /// <summary>
        /// If a file with the same name exists in the destination, skip copying the file.
        /// </summary>
        KeepExistingFiles,
        /// <summary>
        /// If a file with the same name exists in the destination, copy the source file but rename it.
        /// </summary>
        RenameAnyExistingFiles,
        /// <summary>
        /// If a file with the same name exists in the destination, copy and rename the source file only if its contents differ from the destination. This is the default action.
        /// </summary>
        RenameDifferentExistingFiles
    }

    // =================================================================================================================================================

    /// <summary>
    /// Provides values to indicate why a directory operation has come to an end.
    /// </summary>
    public enum DirectroryOperationEndReason
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
        /// A Fatal error occurred and it was impossible for processing to continue.
        /// </summary>
        FatalError
    }

    // =================================================================================================================================================

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
    public class OperationErrorEventArgs : EventArgs
    {
        private bool m_Cancel;
        private string m_Error;
        private Exception m_Exception;
        private object m_ClientData;
        private string m_Directory;

        /// <summary>
        /// Initializes a new instance of the OperationErrorEventArgs class with the specified message, exception and directory
        /// </summary>
        /// <param name="error">The text of the error event that this instance will provide data for.</param>
        /// <param name="exception">The Exception that caused the error event that this instance will provide data for.</param>
        /// <param name="directory">The directory that was being operated when the error event that this instance will provide data for occurred.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal OperationErrorEventArgs(string error, Exception exception, string directory, object clientData)
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
    public class OperationEndedEventArgs : EventArgs
    {
        private object m_ClientData;
        private DirectroryOperationEndReason m_Reason;

        /// <summary>
        /// Initializes a new instance of the SearchEndedEventArgs class with the specified reason.
        /// </summary>
        /// <param name="reason">A DirectrorySearchEndReason enumeration value indicating the reason for the event that this instance is providing data for.</param>
        /// <param name="clientData">The client data object that was passed when the DirectorySearcher instance was created.</param>
        internal OperationEndedEventArgs(DirectroryOperationEndReason reason, object clientData)
        {
            m_Reason = reason;
            m_ClientData = clientData;
        }

        /// <summary>
        /// Gets the reason that this event has been raised.
        /// </summary>
        public DirectroryOperationEndReason Reason
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
    public delegate void OperationErrorEventHandler(object sender, OperationErrorEventArgs e);
    /// <summary>
    /// Represents the method that will handle the DirectorySearcher.SearchEnded event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SearchEndedEventArgs object that contains the event data.</param>
    public delegate void OperationEndedEventHandler(object sender, OperationEndedEventArgs e);

    // =================================================================================================================================================
    
    /// <summary>
    /// The abstract class from which classes that perform operations on directories will derive.
    /// </summary>
    public abstract class DirectoryOperation
    {
        protected object m_ClientData;
        protected long m_Errors;
        protected bool m_Cancelled;

        protected DirectoryOperation(object clientData)
        {
            m_Cancelled = false;
            m_ClientData = clientData;
            m_Errors = 0;
        }

        /// <summary>
        /// The event that is raised when an error occurs during the operation.
        /// </summary>
        public event OperationErrorEventHandler OperationError;
        /// <summary>
        /// The event that is raised when the operation ends.
        /// </summary>
        public event OperationEndedEventHandler OperationEnded;

        /// <summary>
        /// Raises the DirectoryOperation.OperationEnded event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected virtual void OnOperationEnded(OperationEndedEventArgs e)
        {
            if (this.OperationEnded != null) this.OperationEnded(this, e);
        }

        /// <summary>
        /// Raises the DirectorySearcher.OperationError event.
        /// </summary>
        /// <param name="e">The data for this event.</param>
        protected virtual void OnOperationError(OperationErrorEventArgs e)
        {
            if (this.OperationError != null) this.OperationError(this, e);
        }

        /// <summary>
        /// Gets the client data for this instance.
        /// </summary>
        public object ClientData
        {
            get
            {
                return m_ClientData;
            }
        }

        /// <summary>
        /// Get a value to indicate that the current operation has been cancelled and is no longer running.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return m_Cancelled;
            }
        }

        /// <summary>
        /// Gets the number of error that occurred during the last operation.
        /// </summary>
        public long Errors
        {
            get
            {
                return m_Errors;
            }
        }
    }
}
