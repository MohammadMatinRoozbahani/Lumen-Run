using System;

public static class RunSessionChecks
{
    static int checks;

    static void Require(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("RunSession check failed: " + name);
        checks++;
    }

    public static string Run()
    {
        checks = 0;
        var session = new RunSession();
        session.Begin(200);
        Require(session.Lives == 3 && session.Coins == 0 && session.IntegerScore == 0, "new run");
        Require(session.Record == 200 && session.PreviousRecord == 200, "existing record");
        session.Step(1, 8.5f);
        Require(Math.Abs(session.Score - 12) < .001f && Math.Abs(session.Distance - 8.5f) < .001f, "time score and distance");
        session.CollectCoin();
        Require(session.Coins == 1 && session.IntegerScore == 112, "coin reward");
        Require(session.Record == 200, "record never decreases");
        float frozen = session.Score;
        session.Step(0, 10);
        session.Step(-1, 10);
        Require(session.Score == frozen, "zero and negative time");
        Require(session.Damage() && session.Lives == 2, "first collision");
        Require(session.Damage() && session.Lives == 1, "second collision");
        Require(session.Damage() && session.Lives == 0, "third collision");
        Require(!session.Damage() && session.Lives == 0, "lives cannot underflow");
        frozen = session.Score;
        session.CollectCoin();
        session.Step(10, 10);
        Require(session.Score == frozen && session.Coins == 1, "no reward after death");
        session.Begin(200);
        Require(session.Lives == 3 && session.Score == 0 && session.Distance == 0 && session.Coins == 0, "restart resets run");
        session.Step(20, 8.5f);
        Require(session.Record == 240 && session.NewRecord, "new high score");
        session.Stop();
        frozen = session.Score;
        session.Step(5, 8.5f);
        session.CollectCoin();
        Require(session.Score == frozen && !session.Active, "stopped run");
        session.Begin(240);
        Require(!session.NewRecord && session.Record == 240, "restart record baseline");
        session.Step(100000, 14.5f);
        Require(session.Level == 7 && Math.Abs(session.BaseSpeed - 14.5f) < .001f, "difficulty cap");
        session.Begin(-50);
        Require(session.Record == 0, "invalid record sanitization");
        return checks + " RunSession checks passed.";
    }
}
