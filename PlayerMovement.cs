using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    SpriteRenderer _mask;
    [SerializeField]
    Transform _goCenter;
    [SerializeField]
    Transform _goShooter;
    [SerializeField]
    Transform _goCrossHair;
    [SerializeField]
    SpriteRenderer _goShield;
    [SerializeField]
    Transform _goMagnet;

    [SerializeField]
    Transform _goBullet;


    [SerializeField]
    Image _goHPBar;

    [SerializeField]
    PlayerEquipSystem _equipSystem;
    [SerializeField]
    PlayerWeaponSystem _weaponSystem;
    [SerializeField]
    BaseWeapon _goHanGun;


    float vertical;
    float horizontal;

    public float moveSpeed;

    Camera mainCam;
    Vector3 mousePos;

    Vector3 shootPos;
    bool isFire;

    float currentTime = 0f;
    float bulletTickTime = 0.125f;

    float _currentHP = 100f;
    float _maxHP = 100f;

    public float damageDelayTick;
    float _damageDelayTime;

    float currentShieldChargeTime = 0f;


    float _currentFeverTime = 0;

    bool _isMagnetFever = false;

    private void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);


        
    }

    private void FixedUpdate()
    {
        if (MainManager.Instance.CurrentState == MainManager.Game_State.Play)
        {
            if(horizontal > 0)
            {
                _mask.flipX = false;
            }
            else if(horizontal < 0)
            {
                _mask.flipX = true;
            }
            rb.velocity = new Vector2(horizontal * DataManager.Instance.CurrentPlayer.MoveSpeed, vertical * DataManager.Instance.CurrentPlayer.MoveSpeed);

            Vector3 rotation = mousePos - transform.position;

            float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

            _goCenter.rotation = Quaternion.Euler(0, 0, rotZ);
            //_goWeapons.rotation = Quaternion.Euler(0, 0, rotZ);

            if (Input.GetMouseButton(0))
            {
                //Vector3 direction = mousePos - transform.position;
                //rb.velocity = new Vector2(direction.x, direction.y).normalized * DataManager.Instance.CurrentPlayer.MoveSpeed * 0.5f;
                //rb.velocity = new Vector2(horizontal * DataManager.Instance.CurrentPlayer.MoveSpeed, vertical * DataManager.Instance.CurrentPlayer.MoveSpeed);

                if (!isFire)
                {
                    isFire = true;
                    currentTime = 0;

                    shootPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                   
                    //Transform obj = Instantiate(_goBullet, _goShooter.position, Quaternion.identity, DataManager.Instance.GoProjectileParent);
                    //obj.GetComponent<Bullet>().OnShoot(_goCrossHair.position);
                    //Transform obj2 = Instantiate(_goBullet, _goShooter.position , Quaternion.identity, DataManager.Instance.GoProjectileParent);
                    //obj2.GetComponent<Bullet>().OnShoot(_goCrossHair.position + new Vector3(0, 1, 0));
                    //Transform obj3 = Instantiate(_goBullet, _goShooter.position , Quaternion.identity, DataManager.Instance.GoProjectileParent);
                    //obj3.GetComponent<Bullet>().OnShoot(_goCrossHair.position + new Vector3(0, -1, 0));
                }
            }

            if (currentTime < DataManager.Instance.CurrentPlayer.ProjectileCooldown)
            {
                currentTime += Time.fixedDeltaTime;
            }
            else
            {
                isFire = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (DataManager.Instance.CurrentPlayer.SpecialCount > 0)
                {
                    MainManager.Instance.AllEnemyDie();
                    DataManager.Instance.CurrentPlayer.SpecialCount--;
                    MainManager.Instance.OnSpecialCountUIUpdate();
                }
            }

            if(currentShieldChargeTime < DataManager.Instance.PlayerShieldChargeTick)
            {
                currentShieldChargeTime += Time.fixedDeltaTime;
            }
            else
            {
                DataManager.Instance.CurrentPlayer.ChargeShield(1f);
                OnShieldUpate();
                currentShieldChargeTime = 0;
            }

       

            if (Input.GetKeyDown(KeyCode.F))
            {
                MainManager.Instance.OnForceMagnet();
            }
        }
    }

    public void DataInit()
    {
        _equipSystem.DataInit();
        _weaponSystem.DataInit();
        OnHPUpdate();
        transform.parent.localPosition = Vector2.zero;

    }
    public void OnDamaged(float aValue)
    {
        if (DataManager.Instance.CurrentPlayer.CurrentShield > 0f)
        {
            DataManager.Instance.CurrentPlayer.CurrentShield += aValue;
            if(DataManager.Instance.CurrentPlayer.CurrentShield < 0)
            {
                DataManager.Instance.CurrentPlayer.CurrentShield = 0;
                //DataManager.Instance.CurrentPlayer.CurrentHP += DataManager.Instance.CurrentPlayer.CurrentShield;
            }
        }
        else
        {
            DataManager.Instance.CurrentPlayer.CurrentHP += aValue;
        }
        OnHPUpdate();

        if (DataManager.Instance.CurrentPlayer.CurrentHP <= 0f)
        {
            MainManager.Instance.OnStateChange(MainManager.Game_State.Result);
            PauseTransform();
        }
    }
    
    void OnMagnetFeverStart()
    {
        _goMagnet.localScale = new Vector3(150, 150, 1);
        DataManager.Instance.CurrentPlayer.MagnetForce = 20f;
        _currentFeverTime = 0f;
        _isMagnetFever = true;
    }

    void OnMagnetFeverFinish()
    {
        _isMagnetFever = false;
        DataManager.Instance.CurrentPlayer.MagnetForce = 1f;
        _goMagnet.localScale = new Vector3(10, 10, 1);
    }

    void OnShieldUpate()
    {
        Color mColor = _goShield.color;
        mColor.a = 0.5f * DataManager.Instance.CurrentPlayer.ShieldPercent;
        _goShield.color = mColor;
    }
    void OnHPUpdate()
    {
        OnShieldUpate();
        _goHPBar.fillAmount = DataManager.Instance.CurrentPlayer.HPPercent;
    }

    void OnMagentUpdate()
    {
        _goMagnet.localScale = new Vector3(DataManager.Instance.CurrentPlayer.MagnetSize, DataManager.Instance.CurrentPlayer.MagnetSize, 1);
    }

    public void PauseTransform()
    {
        rb.velocity = Vector2.zero;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            float aTime = Time.realtimeSinceStartup;
            if (aTime - _damageDelayTime > damageDelayTick)
            {
                _damageDelayTime = aTime;
                BaseEnemy tmpScript = other.gameObject.GetComponent<BaseEnemy>();
                if(tmpScript != null)
                {
                    if(tmpScript.Type != DataManager.Enemy_Type.Boomer)
                    {
                        OnDamaged(-(5f + tmpScript.MaxHP));
                    }
                    else
                    {
                        OnDamaged(-2f);
                        tmpScript.OnObjectDestroy();
                    }
                    
                }
                else
                {
                    OnDamaged(-5f);
                }
                
              
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ExpGem"))
        {
            ExpGemItem tmpScript = other.GetComponent<ExpGemItem>();
            if(tmpScript != null)
            {
                tmpScript.GainItem();
            }
        }
        else if (other.gameObject.CompareTag("Food"))
        {
            DataManager.Instance.CurrentPlayer.GainFood(30f);
            OnHPUpdate();
            Destroy(other.gameObject);


        }
        else if (other.gameObject.CompareTag("Magnet_Size"))
        {
            DataManager.Instance.CurrentPlayer.MagnetSize += 1f;
            OnMagentUpdate();
            Destroy(other.gameObject);

        }
        else if (other.gameObject.CompareTag("Magnet_Force"))
        {
            DataManager.Instance.CurrentPlayer.MagnetForce += 0.1f;
            OnMagentUpdate();
            Destroy(other.gameObject);


        }
        else if (other.gameObject.CompareTag("Weapon"))
        {
            WeaponItem tmpScript = other.gameObject.GetComponent<WeaponItem>();
            if(tmpScript != null)
            {
                _weaponSystem.SetActiveWeapon((int)tmpScript.Type);
                if(tmpScript.Type == DataManager.Weapon_Type.Triple_Gun)
                {
                    DataManager.Instance.CurrentPlayer.CurrentWeapon = tmpScript.Type;
                }
                
            }

            
            
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Magnet_Fever"))
        {
            MainManager.Instance.OnForceMagnet();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Equip"))
        {
            EquipItem tmpScript = other.GetComponent<EquipItem>();
            if(tmpScript != null)
            {
                Debug.LogError(tmpScript.Type);
                _equipSystem.SetActiveEquip(tmpScript.Type);
            }
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            float aTime = Time.realtimeSinceStartup;
            if (aTime - _damageDelayTime > damageDelayTick)
            {
                _damageDelayTime = aTime;
                BaseEnemy tmpScript = other.gameObject.GetComponent<BaseEnemy>();
                if (tmpScript != null)
                {
                    OnDamaged(-(5f + tmpScript.MaxHP));
                }
                else
                {
                    OnDamaged(-5f);
                }


            }
        }
    }


}
