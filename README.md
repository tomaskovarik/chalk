# chalk

Chalk is a command-line utility for export of commits from SourceGear Vault repository to Git. In comparison to https://github.com/AndreyNikiforov/vault2git Chalk uses Vault command line client (instead of Vault API) and also supports export from a specific version (instead of beginning on the first repository revision).

Chalk is not optimized for speed (yet) as its primary use case is to export commits nightly to a Phabricator-managed Git repository. This way it is possible to perform code reviews even when there is no code review tool supporting aging SourceGear Vault.

## Licence
Apache 2, see LICENCE.txt.
