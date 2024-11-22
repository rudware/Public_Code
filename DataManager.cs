using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoSingletone<DataManager>
{
    public float Score { get; set; }

    [Header("Player")]
    public float PlayerFirstNeedExp;
    public float PlayerMoveSpeed;
    public float PlayerBaseDamage;
    public float PlayerProjectileSpeed;
    public float PlayerProjectileCooldown;
    public float PlayerBaseHP;
    public float PlayerBaseShield;
    public float PlayerBaseMagnetSize;
    public float PlayerBaseMagnetForce;
    public int PlayerBaseSpecialCount;
    public int PlayerBaseSpecialMaxCount;

    public PlayerData CurrentPlayer;
    public void PlayerDataInit()
    {
        CurrentPlayer = new PlayerData();
    }

    public List<Equip_Type> RemainEquipTypeList
    {
        get
        {
            List<Equip_Type> tmpList = new List<Equip_Type>();
            for(int i=0; i<(int)Equip_Type.Max; i++)
            {
                if (!CurrentPlayer.EquipList.Contains((Equip_Type)i))
                {
                    tmpList.Add((Equip_Type)i);
                }
            }

            return tmpList;
        }
    }


    public float PlayerShieldChargeTick;


    public enum Weapon_Type
    {
        Basic,
        Triple_Gun,
        Rifle,
        ShotGun,

        MachineGun,
        RocketLauncher,

        Max

    }

    public enum Equip_Type 
    { 
        Magnet,
        Force_Field,
        Max

    }

    public enum Item_Type
    {
        None,
        ExpGem,
        Food,
    }


    [Header("Enemy")]
    public float EnemyMaxSize;


    public enum Enemy_Type
    {
        Basic,
        Boomer,
        NormalBoss,
        Poison,
        Split,
    }

    [Header("Common Transform")]
    [SerializeField]
    public Transform GoProjectileParent;
    [SerializeField]
    public Transform GoParticleParent;

    [Header("Prefabs")]
    [SerializeField]
    public Transform GoBoomPrefab;
    [SerializeField]
    public Transform GoSmokePrefab;
    [SerializeField]
    public Transform GoHitPrefab;

    float _startTime;
    public float PlayTime { get { return Time.realtimeSinceStartup - _startTime; } }
    public void NewGameStart()
    {
        _startTime = Time.realtimeSinceStartup;
        PlayerDataInit();
    }

    public void OnCreateSmoke(Vector2 aPos, float aSize)
    {
        Transform obj = Instantiate(GoSmokePrefab, aPos, Quaternion.identity, GoParticleParent);
        obj.rotation = Quaternion.identity;
        float ori = obj.localScale.x;
        Vector3 scale = obj.localScale * aSize;
        if(scale.x < ori)
        {
            scale.x = ori;
            scale.y = ori;
            scale.z = ori;
        }
        obj.localScale = scale;

    }

    public void OnHitEffect(Vector2 aPos)
    {
        Instantiate(GoHitPrefab, aPos, Quaternion.identity, GoParticleParent);
    }
}

public class PlayerData
{
    public int Level { get; set; }
    public float CurrentExp { get; set; }
    public float NeedExp { get; set; }
    public float ExpPercent { get { return CurrentExp / NeedExp; } }

    public float CurrentHP { get; set; }
    public float MaxHP { get; set; }
    public float HPPercent { get { return CurrentHP / MaxHP; } }

    public float CurrentShield { get; set; }
    public float MaxShield { get; set; }
    public float ShieldPercent { get { return CurrentShield / MaxShield; } }

    public float MoveSpeed { get; set; }
    public float Damage { get; set; }

    public float ProjectileSpeed { get; set; }
    public float ProjectileCooldown { get; set; }

    public int SpecialCount { get; set; }
    public int SpecialMaxCount { get; set; }

    public float MagnetSize { get; set; }
    public float MagnetForce { get; set; }

    public List<DataManager.Equip_Type> EquipList = new List<DataManager.Equip_Type>();

    public DataManager.Weapon_Type CurrentWeapon = DataManager.Weapon_Type.Basic;

    public int ProjectileCount { get; set; }
    
    public PlayerData()
    {
        Level = 1;
        CurrentExp = 0;
        NeedExp = DataManager.Instance.PlayerFirstNeedExp;

        MoveSpeed = DataManager.Instance.PlayerMoveSpeed;
        Damage = DataManager.Instance.PlayerBaseDamage;
        ProjectileSpeed = DataManager.Instance.PlayerProjectileSpeed;
        ProjectileCooldown = DataManager.Instance.PlayerProjectileCooldown;

        MaxHP = DataManager.Instance.PlayerBaseHP;
        CurrentHP = MaxHP;
        MaxShield = DataManager.Instance.PlayerBaseShield;
        CurrentShield = MaxShield;

        MagnetSize = DataManager.Instance.PlayerBaseMagnetSize;
        MagnetForce = DataManager.Instance.PlayerBaseMagnetForce;

        SpecialCount = DataManager.Instance.PlayerBaseSpecialCount;
        SpecialMaxCount = DataManager.Instance.PlayerBaseSpecialMaxCount;

        CurrentWeapon = DataManager.Weapon_Type.Basic;
        EquipList.Clear();

        ProjectileCount = -1;

    }

    public void SetEquipItem(DataManager.Equip_Type aType)
    {
        if (!EquipList.Contains(aType))
        {
            EquipList.Add(aType);
        }
    }

    public bool IsSetAllEquip
    {
        get
        {
            for(int i=0; i<(int)DataManager.Equip_Type.Max; i++)
            {
                if (!EquipList.Contains((DataManager.Equip_Type)i))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public void GainFood(float aValue)
    {
        CurrentHP += aValue;
        if(CurrentHP > MaxHP)
        {
            CurrentHP = MaxHP;
        }
    }

    public void ChargeShield(float aValue)
    {
        CurrentShield += aValue;
        if(CurrentShield > MaxShield)
        {
            CurrentShield = MaxShield;
        }
    }
    public bool GainExp(float aExp)
    {
        CurrentExp += aExp;
        if (CurrentExp >= NeedExp)
        {
            Level++;
            CurrentExp = 0;
            NeedExp = NeedExp * 1.2f;
            return true;
        }
        return false;
    }
}

public class WeaponData 
{
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    public float Damage { get; set; }
    public float ProjectileSpeed { get; set; }
    public float ProjectileCooldown { get; set; }


    public WeaponData()
    {
        Level = 1;
        MaxLevel = 10;
    }

    public void DataInit(float aDmg, float aSpeed, float aCool)
    {
        Damage = aDmg;
        ProjectileSpeed = aSpeed;
        ProjectileCooldown = aCool;
    }
}
