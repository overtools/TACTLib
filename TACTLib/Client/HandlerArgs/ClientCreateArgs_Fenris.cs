namespace TACTLib.Client.HandlerArgs;

public class ClientCreateArgs_Fenris : IHandlerArgs {
    /// <summary>
    /// Language to use for base language overrides.
    /// </summary>
    public string? BaseLanguage { get; set; }
}
