using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Custom.Network;


public class PurchaseManager : MonoSingletone<PurchaseManager>, IStoreListener
{
    [Header("Product ID")]
    public readonly string productId_ticket1 = "com.CompanyName.pruductName.ticket1";  //Google Store, APPStore 판매 상품 코드
    public readonly string productId_ticket2 = "com.CompanyName.pruductName.ticket2";


    [Header("Cache")]
    IStoreController _storeController;
    IExtensionProvider _storeExtensionProvider;

    public string environment = "production";

    public ProductCollection ProductCollection
    {
        get
        {
            if(_storeController != null)
            {
                return _storeController.products;
            }
            return null;
        }
    }

    private void Awake()
    {
        InitUnityServices();
        

    }

    // Start is called before the first frame update
    void Start()
    {
        InitUnityIAP();
    }

    async void InitUnityServices()
    {
        Debug.Log("InitUnityServices Start");
        try
        {
            var options = new InitializationOptions().SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
        }
        catch(Exception e)
        {

        }
    }

    void InitUnityIAP()
    {
        Debug.Log("InitUnityIAP Start");


        ConfigurationBuilder mBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); 
        mBuilder.AddProduct(productId_ticket1, ProductType.Consumable, new IDs() { { productId_ticket1, GooglePlay.Name } });  //상품 등록
        mBuilder.AddProduct(productId_ticket2, ProductType.Consumable, new IDs() { { productId_ticket2, GooglePlay.Name } });
        if (Application.platform == RuntimePlatform.Android)
        {
        


           
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {

        }
        else
        {
        }

        UnityPurchasing.Initialize(this, mBuilder);

    }

    public ProductCollection GetProductCollection()
    {
        if(_storeController != null)
        {
            return _storeController.products;
        }
        return null;
    }

    public void Purchase(string aProductId)
    {
        NetworkManager.Instance.SetActiveProgress(true);
        Debug.Log("Purchase : " + aProductId);

        Product mProduct = _storeController.products.WithID(aProductId);

        if (mProduct != null && mProduct.availableToPurchase)
        {
            Debug.Log("Purchase Start : " + aProductId);
            if (Application.platform == RuntimePlatform.Android)
            {
                _storeController.InitiatePurchase(mProduct);
            }

        }
        else
        {
            NetworkManager.Instance.SetActiveProgress(false);
            Debug.Log("Not available Purchase");
        }
    }

    public void OnInitialized(IStoreController aController, IExtensionProvider aExtension)
    {
        Debug.Log("Initialization Complete");

        _storeController = aController;
        _storeExtensionProvider = aExtension;
    }

    public void OnInitializeFailed(InitializationFailureReason aError)
    {
        NetworkManager.Instance.SetActiveProgress(false);

        Debug.Log("Initioalization Failed");
    }

    public void OnPurchaseFailed(Product aProduct, PurchaseFailureReason aReason)
    {
        NetworkManager.Instance.SetActiveProgress(false);

        Debug.Log("Purchase Failed");
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("Purchase Success : " + productId_ticket1);

        if (args.purchasedProduct.definition.id == productId_ticket1)
        {
            NetworkManager.Instance.SetActiveProgress(false);

            NetworkManager.Instance.OnSendPurchaseTicket(productId_ticket1, OnResponseSendTicketData);

        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }

    void OnResponseSendTicketData(Network_Result_State aState)
    {
        NetworkManager.Instance.OnRequestUserData(OnResponseUserData);

    }

    void OnResponseUserData(Network_Result_State aState)
    {
        MainSceneManager.Instance.OnPurchaseComplete();
    }

}

public class PurchaseData 
{
    string identifier;
    string userId;

    int ticketcount;

    List<PurchasedLevelData> _purchasedLevelDataList = new List<PurchasedLevelData>();

    public int TicketCount { get { return ticketcount; }  set { ticketcount = value; } }

    public PurchaseData()
    {

    }

    public PurchaseData(string aUserId)
    {
        userId = aUserId;
        ticketcount = 0;
        _purchasedLevelDataList.Clear();
       
    }

    public void DataInit(string aUserId, string aJson = null)
    {
        Debug.Log("Purchased Level Data Init");
        userId = aUserId;
        _purchasedLevelDataList.Clear();
        if (!string.IsNullOrEmpty(aJson))
        {
            Debug.LogError(aJson);
            try
            {
                JObject mObject = JObject.Parse(aJson);
                

                foreach(var node in mObject.Values())
                {
                    PurchasedLevelData mData = JsonUtility.FromJson<PurchasedLevelData>(node.ToString());
                    Debug.Log(mData.LevelIndex);
                    _purchasedLevelDataList.Add(mData);
                }

                
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            
        }
      
    }

    public void DataInit(Net_PurchadedLevelData[] aData)
    {
        Debug.LogError("Purchased Level Data Init");
        _purchasedLevelDataList.Clear();

        if(aData != null)
        {
            foreach(var node in aData)
            {
                if(node != null)
                {

                    if (!node.IsExpire)
                    {
                        PurchasedLevelData mData = new PurchasedLevelData();
                        mData.DataInit(node);
                        _purchasedLevelDataList.Add(mData);
                    }
                    
                    
                }
                
            }
        }

    }



    public bool GetPurchasedLevel(string aIndex)
    {
        for(int i=0; i<_purchasedLevelDataList.Count; i++)
        {
            if(_purchasedLevelDataList[i].LevelIndex == aIndex)
            {
                return true;
            }
        }
        return false;
    }




    public void SetPurchaseData(string aPurchaseId)
    {

    }
}


public class PurchaseItemData
{
    string id;
    string name;
    string price;
    string description;
}

[Serializable]
public class PurchasedLevelData
{
    [SerializeField]
    string id;

    [SerializeField]
    string purchasedTimestamp;  //구매 일시 타임 스탬프
    [SerializeField]
    string expireTimestamp;  //기간 한정 상품의 경우 만료 일시 타임스탬프
    [SerializeField]
    string purchasedDateTime; //구매 일시 데이트타임 
    [SerializeField]
    string expireDateTime; //만료 일시 데이트타임

    public string PurchasedDateTime { get { return purchasedDateTime; } }

    public string PurchasedTimestamp { get { return purchasedTimestamp; } }
    public void SetDateTime()
    {
        long mNowTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();  //현재 시간을 타임스탬프로 설정
        purchasedTimestamp = mNowTimestamp.ToString(); //string 으로 저장
        purchasedDateTime = DateTimeOffset.FromUnixTimeMilliseconds(mNowTimestamp).ToLocalTime().ToString(); //타임 스탬프를 현지 시간으로 변환하여 DateTime으로 저장
        DateTimeOffset mExpireTime = DateTimeOffset.FromUnixTimeMilliseconds(mNowTimestamp).AddYears(1); //1년후 만료 되게끔 설정
        expireDateTime = mExpireTime.ToString();
        expireTimestamp = mExpireTime.ToUnixTimeMilliseconds().ToString();
    }

    public void SetData(string aId)
    {
        id = aId;
        SetDateTime();
    }

}


