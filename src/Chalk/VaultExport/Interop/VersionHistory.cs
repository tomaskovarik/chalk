using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chalk.Interop;
using YAXLib;

namespace Chalk.VaultExport.Interop
{
    public class VersionHistory
    {
        const string VersionHistoryCommand = "VERSIONHISTORY";

        readonly IVaultClient vaultClient;
        readonly string repositoryName;
        readonly string repositoryPath;

        public VersionHistory(IVaultClient vaultClient, string repositoryName, string repositoryPath)
        {
            this.vaultClient = vaultClient;
            this.repositoryName = repositoryName;
            this.repositoryPath = repositoryPath;
        }

        public IEnumerable<VersionHistoryItem> Fetch(int? beginAtVersion)
        {
            IArgument[] arguments = CreateArguments(repositoryName, beginAtVersion);
            var commandOutput = vaultClient.ExecuteCommand<VersionHistoryCommandOutput>(VersionHistoryCommand,
                new PositionalArgument(repositoryPath), arguments);

            var historyItems =
                commandOutput.HistoryItems.OrderBy(i => i.Version)
                    .Select(i => new VersionHistoryItem(i.Version, i.TransactionId, i.Date, i.Comment, i.User));

            return historyItems.ToArray();
        }

        static IArgument[] CreateArguments(string repositoryName, int? beginAtVersion)
        {
            var args = new List<IArgument>(3)
            {
                CommandLineClientArgument.RepositoryName(repositoryName),
            };

            if (beginAtVersion != null)
            {
                args.Add(BeginAtVersionArgument(beginAtVersion.Value));
                args.Add(UnlimitedRowsArgument());                
            }
            else
            {
                args.Add(BeginAtLatestVersionArgument());
            }

            return args.ToArray();
        }

        static NamedArgument UnlimitedRowsArgument()
        {
            return CommandLineClientArgument.RowLimit(100000);
        }

        static NamedArgument BeginAtLatestVersionArgument()
        {
            return CommandLineClientArgument.RowLimit(1);
        }

        static NamedArgument BeginAtVersionArgument(int beginAtVersion)
        {
            return CommandLineClientArgument.BeginVersion(beginAtVersion);
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        class VersionHistoryCommandOutput
        {
            [YAXSerializeAs("history")]
            [YAXCollection(YAXCollectionSerializationTypes.Recursive, EachElementName = "item")]
            [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
            public VersionHistoryItem[] HistoryItems { get; set; }

            public class VersionHistoryItem
            {
                [YAXSerializeAs("version")]
                [YAXAttributeForClass]
                public int Version { get; set; }

                [YAXSerializeAs("txid")]
                [YAXAttributeForClass]
                public int TransactionId { get; set; }

                [YAXSerializeAs("comment")]
                [YAXAttributeForClass]
                [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
                public string Comment { get; set; }

                [YAXSerializeAs("date")]
                [YAXAttributeForClass]
                public DateTime Date { get; set; }

                [YAXSerializeAs("user")]
                [YAXAttributeForClass]
                public string User { get; set; }
            }
        }
    }
}
