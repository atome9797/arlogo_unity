using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LogoData
{
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float RotationX;
    public float RotationY;
    public float RotationZ;
    public float ScaleX;
    public float ScaleY;
    public float ScaleZ;
    public string LogoImage;
}

[Serializable]
public class ModelData
{
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float RotationX;
    public float RotationY;
    public float RotationZ;
    public float ScaleX;
    public float ScaleY;
    public float ScaleZ;
}


[Serializable]
public class CurrentData
{
    public string target;
    public string LogoImage;
    public string Thumbnail;
    public string loadJson;
}

[Serializable]
public class SaveData
{
    public string LogoImage; //임시
    public int ModelIndex; //임시
    public string Thumbnail;
}

[Serializable]
public class SaveDataList
{
    public List<SaveData> DataList = new List<SaveData>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public CurrentData _ItemData = new CurrentData();
    //public ModelData modelData = new ModelData();
    //public LogoData logoData = new LogoData();
    public SaveDataList _SaveDataList = new SaveDataList();

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


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
    public string folderName = "ModelThumbnailFolder";
    private string FolderPath => $"{RootPath}/{folderName}"; //screenshothandler쪽에서 사용됨

    //private string path => Path.Combine(RootPath, "database.json");


    public string SaveData(int _ModelIndex, string _Thumbnail)
    {
        SaveData saveData = new SaveData();
        saveData.LogoImage = _ItemData.LogoImage;//로고 uri 추가
        saveData.ModelIndex = _ModelIndex;//모델 번호 추가
        saveData.Thumbnail = _Thumbnail;//썸네일 추가

        _SaveDataList.DataList.Add(saveData);
        string jsonData = JsonUtility.ToJson(_SaveDataList);
        //File.WriteAllText(path, jsonData);

        return jsonData;
    }

    public string UpdateData(int _ModelIndex, string _Thumbnail)
    {
        SaveData saveData = new SaveData();
        saveData.LogoImage = _ItemData.LogoImage;//로고 uri 추가
        saveData.ModelIndex = _ModelIndex;//모델 번호 추가
        saveData.Thumbnail = _Thumbnail;//썸네일 추가

        for(int i = 0; i < _SaveDataList.DataList.Count; i++)
        {
            if(_SaveDataList.DataList[i].Thumbnail == _Thumbnail)
            {
                _SaveDataList.DataList[i].ModelIndex = saveData.ModelIndex;
                _SaveDataList.DataList[i].LogoImage = saveData.LogoImage;
            }
        }

        string jsonData = JsonUtility.ToJson(_SaveDataList);
        //File.WriteAllText(path, jsonData);

        return jsonData;
    }

    /// <summary>
    /// 전체 데이터 로딩
    /// </summary>
    public void JsonLoad()
    {

        //파일이 없으면 리턴
        /*
        if (!File.Exists(path))
        {
            Debug.Log("No such saveFile exists");
            return;
        }*/

        //string loadJson = File.ReadAllText(path);

        if(_ItemData.loadJson != null && _ItemData.loadJson != "[]" && _ItemData.loadJson != "")
        {
            _SaveDataList = JsonUtility.FromJson<SaveDataList>(_ItemData.loadJson);
        }
    }

    /// <summary>
    /// 썸네일에 맞는 모델 아이디 불러오기
    /// </summary>
    /// <param name="Thumbnail"></param>
    /// <returns></returns>
    public int jsonTargetDataLoadItemIndex(string Thumbnail)
    {
        
        for(int i = 0; i < _SaveDataList.DataList.Count; i++)
        {
            if(_SaveDataList.DataList[i].Thumbnail == Thumbnail)
            {
                return _SaveDataList.DataList[i].ModelIndex;
            }
        }

        return -1;
    }

}
