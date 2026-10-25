using System;

public sealed class RunSession
{
    public const int MaximumLives = 3;
    public const int CoinScore = 100;
    public int Lives { get; private set; }
    public int Coins { get; private set; }
    public int Record { get; private set; }
    public int PreviousRecord { get; private set; }
    public float Score { get; private set; }
    public float Distance { get; private set; }
    public bool Active { get; private set; }
    public int IntegerScore { get { return (int)Score; } }
    public int Level { get { return Math.Min(7, 1 + (int)(Distance / 180f)); } }
    public float BaseSpeed { get { return 8.5f + Level - 1; } }
    public bool NewRecord { get { return IntegerScore > PreviousRecord; } }

    public void Begin(int record)
    {
        Lives = MaximumLives;
        Coins = 0;
        PreviousRecord = Record = Math.Max(0, record);
        Score = Distance = 0;
        Active = true;
    }

    public void Step(float deltaTime, float worldSpeed)
    {
        if (!Active || Lives <= 0 || deltaTime <= 0) return;
        Score += deltaTime * 12f;
        Distance += Math.Max(0, worldSpeed) * deltaTime;
        Record = Math.Max(Record, IntegerScore);
    }

    public bool Damage()
    {
        if (!Active || Lives <= 0) return false;
        Lives--;
        return true;
    }

    public void CollectCoin()
    {
        if (!Active || Lives <= 0) return;
        Coins++;
        Score += CoinScore;
        Record = Math.Max(Record, IntegerScore);
    }

    public void Stop()
    {
        Active = false;
    }
}
