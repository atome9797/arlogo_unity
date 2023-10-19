using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ARUXAnimationManager : MonoBehaviour
{

    [SerializeField]
    [Tooltip("안내 텍스트 문구 다이얼 박스")]
    GameObject m_DialogBoxCommon;

    public GameObject DialogBoxCommon
    {
        get => m_DialogBoxCommon;
        set => m_DialogBoxCommon = value;
    }

    [SerializeField]
    [Tooltip("안내 텍스트 문구")]
    TMP_Text m_InstructionText;

    public TMP_Text instructionText
    {
        get => m_InstructionText;
        set => m_InstructionText = value;
    }

    [SerializeField]
    [Tooltip("앵커 감지 이미지")]
    Image m_Image;

    public Image planeTrackingImage
    {
        get => m_Image;
        set => m_Image = value;
    }


    [SerializeField]
    [Tooltip("페이드인 걸리는 시간")]
    float m_FadeOnDuration = 1.0f;
    [SerializeField]
    [Tooltip("페이드 아웃 걸리는 시간")]
    float m_FadeOffDuration = 0.5f;

    //모르겠음
    Color m_AlphaWhite = new Color(1, 1, 1, 0);
    Color m_White = new Color(1, 1, 1, 1);

    Color m_TargetColor;
    Color m_StartColor;
    Color m_LerpingColor;
    bool m_FadeOn;
    bool m_FadeOff;
    bool m_Tweening;
    float m_TweenTime;
    float m_TweenDuration;

    const string k_MoveDeviceText = "앵커가 생성될 때까지 카메라를 움직여 주위를 스캔해 주세요.";
    const string k_TapToPlaceText = "화면 터치 시 물체의 위치를 지정할 수 있습니다.";

    public static event Action onFadeOffComplete;

    private void Start()
    {
        m_StartColor = m_AlphaWhite;
        m_TargetColor = m_White;
    }

    void Update()
    {

        if (m_FadeOff || m_FadeOn)
        {
            if (m_FadeOn)
            {
                m_StartColor = m_AlphaWhite;
                m_TargetColor = m_White;
                m_TweenDuration = m_FadeOnDuration;
                m_FadeOff = false;
            }

            if (m_FadeOff)
            {
                m_StartColor = m_White;
                m_TargetColor = m_AlphaWhite;
                m_TweenDuration = m_FadeOffDuration;

                m_FadeOn = false;
            }

            if (m_TweenTime < 1)
            {
                m_TweenTime += Time.deltaTime / m_TweenDuration;
                m_LerpingColor = Color.Lerp(m_StartColor, m_TargetColor, m_TweenTime);
                m_Image.color = m_LerpingColor;
                instructionText.color = m_LerpingColor;

                m_Tweening = true;
            }
            else
            {
                m_TweenTime = 0;
                m_FadeOff = false;
                m_FadeOn = false;
                m_Tweening = false;

                // was it a fade off?
                if (m_TargetColor == m_AlphaWhite)
                {
                    if (onFadeOffComplete != null)
                    {
                        onFadeOffComplete();
                    }
                }
            }
        }
    }

    public void ShowTapToPlace()
    {
        //instructionText.text = k_TapToPlaceText;
        instructionText.gameObject.SetActive(false);
        m_Image.gameObject.SetActive(false);
        DialogBoxCommonView(true);
        m_FadeOn = true;
    }

    public void DialogBoxCommonView(bool check)
    {
        DialogBoxCommon.SetActive(check);
    }

    public void ShowCrossPlatformFindAPlane()
    {
        instructionText.text = k_MoveDeviceText;
        m_Image.gameObject.SetActive(true);
        m_FadeOn = true;
    }

    public void FadeOffCurrentUI()
    {

        if (planeTrackingImage.sprite != null)
        {
            // handle exiting fade out early if currently fading out another Clip
            if (m_Tweening || m_FadeOn)
            {
                // stop tween immediately
                m_TweenTime = 1.0f;
                planeTrackingImage.color = m_AlphaWhite;
                instructionText.color = m_AlphaWhite;
                if (onFadeOffComplete != null)
                {
                    onFadeOffComplete();
                }
            }

            m_FadeOff = true;
        }
    }

}
