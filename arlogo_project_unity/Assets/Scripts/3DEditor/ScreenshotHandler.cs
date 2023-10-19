using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ScreenshotHandler : MonoBehaviour
{

    private Camera myCamera;
    private bool takeScreenshotOnNextFrame;

    public string folderName = "ModelThumbnailFolder";
    public string fileName => $"ModelThumbnail_{DateTime.Now.ToString("MMddHHmmss")}.png";

    [SerializeField]
    RNManager _RNManager;


    private string RootPath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.dataPath;
#elif UNITY_ANDROID
                //return $"/storage/emulated/0/DCIM/{Application.productName}/";
                return Application.persistentDataPath;
#endif
        }
    }
    private string FolderPath => $"{RootPath}/{folderName}";
    private string TotalPath => $"{FolderPath}/{fileName}";

    private string CurrentPath = null;


    private void Awake()
    {
        myCamera = gameObject.GetComponent<Camera>();
    }

    private int resWidth;
    private int resHeight;

    private void Start()
    {
        resWidth = Screen.width;
        resHeight = Screen.height;
    }


    /// <summary>
    /// 모델 포함한 썸네일 캡처시 사용
    /// </summary>
    public void CaptureRenderTexture()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        myCamera.targetTexture = rt;
        myCamera.Render();
        RenderTexture.active = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        
        screenShot.ReadPixels(rec, 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();

        string type = "insert";
        CurrentPath = TotalPath;

        for (int i = 0; i < DataManager.Instance._SaveDataList.DataList.Count; i++)
        {
            //로고 이미지가 있으면 업데이트 없으면 insert
            if (DataManager.Instance._SaveDataList.DataList[i].Thumbnail == DataManager.Instance._ItemData.Thumbnail)
            {
                type = "update";
                CurrentPath = DataManager.Instance._SaveDataList.DataList[i].Thumbnail;
            }
        }

        _RNManager.SaveButtonFunc(CurrentPath, bytes, type);

    }

}
