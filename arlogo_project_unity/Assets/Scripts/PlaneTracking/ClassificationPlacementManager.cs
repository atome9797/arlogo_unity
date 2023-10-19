using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using System.IO;

public class ClassificationPlacementManager : MonoBehaviour
{

    bool m_CanReposition = false;

    public bool CanReposition
    {
        get => m_CanReposition;
        set => m_CanReposition = value;
    }

    public static event Action onPlacedObject;

    [SerializeField]
    Sprite btn_capture_off;

    [SerializeField]
    Sprite btn_capture_on;


    [SerializeField]
    Button m_CaptureButton;

    public Button CaptureButton
    {
        get => m_CaptureButton;
        set => m_CaptureButton = value;
    }



    [SerializeField]
    List<GameObject> m_FloorPrefabs;

    public List<GameObject> floorPrefabs
    {
        get => m_FloorPrefabs;
        set => m_FloorPrefabs = value;
    }

    [SerializeField]
    List<GameObject> m_TablePrefabs;


    public List<GameObject> tablePrefabs
    {
        get => m_TablePrefabs;
        set => m_TablePrefabs = value;
    }

    [SerializeField]
    PlacementReticle m_Reticle;

    public PlacementReticle reticle
    {
        get => m_Reticle;
        set => m_Reticle = value;
    }


    [SerializeField]
    UIManager m_UIManager;

    public UIManager uiManager
    {
        get => m_UIManager;
        set => m_UIManager = value;
    }


    [SerializeField]
    Transform m_ARCameraTransform;

    public Transform arCameraTransform
    {
        get => m_ARCameraTransform;
        set => m_ARCameraTransform = value;
    }

    GameObject m_SpawnedObject = null;

    public GameObject SpawnedObject
    {
        get => m_SpawnedObject;
        set => m_SpawnedObject = value;
    }

    Ease m_TweenEase = Ease.OutQuart;

    const float k_TweenTime = 0.4f;

    //[SerializeField]
    //Button m_TouchButton;

    /*
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && m_CanReposition)
            {
                RemoveObject();
                PlaceFloorObject(0);
            }
        }
    }*/

    [SerializeField]
    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private RaycastHit hit;

    [SerializeField]
    int m_MaxNumberOfObjectsToPlace = 1; //생성해야 할 오브젝트 갯수

    int m_NumberOfPlacedObjects = 0;// 처음 오브젝트 갯수

    bool m_editUI = false;

    public bool EditUI
    {
        get => m_editUI;
        set => m_editUI = value;
    }


    [SerializeField]
    Camera m_Camera;

    int ItemNumber;
    string LogoImage;

    [SerializeField]
    Material TestMats;

    private void Start()
    {
        ItemNumber = PlayerPrefs.GetInt("Item");
        LogoImage = PlayerPrefs.GetString("Logo");

        for(int i = 0; i < m_FloorPrefabs.Count; i++)
        {
            m_FloorPrefabs[i].SetActive(false);
        }
    }

    
    void Update()
    {

        //UI 를 제외한 클릭시
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            Touch touch = Input.GetTouch(0);


            if (m_NumberOfPlacedObjects < m_MaxNumberOfObjectsToPlace && m_CanReposition)
            {
                if (touch.phase == TouchPhase.Began && m_Reticle.GetReticlePosition().gameObject.activeSelf)
                {
                    Clear();
                    PlaceFloorObject(ItemNumber);
                    m_editUI = true;
                }
            }
            else if(m_NumberOfPlacedObjects >= m_MaxNumberOfObjectsToPlace && !m_CanReposition)
            {
                Ray ray = m_Camera.ScreenPointToRay(Input.GetTouch(0).position);
                
                //_Text.text = "inner7";

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        //_Text.text = "inner8";
                        //박스 콜라이더 활성화
                        //한번만 활성화 시키기
                        ActiveBoxCollider();
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                        {
                            //_Text.text = "inner9";
                            Pose hitPose = s_Hits[0].pose;
                            
                            m_SpawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                        }                        
                    }
                }

            }
        }
    }

    void ActiveBoxCollider()
    {
        if (m_editUI)
        {
            m_editUI = !m_editUI;
            SpawnedObjectCollider(true);
            uiManager.EditorUI(true);
        }
    }



    public void SpawnedObjectCollider(bool check)
    {
        m_SpawnedObject.GetComponent<DimBoxes.BoundBox>().wire_renderer = check;
    }


    public void Clear()
    {
        if (m_SpawnedObject != null)
        {
            //Destroy(m_SpawnedObject);
            m_SpawnedObject.SetActive(false);
            m_SpawnedObject = null;
            m_NumberOfPlacedObjects = 0;
            //캡처 버튼 비활성화
            m_CaptureButton.GetComponent<Image>().sprite = btn_capture_off;
            m_CaptureButton.GetComponent<Button>().interactable = false;
        }
    }


    public void PlaceFloorObject(int indexToPlace)
    {
        m_FloorPrefabs[indexToPlace].SetActive(true);
        m_SpawnedObject = m_FloorPrefabs[indexToPlace];
        m_SpawnedObject.transform.position = m_Reticle.GetReticlePosition().position;
        m_SpawnedObject.transform.rotation = m_Reticle.GetReticlePosition().rotation;

        //m_SpawnedObject = Instantiate(m_FloorPrefabs[indexToPlace], m_Reticle.GetReticlePosition().position, m_Reticle.GetReticlePosition().rotation);

        m_SpawnedObject.transform.localScale = Vector3.zero;

        m_SpawnedObject.transform.LookAt(m_ARCameraTransform, Vector3.up);
        m_SpawnedObject.transform.rotation = Quaternion.Euler(0, m_SpawnedObject.transform.eulerAngles.y + 180f, 0);

        m_SpawnedObject.transform.DOScale(new Vector3(0.2f,0.2f,0.2f) , k_TweenTime).SetEase(m_TweenEase);
        m_SpawnedObject.GetComponent<DimBoxes.BoundBox>().wire_renderer = false;
        //좌표 원 비활성화
        m_CanReposition = !m_CanReposition;
        //캡처 버튼 활성화
        m_CaptureButton.GetComponent<Image>().sprite = btn_capture_on;
        m_CaptureButton.GetComponent<Button>().interactable = true;


        //로고 이미지 불러오기
        
        if (File.Exists(LogoImage)) 
        {

            byte[] byteTexture = File.ReadAllBytes(LogoImage);

            Texture2D texture = new Texture2D(0, 0);

            texture.LoadImage(byteTexture);

            TestMats.mainTexture = texture;

        }
        

        //바닥 트래킹 성공여부 true로 설정
        if (onPlacedObject != null)
        {
            onPlacedObject();
        }

        m_NumberOfPlacedObjects++;
        //_Text.text = $"{ItemNumber}/{LogoImage}/{m_NumberOfPlacedObjects}";
    }

    public void PlaceTableObject(int indexToPlace)
    {
        m_SpawnedObject = Instantiate(m_TablePrefabs[indexToPlace], m_Reticle.GetReticlePosition().position, m_Reticle.GetReticlePosition().rotation);
        m_SpawnedObject.transform.localScale = Vector3.zero;
        // look at device but stay 'flat'
        m_SpawnedObject.transform.LookAt(m_ARCameraTransform, Vector3.up);
        m_SpawnedObject.transform.rotation = Quaternion.Euler(0, m_SpawnedObject.transform.eulerAngles.y, 0);

        m_SpawnedObject.transform.DOScale(Vector3.one, k_TweenTime).SetEase(m_TweenEase);
    }
}
