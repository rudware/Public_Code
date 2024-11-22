using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoSingletone<MainManager>
{
    [SerializeField]
    PlayerMovement _goPlayer;

    [SerializeField]
    GameObject _goStartPage;

    [Header("Play")]
    [SerializeField]
    GameObject _goPlayPage;
    [SerializeField]
    EnemySpawn _enemySpawn;
    [SerializeField]
    Text _scoreBoard;
    [SerializeField]
    Text tx_playTime;
    [SerializeField]
    Image img_exp;


    [SerializeField]
    Transform _itemParent;
    [SerializeField]
    Transform _goExpGemPrefab;
    [SerializeField]
    Transform _goFoodPrefab;
    [SerializeField]
    Transform _goMagnetForce;
    [SerializeField]
    Transform _goMagnetSize;



    [Header("Result")]
    [SerializeField]
    GameObject _goResultPage;
    [SerializeField]
    Text _resultScore;


    float _score = 0;
    
    

    public enum Game_State
    {
        None = 0,
        Start,
        Play,
        Result,
    }

    public Game_State CurrentState = Game_State.None;

    // Start is called before the first frame update
    void Start()
    {
        OnStateChange(Game_State.Start);
    }

    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        if(CurrentState == Game_State.Play)
        {
            tx_playTime.text = (TimeSpan.FromSeconds(DataManager.Instance.PlayTime)).ToString(@"mm\:ss");

            if(DataManager.Instance.PlayTime >= 600)
            {
                OnStateChange(Game_State.Result);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (_statusBoard.gameObject.activeInHierarchy)
                {
                    _statusBoard.Close();
                }
                else
                {
                    _statusBoard.Open();
                }
            }

            
        }
    }

    public void OnStateChange(Game_State aState)
    {
        _statusBoard.Close();
        OnPopupDataInit();
        _goStartPage.SetActive(false);
        _goPlayPage.SetActive(false);
        _goResultPage.SetActive(false);

        switch (aState) 
        {
            case Game_State.None:

                break;
            case Game_State.Start:
                OnStartStateInit();
                break;

            case Game_State.Play:
                OnPlayStateInit();
                break;
            case Game_State.Result:
                OnResultStateInit();
                break;
        
        }
        CurrentState = aState;
    }

    void OnStartStateInit()
    {
        _goStartPage.SetActive(true);
    }
    

    void OnPlayStateInit()
    {
        DataManager.Instance.NewGameStart();
        _goPlayPage.SetActive(true);
        _enemySpawn.DataInit();
        _goPlayer.DataInit();
        OnScoreUIUpdate();
        OnSpecialCountUIUpdate();

        for (int i=0; i<_itemParent.childCount; i++)
        {
            Destroy(_itemParent.GetChild(i).gameObject);
        }
    }


    public void OnCreateItem(float aValue, Vector2 aPos, bool aIsBoss = false)
    {
        int ran = UnityEngine.Random.Range(0, 100);
        if (!aIsBoss)
        {
            if (ran < 60)
            {
                Transform obj = Instantiate(_goExpGemPrefab, aPos, Quaternion.identity, _itemParent);
                ExpGemItem tmpScript = obj.GetComponent<ExpGemItem>();
                if (tmpScript != null)
                {
                    tmpScript.DataInit(aValue);
                }
            }
            else if (ran < 70)
            {
                Instantiate(_goFoodPrefab, aPos, Quaternion.identity, _itemParent);
            }
            else if (ran < 72)
            {
                Instantiate(_goMagnetForce, aPos, Quaternion.identity, _itemParent);
            }
            else if (ran < 74)
            {
                Instantiate(_goMagnetSize, aPos, Quaternion.identity, _itemParent);
            }
            else if (ran < 80)
            {
                
                Transform obj = Instantiate(DataManager.Instance.GoBoomPrefab, aPos, Quaternion.identity);
                obj.GetComponent<BoomItem>().TimerStart(0.25f);
                
            }
        }
        else
        {
            if(ran < 80)
            {
                Transform obj = Instantiate(_goExpGemPrefab, aPos, Quaternion.identity, _itemParent);
                ExpGemItem tmpScript = obj.GetComponent<ExpGemItem>();
                if (tmpScript != null)
                {
                    tmpScript.DataInit(aValue);
                }
            }

            {
                Transform obj = Instantiate(DataManager.Instance.GoBoomPrefab, aPos, Quaternion.identity);
                obj.GetComponent<BoomItem>().TimerStart(0.25f, 10f);
            }
        }

     
    }

    public void OnForceMagnet()
    {
        for(int i=0; i<_itemParent.childCount; i++)
        {
            ExpGemItem tmpScript = _itemParent.GetChild(i).GetComponent<ExpGemItem>();
            if(tmpScript != null)
            {
                tmpScript.OnForceMagnet(_goPlayer.transform, 20f);
            }
        }
    }

    public void OnScoreUpdate(float aValue)
    {
        if (DataManager.Instance.CurrentPlayer.GainExp(aValue))
        {
            OnOpenPopupLevelUp();
        }

        OnScoreUIUpdate();
    }

    void OnScoreUIUpdate()
    {
        tx_Lv.text = string.Format("LV{0}", DataManager.Instance.CurrentPlayer.Level);
        _scoreBoard.text = string.Format("SCORE : {0}", DataManager.Instance.Score);
        OnExpUIUpdate();
    }

    void OnExpUIUpdate()
    {
        img_exp.fillAmount = DataManager.Instance.CurrentPlayer.ExpPercent;
    }


    void OnResultStateInit()
    {
        _goResultPage.SetActive(true);
        _resultScore.text = (TimeSpan.FromSeconds(DataManager.Instance.PlayTime)).ToString(@"mm\:ss");
    }


    public void OnClickStart()
    {
        OnStateChange(Game_State.Play);
    }

    public void AllEnemyDie()
    {
        _enemySpawn.AllEnemyDie();
    }



    #region UI
    [Header("UI")]
    [SerializeField]
    Text tx_Lv;
    [SerializeField]
    Text tx_SpecialCount;
    [SerializeField]
    Text tx_ProjectileCount;
    public void OnSpecialCountUIUpdate()
    {
        tx_SpecialCount.text = string.Format("{0}", DataManager.Instance.CurrentPlayer.SpecialCount);
    }
    [SerializeField]
    public void OnProjectileCountUIUpdate(int aCount)
    {
        if (aCount >= 0)
        {
            tx_ProjectileCount.text = string.Format("{0}", aCount);
        }
        else
        {
            tx_ProjectileCount.text = string.Format("<size=50>¡Ä</size>");
        }
    }



    [Space]
    [SerializeField]
    GameObject _goBasePopup;
    [SerializeField]
    LevelUpPopupScript _goLevelUpPopup;

    void OnPopupDataInit()
    {
        _goBasePopup.SetActive(false);
        _goLevelUpPopup.Close();
    }

    public void OnOpenPopupLevelUp()
    {
        _goBasePopup.SetActive(true);
        _goLevelUpPopup.Open();
    }


    [Space]
    [SerializeField]
    StatusBoard _statusBoard;

    #endregion

    private void OnApplicationFocus(bool focus)
    {
        if (!_goLevelUpPopup.gameObject.activeInHierarchy)
        {
            Time.timeScale = focus ? 1 : 0;
        }
        
    }
}
