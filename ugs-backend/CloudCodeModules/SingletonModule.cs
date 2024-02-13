using Openfort.SDK;
using Openfort.SDK.Model;

namespace CloudCodeModules;

public class SingletonModule
{
    private static SingletonModule instance;
    private static readonly object Padlock = new object();

    private const string OfApiKey = "";
    public const string OfNftContract = "";
    public const string OfGoldContract = "";
    public const string OfSponsorPolicy = "";
    public const string OfDevTreasuryAccount = "";
    public const string OfDevMintingAccount = "";
    
    public OpenfortClient OfClient { get; private set; }
    public int ChainId { get; } = 80001;
    public PlayerResponse? CurrentOfPlayer { get; set; }
    public AccountResponse? CurrentOfAccount { get; set; }

    SingletonModule()
    {
        OfClient = new OpenfortClient(OfApiKey);
    }

    public static SingletonModule Instance()
    {
        lock (Padlock)
        {
            if (instance == null)
            {
                instance = new SingletonModule();
            }
            return instance;
        }
    }
}
