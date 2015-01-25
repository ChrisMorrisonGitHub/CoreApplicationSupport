using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{
    public class DirectoryDuplicator : DirectoryOperation
    {
        private string m_SourcePath;
        private string m_destinationPath;
        private DuplicateOptions m_DuplicateOptions;
        private CollisionAction m_CollisionAction;

        public DirectoryDuplicator(string sourcePath, string destPath) : this(sourcePath, destPath, null, DuplicateOptions.MergeExistingDirectories, CollisionAction.RenameDifferentExistingFiles) 
        {

        }

        public DirectoryDuplicator(string sourcePath, string destPath, object clientData) : this(sourcePath, destPath, clientData, DuplicateOptions.MergeExistingDirectories, CollisionAction.RenameDifferentExistingFiles)
        {

        }

        public DirectoryDuplicator(string sourcePath, string destPath, object clientData, DuplicateOptions options, CollisionAction collisionAction) : base(clientData)
        {
            m_SourcePath = sourcePath;
            m_destinationPath = destPath;
            m_DuplicateOptions = options;
            m_CollisionAction = collisionAction;
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void StartAsync()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
