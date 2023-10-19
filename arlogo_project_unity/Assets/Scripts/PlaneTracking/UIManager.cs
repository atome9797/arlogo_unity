using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public struct UXHandle
{
    public UIManager.InstructionUI InstructionalUI;
    public UIManager.InstructionGoals Goal;

    public UXHandle(UIManager.InstructionUI ui, UIManager.InstructionGoals goal)
    {
        InstructionalUI = ui;
        Goal = goal;
    }
}

public class UIManager : MonoBehaviour
{

    [SerializeField]
    bool m_FindPlane = false;

    public bool FindPlane
    {
        get => m_FindPlane;
        set => m_FindPlane = value;
    }

    [SerializeField]
    ClassificationPlacementManager m_ClassificationPlacementManager;

    [SerializeField]
    Camera m_Camera;

    [SerializeField]
    ARSession _ARSession;

    [SerializeField]
    Image TouchButtonImg;

    [SerializeField]
    GameObject DialogBox;

    [SerializeField]
    Button m_CaptureBtn;
    public Button CaptureBtn
    {
        get => m_CaptureBtn;
        set => m_CaptureBtn = value;
    }

    [SerializeField]
    Button m_ResetBtn;
    public Button ResetBtn
    {
        get => m_ResetBtn;
        set => m_ResetBtn = value;
    }

    [SerializeField]
    Button m_BackBtn;
    public Button BackBtn
    {
        get => m_BackBtn;
        set => m_BackBtn = value;
    }

    [SerializeField]
    Button m_SaveBtn;
    public Button SaveBtn
    {
        get => m_SaveBtn;
        set => m_SaveBtn = value;
    }

    [SerializeField]
    Button m_RemoveBtn;
    public Button RemoveBtn
    {
        get => m_RemoveBtn;
        set => m_RemoveBtn = value;
    }


    [SerializeField]
    bool m_StartWithInstructionalUI = true;

    public bool startWithInstructionalUI
    {
         get => m_StartWithInstructionalUI;
         set => m_StartWithInstructionalUI = value;
    }

    public enum InstructionUI
    {
        CrossPlatformFindAPlane,
        TapToPlace,
        None
    };

    [SerializeField]
    InstructionUI m_InstructionalUI;

    public InstructionUI instructionalUI
    {
        get => m_InstructionalUI;
        set => m_InstructionalUI = value;
    }

    public enum InstructionGoals
    {
        FoundAPlane,
        PlacedAnObject,
        None
    };

    [SerializeField]
    InstructionGoals m_InstructionalGoal;
    
    public InstructionGoals instructionalGoal
    {
        get => m_InstructionalGoal;
        set => m_InstructionalGoal = value;
    }

    [SerializeField]
    bool m_ShowSecondaryInstructionalUI;
    
    public bool showSecondaryInstructionalUI
    {
        get => m_ShowSecondaryInstructionalUI;
        set => m_ShowSecondaryInstructionalUI = value;
    }

    [SerializeField]
    InstructionUI m_SecondaryInstructionUI = InstructionUI.TapToPlace;

    public InstructionUI secondaryInstructionUI
    {
        get => m_SecondaryInstructionUI;
        set => m_SecondaryInstructionUI = value;
    }

    [SerializeField]
    InstructionGoals m_SecondaryGoal = InstructionGoals.PlacedAnObject;

    public InstructionGoals secondaryGoal
    {
        get => m_SecondaryGoal;
        set => m_SecondaryGoal = value;
    }

    [SerializeField]
    GameObject m_ARSessionOrigin;

    public GameObject arSessionOrigin
    {
        get => m_ARSessionOrigin;
        set => m_ARSessionOrigin = value;
    }

    Func<bool> m_GoalReached;
    bool m_SecondaryGoalReached;
    
    Queue<UXHandle> m_UXOrderedQueue;
    UXHandle m_CurrentHandle;
    bool m_ProcessingInstructions;
    bool m_PlacedObject;

    [SerializeField]
    ARPlaneManager m_PlaneManager;
    
    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    [SerializeField]
    ARUXAnimationManager m_AnimationManager;

    public ARUXAnimationManager animationManager
    {
        get => m_AnimationManager;
        set => m_AnimationManager = value;
    }

    bool m_FadedOff = false;


    void OnEnable()
    {
        ARUXAnimationManager.onFadeOffComplete += FadeComplete;

        ClassificationPlacementManager.onPlacedObject += () => m_PlacedObject = true;

        GetManagers();
        m_UXOrderedQueue = new Queue<UXHandle>();
        if (m_StartWithInstructionalUI)
        {
            m_UXOrderedQueue.Enqueue(new UXHandle(m_InstructionalUI, m_InstructionalGoal));
        }

        if (m_ShowSecondaryInstructionalUI)
        {
            m_UXOrderedQueue.Enqueue(new UXHandle(m_SecondaryInstructionUI, m_SecondaryGoal));
        }
    }

    void OnDisable()
    {
        ARUXAnimationManager.onFadeOffComplete -= FadeComplete;
    }

    void Reset()
    {
        //3d오브젝트가 있으면 삭제
        m_ClassificationPlacementManager.Clear();
        //트래킹 좌표 보이게 설정
        m_ClassificationPlacementManager.CanReposition = true;
    }

    void Awake()
    {
        m_ResetBtn.onClick.AddListener(Reset);
        m_RemoveBtn.onClick.AddListener(Remove);
        m_SaveBtn.onClick.AddListener(Save);
        m_BackBtn.onClick.AddListener(Back);
    }

    void Update()
    {

        if (m_UXOrderedQueue.Count > 0 && !m_ProcessingInstructions)
        {
            // pop off
            m_CurrentHandle = m_UXOrderedQueue.Dequeue();
            
            // exit instantly, if the goal is already met it will skip showing the first UI and move to the next in the queue 
            m_GoalReached = GetGoal(m_CurrentHandle.Goal);
            if (m_GoalReached.Invoke())
            {
                TargetPlaceTracking();
                return;
            }

            // fade on
            FadeOnInstructionalUI(m_CurrentHandle.InstructionalUI);
            m_ProcessingInstructions = true;
            m_FadedOff = false;
        }

        if (m_ProcessingInstructions)
        {
            // start listening for goal reached
            if (m_GoalReached.Invoke())
            {
                TargetPlaceTracking();

                // if goal reached, fade off
                if (!m_FadedOff)
                {
                    m_FadedOff = true;
                    m_AnimationManager.FadeOffCurrentUI();
                }
            }
        }
    }

    void TargetPlaceTracking()
    {
        if (m_CurrentHandle.Goal == InstructionGoals.FoundAPlane && !m_FindPlane)
        {
            m_ClassificationPlacementManager.CanReposition = !m_ClassificationPlacementManager.CanReposition;
            m_FindPlane = !m_FindPlane;
            EditorUI(false);
            Color color = TouchButtonImg.color;
            color.a = 0;
            TouchButtonImg.color = color;
        }
    }


    /// <summary>
    /// 제거
    /// </summary>
    void Remove()
    {
        //오브젝트 삭제
        Reset();
        //버튼 초기화
        EditorUI(false);
    }

    /// <summary>
    /// 콜라이더 제거
    /// </summary>
    void Save()
    {
        m_ClassificationPlacementManager.EditUI = true;
        m_ClassificationPlacementManager.SpawnedObjectCollider(false);
        EditorUI(false);
    }

    /// <summary>
    /// 메인 씬으로 이동
    /// </summary>
    void Back()
    {
        DialogBox.SetActive(true);
    }

    

    public void EditorUI(bool check)
    {
        m_SaveBtn.gameObject.SetActive(check);
        m_RemoveBtn.gameObject.SetActive(check);
        m_ResetBtn.gameObject.SetActive(!check);
        m_BackBtn.gameObject.SetActive(!check);
        m_CaptureBtn.gameObject.SetActive(true);
    }

    void GetManagers()
    {
        if (m_ARSessionOrigin)
        {
            if (m_ARSessionOrigin.TryGetComponent(out ARPlaneManager arPlaneManager))
            {
                m_PlaneManager = arPlaneManager;
            }
        }
    }
    
    Func<bool> GetGoal(InstructionGoals goal)
    {
        switch (goal)
        {
            case InstructionGoals.FoundAPlane:
                return PlanesFound;

            case InstructionGoals.PlacedAnObject:
                return PlacedObject;

            case InstructionGoals.None:
                return () => false;
        }

        return () => false;
    }

    void FadeOnInstructionalUI(InstructionUI ui)
    {
        switch (ui)
        {
            case InstructionUI.CrossPlatformFindAPlane:
                m_AnimationManager.ShowCrossPlatformFindAPlane();
                break;

            case InstructionUI.TapToPlace:
                m_AnimationManager.ShowTapToPlace();
                break;

            case InstructionUI.None:

                break;
        }
    }

    bool PlanesFound() => m_PlaneManager && m_PlaneManager.trackables.count > 0;


    void FadeComplete()
    {
        m_ProcessingInstructions = false;
    }

    bool PlacedObject()
    {
        // reset flag to be used multiple times
        if (m_PlacedObject)
        {
            m_PlacedObject = false;
            return true;
        }
        return m_PlacedObject;
    }

    public void AddToQueue(UXHandle uxHandle)
    {
        m_UXOrderedQueue.Enqueue(uxHandle);
    }

    public void TestFlipPlacementBool()
    {
        m_PlacedObject = true;
    }

}

