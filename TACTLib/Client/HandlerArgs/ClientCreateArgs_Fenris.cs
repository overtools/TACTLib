namespace TACTLib.Client.HandlerArgs;

public class ClientCreateArgs_Fenris : IHandlerArgs {
    /// <summary>
    /// Will not skip loading of encrypted SNOs if set to false
    /// </summary>
    public bool LoadEncryptedSnos { get; set; } = false;

    /// <summary>
    /// Language to use for base language overrides.
    /// </summary>
    public string? BaseLanguage { get; set; }
}
