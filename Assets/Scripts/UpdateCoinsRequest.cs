[System.Serializable]
public class UpdateCoinsRequest
{
    public string type = "updateCoins";
    public string playerId;
    public int coins;
}