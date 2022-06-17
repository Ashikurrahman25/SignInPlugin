using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class Alert : MonoBehaviour
{
    public int transitionValue;
    public static Alert instance;

    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button closeButton;

    public Color errorColor;
    public Color successColor;

    private IEnumerator destroyCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Init(string title,string msg, bool isSuccess)
    {
        if (!isSuccess)
            GetComponent<ProceduralImage>().color = errorColor;
        else
            GetComponent<ProceduralImage>().color = successColor;

        gameObject.SetActive(true);
        messageText.text = msg;
        titleText.text = title;
        destroyCoroutine = DestroySelf();
        closeButton.onClick.AddListener(PlayCloseAnim);
        PlayOpenAnim();
    }

    void PlayOpenAnim()
    {
        //transform.localScale = Vector3.zero;

        Sequence panelAnim = DOTween.Sequence();

        panelAnim.Append(transform.DOMoveY(transitionValue, 0.3f));

        //panelAnim.Append(transform.transform.DOPunchScale(Vector3.one * 0.1f, 0.75f, 8, 0));

        panelAnim.Play().OnComplete(() => StartCoroutine(destroyCoroutine));
    }

    void PlayCloseAnim()
    {
        StopCoroutine(destroyCoroutine);

        Sequence logoAnim = DOTween.Sequence();

        logoAnim.Append(transform.DOMoveY(-200f, 0.25f));

        //logoAnim.Append(transform.transform.DOScale(0, 0.1f));

        logoAnim.Play();
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(3f);
        PlayCloseAnim();
    }
}
