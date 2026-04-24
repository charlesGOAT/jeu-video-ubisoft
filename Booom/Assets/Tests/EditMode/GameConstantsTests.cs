using NUnit.Framework;

public class GameConstantsTests
{
    [Test]
    public void NBPlayers_IsFour()
    {
        Assert.AreEqual(4, GameConstants.NB_PLAYERS);
    }

    [Test]
    public void ColorBoost_IsPositive()
    {
        Assert.Greater(GameConstants.COLOR_BOOST, 1f);
    }

    [Test]
    public void ColorDebuff_IsLessThanOne()
    {
        Assert.Less(GameConstants.COLOR_DEBUFF, 1f);
    }

    [Test]
    public void UnityGridSize_IsTwo()
    {
        Assert.AreEqual(2, GameConstants.UNITY_GRID_SIZE);
    }

    [Test]
    public void HitStateDuration_IsPositive()
    {
        Assert.Greater(GameConstants.HIT_STATE_DURATION, 0f);
    }

    [Test]
    public void GameDuration_IsPositive()
    {
        Assert.Greater(GameConstants.GAME_DURATION, 0f);
    }

    [Test]
    public void SpeedBoostPerKill_HasCorrectNumberOfEntries()
    {
        Assert.AreEqual(6, GameConstants.SpeedBoostPerKill.Count);
    }

    [Test]
    public void SpeedBoostPerKill_Kill0_HasNoBoost()
    {
        Assert.AreEqual(1f, GameConstants.SpeedBoostPerKill[0]);
    }

    [Test]
    public void SpeedBoostPerKill_Kill5_HasMaxBoost()
    {
        Assert.AreEqual(2.25f, GameConstants.SpeedBoostPerKill[5]);
    }

    [Test]
    public void RangeBoostPerKill_HasCorrectNumberOfEntries()
    {
        Assert.AreEqual(5, GameConstants.RangeBoostPerKill.Count);
    }

    [Test]
    public void RangeBoostPerKill_Kill0_HasNoRangeBoost()
    {
        Assert.AreEqual(0, GameConstants.RangeBoostPerKill[0]);
    }

    [Test]
    public void RangeBoostPerKill_Kill4_HasMaxRangeBoost()
    {
        Assert.AreEqual(4, GameConstants.RangeBoostPerKill[4]);
    }

    [Test]
    public void SpeedBoostPerKill_ValuesIncreaseWithKills()
    {
        float previous = 0f;
        foreach (var kvp in GameConstants.SpeedBoostPerKill)
        {
            Assert.Greater(kvp.Value, previous);
            previous = kvp.Value;
        }
    }

    [Test]
    public void RangeBoostPerKill_ValuesIncreaseWithKills()
    {
        int previous = -1;
        foreach (var kvp in GameConstants.RangeBoostPerKill)
        {
            Assert.GreaterOrEqual(kvp.Value, previous);
            previous = kvp.Value;
        }
    }
}