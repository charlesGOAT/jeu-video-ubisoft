using NUnit.Framework;
using System.IO;
using UnityEngine;

public class RuntimeConfigLoaderTests
{
    [Test]
    public void RuntimeConfigData_HasDefaultValues()
    {
        RuntimeConfigData config = new RuntimeConfigData();
        
        Assert.IsTrue(config.IsSpreadingMode);
        Assert.IsFalse(config.IsBonusSpeed);
        Assert.AreEqual(120f, config.GameDuration);
        Assert.AreEqual(30, config.FrozenTileDuration);
        Assert.AreEqual(1.5f, config.ColorBoost);
        Assert.AreEqual(0.85f, config.ColorDebuff);
    }

    [Test]
    public void RuntimeConfigData_ItemSpawnerData_HasDefaults()
    {
        RuntimeConfigData config = new RuntimeConfigData();
        
        Assert.IsNotNull(config.PaintBrushItemSpawnerData);
        Assert.IsNotNull(config.ChainedBombItemSpawnerData);
        Assert.IsNotNull(config.TargetBombItemSpawnerData);
        Assert.IsNotNull(config.FreezeBombItemSpawnerData);
    }

    [Test]
    public void RuntimeConfigData_DefaultSpawnMode_IsFixed()
    {
        RuntimeConfigData config = new RuntimeConfigData();
        
        Assert.AreEqual(SpawnMode.Fixed, config.SpawnMode);
    }

    [Test]
    public void RuntimeConfigData_DefaultIsDropFromSky_IsFalse()
    {
        RuntimeConfigData config = new RuntimeConfigData();
        
        Assert.IsFalse(config.IsDropFromSky);
    }

    [Test]
    public void ItemSpawnerData_CanSetMaxItems()
    {
        ItemSpawnerData spawnerData = new ItemSpawnerData
        {
            MaxItems = 5,
            TimeBetweenSpawns = 10f
        };
        
        Assert.AreEqual(5, spawnerData.MaxItems);
        Assert.AreEqual(10f, spawnerData.TimeBetweenSpawns);
    }
}