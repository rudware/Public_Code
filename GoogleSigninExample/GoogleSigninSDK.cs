using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Custom.Network;
using UnityEngine.Events;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Google;
using System.Threading.Tasks;
using Custom.Crypto;
using UnityEngine.SceneManagement;
using System.IO;
using Custom.Define;

public class GoogleSignInSDKTest : MonoSigletone<GoogleSignInSDKTest>
{
    public string webClientId = ""; //Google Signin을 위한 Auth ID

    string _serverPath = ""; //Web API 주소

    GoogleSignInConfiguration configuration; //SDK Configration 
    GoogleSignInUser _currentUser;   //현재 Signin한 유저 정보

    int _tryIndex = 0; //현재 네트워크 접속 시도 횟수
    readonly int _tryCount = 5; //최대 네트워크 접속 시도 횟수

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestIdToken = true };
        OnGoogleSignInInit();
    }

    void OnGoogleSignInInit()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestAuthCode = true;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        _currentUser = null;
    }

    public void OnSignInGoogle(UnityAction<Network_Result_State, string> aAction)
    {
        try
        {
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    using (IEnumerator<System.Exception> enumerator =
                            task.Exception.InnerExceptions.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            GoogleSignIn.SignInException error =
                                    (GoogleSignIn.SignInException)enumerator.Current;

                            OnLog("Got Error: " + error.Status + " " + error.Message);
                        }
                        else
                        {
                            OnLog("Got Unexpected Exception?!?" + task.Exception);
                        }
                    }

                }
                else if (task.IsCanceled)
                {
                    OnLog("Canceled");
                }
                else
                {

                    _currentUser = task.Result;
                    OnGoogleSignInCallback(task.Result, aAction);



                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch
        {
            PopupManager.Instance.Open(11158); //로그인 실패 팝업
        }
    }

    void OnGoogleSignInCallback(GoogleSignInUser aUser, UnityAction<Network_Result_State, string> aAction)
    {
        _tryIndex = 0;

        Dictionary<string, string> mDic = new Dictionary<string, string>();
        mDic.Add("id_token", aUser.IdToken);  
        string mJson = JsonConvert.SerializeObject(mDic); //로그인 토큰 정보를 json화
        object[] parms = new object[3] { mJson, aAction, aUser.Email };
        StartCoroutine(RequestSignInGoogleUser(parms));
    }

    IEnumerator RequestSignInGoogleUser(object[] parms)
    {
        //구글 로그인한 정보를 WebAPI와 연동하는 부분
         string mJson = string.Empty;
        string mId = string.Empty;
        UnityAction<Network_Result_State, string> aAction = null;
        if (parms.Length > 2)
        {
            mJson = (string)parms[0];
            aAction = (UnityAction<Network_Result_State, string>)parms[1];
            mId = (string)parms[2];
        }


        string mPath = _serverPath + "auth-app";

        using (UnityWebRequest www = UnityWebRequest.Post(mPath, mJson))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(mJson);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");


            www.timeout = DataManager.Instance.TimeOut;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //상황에 맞는 오류 코드 출력
                string mText = www.downloadHandler.text;
                if (mText.Contains("Verify Error"))
                {
                    //인증 오류 코드
                }
                else if (mText.Contains("Inactive User"))
                {
                    //비활성화 유저(탈퇴 대기 중) 체크
                }
                else if (mText.Contains("Incorrect Login Type"))
                {
                    //해당 구글 주소가 구글 로그인 방식이 아닌 다른 방식으로 가입한 기록이 있을 시
                }
                else
                {
                    //기타 에러 메시지 출력 
                }
            }
            else
            {
                Dictionary<string, string> mDic = new Dictionary<string, string>();
                mDic.Add("Type", "google");
                mDic.Add("Json", mJson);

                string sJson = JsonConvert.SerializeObject(mDic);

                DataManager.Instance.CurrentUser = sJson;   //로그인한 유저 정보를 현재 유저 정보에 저장
                if (aAction != null)
                {
                    aAction.Invoke(Network_Result_State.Response, www.downloadHandler.text); //aAction에 셋팅된 함수 호출
                }
            }
        }
    }
}