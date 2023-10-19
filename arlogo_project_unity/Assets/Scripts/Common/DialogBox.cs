using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class DialogBox : MonoBehaviour
{
    [SerializeField]
    Button CheckBtn;

    [SerializeField]
    Button CancelBtn;

    [SerializeField]
    ARSession _ARSession;



    private void Awake()
    {
        CheckBtn.onClick.AddListener(Check);
        CancelBtn.onClick.AddListener(Cancel);
    }

    void Check()
    {
        //데이터 다시보내기
        
        _ARSession.Reset();
        SceneManager.LoadScene("MainScene");
    }

    void Cancel()
    {
        gameObject.SetActive(false);
    }
    
}
