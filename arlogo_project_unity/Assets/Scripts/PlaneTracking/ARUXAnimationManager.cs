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
    [Tooltip("�ȳ� �ؽ�Ʈ ���� ���̾� �ڽ�")]
    GameObject m_DialogBoxCommon;

    public GameObject DialogBoxCommon
    {
        get => m_DialogBoxCommon;
        set => m_DialogBoxCommon = value;
    }

    [SerializeField]
    [Tooltip("�ȳ� �ؽ�Ʈ ����")]
    TMP_Text m_InstructionText;

    public TMP_Text instructionText
    {
        get => m_InstructionText;
        set => m_InstructionText = value;
    }

    [SerializeField]
    [Tooltip("��Ŀ ���� �̹���")]
    Image m_Image;

    public Image planeTrackingImage
    {
        get => m_Image;
        set => m_Image = value;
    }


    [SerializeField]
    [Tooltip("���̵��� �ɸ��� �ð�")]
    float m_FadeOnDuration = 1.0f;
    [SerializeField]
    [Tooltip("���̵� �ƿ� �ɸ��� �ð�")]
    float m_FadeOffDuration = 0.5f;

    //�𸣰���
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

    const string k_MoveDeviceText = "��Ŀ�� ������ ������ ī�޶� ������ ������ ��ĵ�� �ּ���.";
    const string k_TapToPlaceText = "ȭ�� ��ġ �� ��ü�� ��ġ�� ������ �� �ֽ��ϴ�.";

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
