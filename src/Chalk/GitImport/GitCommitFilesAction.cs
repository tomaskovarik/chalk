using System;
using System.Globalization;
using System.Text;
using Chalk.Actions;
using Chalk.Interop;
using Chalk.VaultExport.Interop;
using Seterlund.CodeGuard;
using CommandLineClient = Chalk.GitImport.Interop.CommandLineClient;

namespace Chalk.GitImport
{
    class GitCommitFilesAction
    {
        const string AddCommand = "add";
        const string AllSwitchArgument = "all";
        const string CommitCommand = "commit";
        const string MessageSourceFileArgument = "file";
        const string AllowEmptySwitchArgument = "allow-empty";
        const string AuthorArgument = "author";
        const string DateArgument = "date";
        const string UseStandardInputArgumentValue = "-";
        const string CommitAuthorFormat = "{0} <{0}@{1}>";
        const string Iso8601DateTimeFormat = "s";

        const string VaultExportSourceNoteFormat = "(Exported from Vault repository {0} and path {1} at version {2})";

        readonly ActionContext context;
        readonly CommandLineClient gitClient;

        public GitCommitFilesAction(ActionContext context, CommandLineClient gitClient)
        {
            Guard.That(context).IsNotNull();
            Guard.That(gitClient).IsNotNull();
            this.context = context;
            this.gitClient = gitClient;
        }

        public void Execute(VersionHistoryItem historyItem)
        {
            StageFiles();
            CommitFiles(historyItem);
        }

        void StageFiles()
        {
            context.Logger.LogInfo("Staging files for commit to Git..."); 
            gitClient.ExecuteCommand(AddCommand, new SwitchArgument(AllSwitchArgument));
        }

        void CommitFiles(VersionHistoryItem historyItem)
        {
            context.Logger.LogInfo("Committing files to Git...");

            string commitMessage = FormatCommitMessage(historyItem);
            string commitDate = FormatDateForCommit(historyItem.Date);
            string commitAuthor = FormatAuthorForCommit(historyItem.Author, context.Parameters.CommitAuthorEmailDomain);

            gitClient.ExecuteCommand(CommitCommand, commitMessage,
                new NamedArgument(MessageSourceFileArgument, UseStandardInputArgumentValue),
                new NamedArgument(AuthorArgument, commitAuthor), new NamedArgument(DateArgument, commitDate),
                new SwitchArgument(AllowEmptySwitchArgument));
        }

        string FormatCommitMessage(VersionHistoryItem historyItem)
        {
            var standardInputBuilder = new StringBuilder();
            standardInputBuilder.AppendLine(historyItem.Comment);
            standardInputBuilder.AppendLine();
            standardInputBuilder.AppendLine(FormatExportSourceNote(historyItem));

            return standardInputBuilder.ToString();
        }

        string FormatExportSourceNote(VersionHistoryItem historyItem)
        {
            return string.Format(VaultExportSourceNoteFormat, context.Parameters.VaultRepositoryName, context.Parameters.VaultRepositoryPath,
                historyItem.Version);
        }

        static string FormatDateForCommit(DateTime dateTime)
        {
            return dateTime.ToString(Iso8601DateTimeFormat, CultureInfo.InvariantCulture);
        }

        static string FormatAuthorForCommit(string author, string authorEmailDomain)
        {
            return string.Format(CommitAuthorFormat, author, authorEmailDomain);
        }
    }
}
