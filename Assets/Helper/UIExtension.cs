using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public static class UIExtension
{
    /// <summary>
    /// Fade CanvasGroup
    /// </summary>
    /// <param name="target"></param>
    /// <param name="fadeValue"></param>
    /// <param name="command"></param>
    public static void DoFadeGroup(this GameObject target, float fadeValue, Action command)
    {
        target.SetActive(true);
        target.GetComponent<CanvasGroup>().DOFade(fadeValue, .5f).SetEase(Ease.Linear).OnComplete(() => command.Invoke());
    }

    /// <summary>
    /// Horizontally transit RectTransform to given value, use for bringing off-screen UI Element to on-screen.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value"></param>
    /// <param name="command"></param>
    /// <param name="speed"></param>
    public static void DoHorizontalTransition(this GameObject target, float value, Action command, float speed = .3f)
    {
        target.SetActive(true);
        target.transform.GetComponent<RectTransform>()?.DOAnchorPos3DX(value, speed).SetEase(Ease.InOutQuad)
            .OnComplete(() => command?.Invoke());
    }

    /// <summary>
    /// Vertically transit RectTransform to given value, use for bringing off-screen UI Element to on-screen.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value"></param>
    /// <param name="command"></param>
    /// <param name="speed"></param>
    public static void DoVerticalTransition(this GameObject target, float value, Action command, float speed = .3f)
    {
        target.SetActive(true);
        target.transform.GetComponent<RectTransform>()?.DOAnchorPos3DY(value, speed).SetEase(Ease.InOutQuad)
            .OnComplete(() => command?.Invoke());
    }

    /// <summary>
    /// Horizontally transit deactivate panel to given value and disable deactivate panel. 
    /// Then, enable and transit activate panel to given value.
    /// </summary>
    /// <param name="panelToActivate"></param>
    /// <param name="panelToDeactivate"></param>
    /// <param name="activatePanelPos"></param>
    /// <param name="deactivatePanelPos"></param>
    /// <param name="speed"></param>
    public static void DoActiveDeactivateTransitionX(GameObject panelToActivate, GameObject panelToDeactivate, float activatePanelPos, 
        float deactivatePanelPos, float speed = 0.15f)
    {
        panelToDeactivate.transform.GetComponent<RectTransform>()?.DOAnchorPos3DX(deactivatePanelPos, speed).SetEase(Ease.Linear).
        OnComplete(() =>
        {
            panelToActivate.SetActive(true);

            panelToActivate.transform.GetComponent<RectTransform>()?.DOAnchorPos3DX(activatePanelPos, speed).SetEase(Ease.Linear).
            OnComplete(() => panelToDeactivate.SetActive(false));
        });
    }

    /// <summary>
    /// Vertically transit deactivate panel to given value and disable deactivate panel. 
    /// Then, enable and transit activate panel to given value.
    /// </summary>
    /// <param name="panelToActivate"></param>
    /// <param name="panelToDeactivate"></param>
    /// <param name="activatePanelPos"></param>
    /// <param name="deactivatePanelPos"></param>
    /// <param name="speed"></param>
    public static void DoActiveDeactivateTransitionY(GameObject panelToActivate, GameObject panelToDeactivate, float activatePanelPos,
    float deactivatePanelPos, float speed = 0.15f)
    {
        panelToDeactivate.transform.GetComponent<RectTransform>()?.DOAnchorPos3DY(deactivatePanelPos, speed).SetEase(Ease.Linear).
        OnComplete(() =>
        {
            panelToActivate.SetActive(true);

            panelToActivate.transform.GetComponent<RectTransform>()?.DOAnchorPos3DY(activatePanelPos, speed).SetEase(Ease.Linear).
            OnComplete(() => panelToDeactivate.SetActive(false));
        });
    }
}
