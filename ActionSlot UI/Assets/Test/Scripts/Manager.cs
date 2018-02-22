﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private List<Action> actions = new List<Action>();
    [SerializeField] private List<Lairinus.UI.ActionSlotUI> actionSlots = new List<Lairinus.UI.ActionSlotUI>();
    [SerializeField] private List<Button> actionButtons = new List<Button>();

    private void Update()
    {
        UpdateActionInput();
        UpdateActionSlots();
    }

    private void Awake()
    {
        InitializeActionSlots();
        InitializeActionButtons();
        OnClick_StartActions();
    }

    private void InitializeActionSlots()
    {
        for (var a = 0; a < actionSlots.Count; a++)
        {
            if (a < actions.Count && actions[a] != null)
            {
                actionSlots[a].SetActionIcon(actions[a].icon);
            }
        }
    }

    private void InitializeActionButtons()
    {
        for (var a = 0; a < actionButtons.Count; a++)
        {
            int capture = a;
            if (capture < actions.Count)
            {
                Action act = actions[capture];
                actionButtons[a].onClick.AddListener(() => TryStartAction(actionSlots[capture], actions[capture]));
            }
        }
    }

    private void TryStartAction(Lairinus.UI.ActionSlotUI actionSlot, Action action)
    {
        if (actionSlot != null)
        {
            if (!actionSlot.isEnabled)
                return;
        }

        if (action != null)
            action.StartAction();
    }

    private void UpdateActionInput()
    {
        for (var a = 0; a < actions.Count; a++)
        {
            if (actions[a] != null)
            {
                if (Input.GetKeyDown(actions[a].keycode))
                {
                    if (actionSlots[a].isEnabled)
                        actions[a].StartAction();
                    break;
                }
            }
        }
    }

    private void UpdateActionSlots()
    {
        for (var a = 0; a < actions.Count; a++)
        {
            Action action = actions[a];
            if (a < actionSlots.Count)
            {
                if (actionSlots[a] != null)
                {
                    actionSlots[a].UpdateActionSlot(action.remainingCooldown, action.totalCooldown, action.remainingDuration, action.totalDuration);
                }
            }
        }
    }

    #region Click Events

    /// <summary>
    /// Inspector access - sets the duration of the Action
    /// </summary>
    /// <param name="durationInSeconds"></param>
    public void OnClick_SetDuration(int durationInSeconds)
    {
        foreach (Action a in actions)
        {
            if (a != null)
            {
                a.ResetAction();
                a.SetDurationAndCooldown(durationInSeconds, a.totalCooldown);
            }
        }

        OnClick_StartActions();
    }

    /// <summary>
    /// Inspector access - sets the cooldown of the action
    /// </summary>
    /// <param name="cooldownInSeconds"></param>
    public void OnClick_SetCooldown(int cooldownInSeconds)
    {
        foreach (Action a in actions)
        {
            if (a != null)
            {
                a.ResetAction();
                a.SetDurationAndCooldown(a.totalDuration, cooldownInSeconds);
            }
        }

        OnClick_StartActions();
    }

    /// <summary>
    /// Inspector access - resets an action's cooldown and duration
    /// </summary>
    public void OnClick_ResetActions()
    {
        foreach (Action a in actions)
        {
            if (a != null)
            {
                a.ResetAction();
            }
        }

        foreach (Lairinus.UI.ActionSlotUI asu in actionSlots)
        {
            if (asu != null)
            {
                asu.UpdateActionSlot(0, 0, 0, 0);
            }
        }
    }

    /// <summary>
    /// Inspector access - starts all actions
    /// </summary>
    public void OnClick_StartActions()
    {
        for (var a = 0; a < actions.Count; a++)
        {
            Lairinus.UI.ActionSlotUI actionSlot = actionSlots[a];
            Action action = actions[a];
            TryStartAction(actionSlot, action);
        }
    }

    /// <summary>
    /// Inspector access - disables all actions
    /// </summary>
    public void OnClick_EnableAllActionSlots(bool enable)
    {
        foreach (Lairinus.UI.ActionSlotUI a in actionSlots)
        {
            if (a != null)
            {
                a.EnableActionSlot(enable);
            }
        }
    }

    #endregion Click Events
}