public static class GameConstants
{
    public const string CurrentCloudModule = "CloudCodeModules";

    public const string MintNftCloudFunctionName = "MintNFT";
    public const string SellNftCloudFunctionName = "SellNFT";
    public const string BuyCryptoCloudFunctionName = "TokensToPlayer";
    public const string SpendCryptoCloudFunctionName = "TokensToDevAccount";
    public const string ReceiveCryptoCloudFunctionName = "TokensToPlayerAfterNftSale";

    public const string UgsCurrencyId = "GOLD";
    public const int DollarToCurrencyRate = 10; 
    public const int CurrencyToCryptoSwapRate = 10;
    public const float CryptoPriceReductionRate = 0.7f;
    
    // Cloud Save Keys
    public const string ReceiptTransactionIdKey = "TransactionID";
    public const string OpenfortPlayerIdKey = "OpenfortPlayerId";
}
