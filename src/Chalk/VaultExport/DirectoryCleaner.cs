using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Chalk.VaultExport
{
    public class DirectoryCleaner
    {
        /// <summary>
        /// Files and directories not iriginating from Vault, that should be left when purging workspace. They are only in root.
        /// </summary>
        static readonly List<string> UndeletableRootNames = new List<string> { ".git", ".gitignore", ".chalk-lastversion" };

        readonly IFileSystem fileSystem;

        public DirectoryCleaner(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void DeleteContents(string path)
        {
            DirectoryInfoBase directory = fileSystem.DirectoryInfo.FromDirectoryName(path);

            foreach (FileInfoBase file in directory.GetFiles().Where(file => !UndeletableRootNames.Contains(file.Name)))
                file.Delete();

            foreach (DirectoryInfoBase childDirectory in directory.GetDirectories().Where(childDirectory => !UndeletableRootNames.Contains(childDirectory.Name)))
            {
                childDirectory.Delete(true);
            }
        }
    }
}
