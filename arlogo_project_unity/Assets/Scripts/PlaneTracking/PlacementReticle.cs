﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementReticle : MonoBehaviour
{
    [SerializeField]
    ClassificationPlacementManager m_ClassificationPlacementManager;

    [SerializeField]
    bool m_SnapToMesh;

    public bool snapToMesh
    {
        get => m_SnapToMesh;
        set => m_SnapToMesh = value;
    }

    [SerializeField]
    UIManager m_UIManager;


    [SerializeField]
    ARRaycastManager m_RaycastManager;
    
    public ARRaycastManager raycastManager
    {
        get => m_RaycastManager;
        set => m_RaycastManager = value;
    }

    [SerializeField]
    GameObject m_ReticlePrefab;

    public GameObject reticlePrefab
    {
        get => m_ReticlePrefab;
        set => m_ReticlePrefab = value;
    }

    [SerializeField]
    bool m_DistanceScale;

    public bool distanceScale
    {
        get => m_DistanceScale;
        set => m_DistanceScale = value;
    }

    [SerializeField]
    Transform m_CameraTransform;

    public Transform cameraTransform
    {
        get => m_CameraTransform;
        set => m_CameraTransform = value;
    }

    [SerializeField]
    GameObject m_CommonDialogBox;

    public GameObject CommonDialogBox
    {
        get => m_CommonDialogBox;
        set => m_CommonDialogBox = value;
    }

    [SerializeField]
    ARUXAnimationManager m_AnimationManager;

    public ARUXAnimationManager AnimationManager
    {
        get => m_AnimationManager;
        set => m_AnimationManager = value;
    }

    GameObject m_SpawnedReticle;
    CenterScreenHelper m_CenterScreen;
    TrackableType m_RaycastMask;
    float m_CurrentDistance;
    float m_CurrentNormalizedDistance;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    const float k_MinScaleDistance = 0.0f;
    const float k_MaxScaleDistance = 1.0f;
    const float k_ScaleMod = 0.7f;

    
    
    void Start()
    {
        m_CenterScreen = CenterScreenHelper.Instance;
        if (m_SnapToMesh)
        {
            m_RaycastMask = TrackableType.PlaneEstimated;
        }
        else
        {
            m_RaycastMask = TrackableType.PlaneWithinPolygon;
        }

        m_SpawnedReticle = Instantiate(m_ReticlePrefab);
        m_SpawnedReticle.SetActive(false);
    }

    void Update()
    {

        if (m_RaycastManager.Raycast(m_CenterScreen.GetCenterScreen(), s_Hits, m_RaycastMask))
        {
            Pose hitPose = s_Hits[0].pose;
            m_SpawnedReticle.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            m_CommonDialogBox.SetActive(false);
            //트래킹 가능하고 물체가 없을 경우
            if (m_ClassificationPlacementManager.CanReposition)
            {
                m_SpawnedReticle.SetActive(true);
                m_AnimationManager.DialogBoxCommonView(true);
            }
            else
            {
                m_SpawnedReticle.SetActive(false);
                m_AnimationManager.DialogBoxCommonView(false);
            }
        }
        else
        {
            m_SpawnedReticle.SetActive(false);
            m_AnimationManager.DialogBoxCommonView(false);

            if(m_UIManager.FindPlane)
            {
                //특징점을 비춰주세여
                m_CommonDialogBox.SetActive(true);
            }
        }



        if (m_DistanceScale)
        {
            m_CurrentDistance = Vector3.Distance(m_SpawnedReticle.transform.position, m_CameraTransform.position);
            m_CurrentNormalizedDistance = ((Mathf.Abs(m_CurrentDistance - k_MinScaleDistance)) / (k_MaxScaleDistance - k_MinScaleDistance)) + k_ScaleMod;
            m_SpawnedReticle.transform.localScale = new Vector3(m_CurrentNormalizedDistance, m_CurrentNormalizedDistance, m_CurrentNormalizedDistance);
        }

    }

    public Transform GetReticlePosition()
    {
        return m_SpawnedReticle.transform;
    }
}
