using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using System.IO;

public class NativeAPI
{
#if UNITY_IOS && !UNITY_EDITOR
  [DllImport("__Internal")]
  public static extern void sendMessageToMobileApp(string message);
#endif
}

public enum pageType
{
    step1 = 0,
    step2
}


public class RNManager : MonoBehaviour
{
    [SerializeField]
    Button _BackButton;

    [SerializeField]
    Button _SaveButton;

    [SerializeField]
    Button _ARButton;

    [SerializeField]
    Button _NextButton;

    [SerializeField]
    Button _MainBackButton;

    [SerializeField]
    Button[] _ItemButtons;

    [SerializeField]
    GameObject[] _OnItems;

    int selectItemNum;

    [SerializeField]
    GameObject UI_Menu;

    [SerializeField]
    GameObject UI_3D;

    [SerializeField]
    GameObject[] Items;

    [SerializeField]
    Material[] ItemMats;

    [SerializeField]
    Material TestMats;

    //[SerializeField]
    //ItemLogo[] itemlogos;

    [SerializeField]
    Texture2D[] logos;

    [SerializeField]
    GameObject _DialogBoxType1;

    [SerializeField]
    GameObject _DialogBoxType2;

    [SerializeField]
    Button _DialogBoxCheck;

    [SerializeField]
    Button _DialogBoxCancel;

    [SerializeField]
    ScreenshotHandler _ScreenshotHandler;

    [SerializeField]
    GameObject[] Page;


    private void Awake()
    {

        _BackButton.onClick.AddListener(BackButtonFunc);
        _SaveButton.onClick.AddListener(() => _DialogBoxType2.SetActive(true));
        _DialogBoxCheck.onClick.AddListener(() => _ScreenshotHandler.CaptureRenderTexture());
        _DialogBoxCancel.onClick.AddListener(() => _DialogBoxType2.SetActive(false));

        _ARButton.onClick.AddListener(ARButtonFunc);

        _NextButton.onClick.AddListener(NextButtonFunc);
        _MainBackButton.onClick.AddListener(() => ReactNativeActiveFunc("stop"));

        for (int i = 0; i < _ItemButtons.Length; i++)
        {
            int index = i;

            _ItemButtons[i].onClick.AddListener(()=>
            {
                SelectItemButtonFunc(index);
            });
        }

        selectItemNum = -1;

    }

    private void Start()
    {
        //데이터가 있으면 불러오기
        if(DataManager.Instance._ItemData.target != null && DataManager.Instance._ItemData.target != "")
        {
            BackupStorage();
        }        
    }

    /// <summary>
    /// 아이템 선택
    /// </summary>
    /// <param name="num"></param>

    private void SelectItemButtonFunc(int num)
    {
        for (int i = 0; i < _ItemButtons.Length; i++)
        {
            _OnItems[i].SetActive(false);
        }

        _OnItems[num].SetActive(true);

        selectItemNum = num;

    }


    private void BackButtonFunc()
    {
        // 선택 메뉴로 돌아감
        if (DataManager.Instance._ItemData.target == "step1")
        {
            PageType(pageType.step1);
        }
        // 리엑트 화면으로
        else if (DataManager.Instance._ItemData.target == "step2")
        {
            ReactNativeActiveFunc("stop");
        }
    }

    /// <summary>
    /// 3D 모드로
    /// </summary>
    private void NextButtonFunc()
    {
        Debug.Log("Select Num : " + selectItemNum);

        if(selectItemNum > -1 )
        {
            PageType(pageType.step2);

            SelectItem(selectItemNum);


            // 선택한 아이템 로드
            //Load_SelectItem();
        }
    }

    void SelectItem(int selectItemNum)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].SetActive(false);
            if (i == selectItemNum)
            {
                Items[selectItemNum].SetActive(true);
            }
        }
    }
    

    void Load_SelectItem()
    {
        int ran =  UnityEngine.Random.Range(0, logos.Length);
        
        ItemMats[selectItemNum].mainTexture = logos[ran];

        Items[selectItemNum].SetActive(true);
    }

    /// <summary>
    /// ARScene 전환
    /// </summary>
    private void ARButtonFunc()
    {
        //로드전 아이템 세팅
        PlayerPrefs.SetInt("Item", selectItemNum);
        PlayerPrefs.SetString("Logo", DataManager.Instance._ItemData.LogoImage);

        SceneManager.LoadScene("ARScene");
    }


    /// <summary>
    /// react native 에서 데이터를 불러올때 사용하는 함수
    /// </summary>
    /// <param name="jsonData"></param>
    public void SetItem(string jsonData)
    {
        DataManager.Instance._ItemData = JsonUtility.FromJson<CurrentData>(jsonData);
        DataManager.Instance.JsonLoad(); //저장된 데이터 전체 불러오기

        ///맨 처음 단계부터 시작시(편집 화면에서 접근 시)
        if (DataManager.Instance._ItemData.target == "step1")
        {
            //3d 선택창 띄워줘야함
            if (File.Exists(DataManager.Instance._ItemData.LogoImage))
            {
                byte[] byteTexture = File.ReadAllBytes(DataManager.Instance._ItemData.LogoImage);

                Texture2D texture = new Texture2D(0, 0);

                texture.LoadImage(byteTexture);

                TestMats.mainTexture = texture;

                PageType(pageType.step1);
            }
        }
        else if (DataManager.Instance._ItemData.target == "step2")
        {
            //이전 편집된 모델 데이터 불러오기
            PageType(pageType.step2);
            GetStorageItem(); //선택된 썸네일의 데이터 불러와서 정렬
        }

    }

    public void BackupStorage()
    {
        ///맨 처음 단계부터 시작시(편집 화면에서 접근 시)
        if (DataManager.Instance._ItemData.target == "step1")
        {
            //3d 선택창 띄워줘야함
            if (File.Exists(DataManager.Instance._ItemData.LogoImage))
            {
                byte[] byteTexture = File.ReadAllBytes(DataManager.Instance._ItemData.LogoImage);

                Texture2D texture = new Texture2D(0, 0);

                texture.LoadImage(byteTexture);

                TestMats.mainTexture = texture;

                PageType(pageType.step1);
            }
        }
        else if (DataManager.Instance._ItemData.target == "step2")
        {
            //이전 편집된 모델 데이터 불러오기
            PageType(pageType.step2);
            GetStorageItem(); //선택된 썸네일의 데이터 불러와서 정렬
        }

    }


    /// <summary>
    /// 저장소에서 선택시 불러오는 함수
    /// </summary>
    public void GetStorageItem()
    {
        //3d 선택창 띄워줘야함
        if (File.Exists(DataManager.Instance._ItemData.LogoImage))
        {
            byte[] byteTexture = File.ReadAllBytes(DataManager.Instance._ItemData.LogoImage);

            Texture2D texture = new Texture2D(0, 0);

            texture.LoadImage(byteTexture);

            TestMats.mainTexture = texture;

            //모델 번호 세팅
            selectItemNum = DataManager.Instance.jsonTargetDataLoadItemIndex(DataManager.Instance._ItemData.Thumbnail);

            SelectItem(selectItemNum);
        }
    }


    /// <summary>
    /// 캡처후 썸네일 및 모델 포지션 저장
    /// </summary>
    public void SaveButtonFunc(string CurrentPath, byte[] bytes, string Type)
    {
        _DialogBoxType2.SetActive(false);
        if(Type == "insert")
        {
            File.WriteAllBytes(CurrentPath, bytes);
            ReactNativeActiveFunc(DataManager.Instance.SaveData(selectItemNum, CurrentPath));//모델 번호와 썸네일 저장
        }
        else
        {
            ReactNativeActiveFunc(DataManager.Instance.UpdateData(selectItemNum, CurrentPath));//모델 번호와 썸네일 저장
        }

    }

    public void PageType(pageType type)
    {
        for(int i = 0; i < Page.Length; i++)
        {
            Page[i].SetActive(false);
            if (i == (int)type)
            {
                Page[i].SetActive(true);
            }
        }
    }

    /// <summary>
    /// React Native 에 데이터 보내기
    /// </summary>
    /// <param name="type"></param>
    public void ReactNativeActiveFunc(string data)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.azesmwayreactnativeunity.ReactNativeUnityViewManager"))
            {
                jc.CallStatic("sendMessageToMobileApp", data);
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS && !UNITY_EDITOR
      NativeAPI.sendMessageToMobileApp("The button has been tapped!");
#endif
        }
    }

}