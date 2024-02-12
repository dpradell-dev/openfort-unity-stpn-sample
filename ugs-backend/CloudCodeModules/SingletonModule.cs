using Openfort.SDK;
using Openfort.SDK.Model;

namespace CloudCodeModules;

public class SingletonModule
{
    private static SingletonModule instance;
    private static readonly object Padlock = new object();

    private const string OfApiKey = "sk_test_d79a8f16-c56d-528b-8b69-40d3561a1736";
    public const string OfNftContract = "con_dd370178-0cca-4888-a2a0-ea194a7415c7";
    public const string OfGoldContract = "con_289384bf-ff3d-4aed-b5b3-ada9cbd4650b";
    public const string OfSponsorPolicy = "pol_a43e5145-6db9-4351-a800-b5c473d5dc92";
    public const string OfDevTreasuryAccount = "dac_05a72f40-b2d6-4d91-9e40-880508527e03";
    public const string OfDevMintingAccount = "dac_fcb38079-a918-406a-a968-672688c5fed1";
    
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
