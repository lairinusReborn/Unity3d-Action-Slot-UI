﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lairinus.UI
{
    public class ActionSlotUI : MonoBehaviour
    {
        /// <summary>
        /// Configuration for when the Action is currently being used (casting), or when an action is already used and this is tracking its' duration (buff time, active time, etc)
        /// </summary>
        [SerializeField] private Configuration _actionActiveConfiguration = new Configuration();

        /// <summary>
        /// Configuration for when you cannot use the action for a certain period of time
        /// </summary>
        [SerializeField] private Configuration _actionCooldownConfiguration = new Configuration();

        /// <summary>
        /// Configuration to handle disabled and active states
        /// </summary>
        [SerializeField] private StateConfiguration _disabledConfiguration = new StateConfiguration();

        /// <summary>
        /// Configuration for the base UI element
        /// </summary>
        [SerializeField] private StateConfiguration _normalConfiguration = new StateConfiguration();

        // The Action model that will be used
        private IActionObject _actionModel = null;

        /// <summary>
        /// The button that is clicked to activate this Action from its' Action Slot
        /// </summary>
        [SerializeField]
        private Button _actionSlotButton = null;

        // For use with the UpdateActionSlotRoutine() coroutine
        private bool _automaticUpdatesRunning = false;

        /// <summary>
        /// If true, we can debug messages to the console
        /// </summary>
        [SerializeField] private bool _enableDebugging = false;

        /// <summary>
        /// While enabled, this slot will not show its' disabled configuration. If this slot is disabled, the disabled configuration will show
        /// </summary>
        [SerializeField] private bool _isEnabled = true;

        [SerializeField] private bool _showOnAwake = true;
        private GameObject _thisGameObject = null;
        public bool enableDebugging { get { return _enableDebugging; } set { _enableDebugging = value; } }
        public bool isEnabled { get { return _isEnabled; } }

        /// <summary>
        /// Sets this slot's disabled flag. A disabled slot typically means that players cannot interact with it.
        /// </summary>
        public void EnableActionSlot(bool isEnabled)
        {
            _isEnabled = isEnabled;
            _disabledConfiguration.ShowInternal(!isEnabled);
        }

        /// <summary>
        /// Binds the Action model to the controller/view
        /// </summary>
        public void SetAction(IActionObject actionObject)
        {
            _actionModel = actionObject;
        }

        /// <summary>
        /// Sets the action slot's main icon image
        /// </summary>
        /// <param name="icon"></param>
        public void SetActionIcon(Sprite icon)
        {
            _normalConfiguration.SetSpriteInternal(icon);
        }

        /// <summary>
        /// The Action Slot button is used to call the Action.UseAction() function on the model. 
        /// </summary>
        /// <param name="button"></param>
        public void SetActionSlotButton(Button button)
        {
            // Remove the Click from the previous button
            if (_actionSlotButton != null)
                _actionSlotButton.onClick.RemoveListener(() => OnClick_UseAction());

            // Add the Click to the current button
            _actionSlotButton = button;
            if (_actionSlotButton != null)
                _actionSlotButton.onClick.AddListener(() => OnClick_UseAction());
        }

        /// <summary>
        /// Sets the "Disabled" state sprite dynamically
        /// </summary>
        /// <param name="sprite"></param>
        public void SetDisabledSprite(Sprite sprite)
        {
            _disabledConfiguration.SetSpriteInternal(sprite);
        }

        /// <summary>
        /// Shows or hides this Action Slot
        /// </summary>
        /// <param name="show"></param>
        public void ShowActionSlot(bool show)
        {
            if (_thisGameObject == null)
                _thisGameObject = gameObject;

            _thisGameObject.SetActive(show);
            _disabledConfiguration.ShowInternal(!_isEnabled);
        }

        /// <summary>
        /// Updates the ActionSlot to reflect the Action's cooldowns and durations.
        /// If "automaticUpdates" is true, it will start a Coroutine to automatically handle the updates.
        /// If you are updating multiple action slots at the same time, it will give you better performance to set "useCoroutine" as false.
        /// </summary>
        public void UpdateActionSlot(bool automaticUpdates)
        {
            // User wants automatic updates
            if (automaticUpdates && !_automaticUpdatesRunning)
            {
                // Stops and Starts in case the Corotuine is already running
                StopCoroutine("UpdateActionSlotRoutine");
                StartCoroutine("UpdateActionSlotRoutine");
            }
            // User took control of updates; we don't need to do anything. Also stop the automatic update in case it's still going on
            else
            {
                if (_automaticUpdatesRunning)
                {
                    _automaticUpdatesRunning = false;
                    StopCoroutine("UpdateActionSlotRoutine");
                }

                UpdateActionSlotInternal();
            }
        }

        /// <summary>
        /// UnitEngine default - same as a class constructor
        /// </summary>
        private void Awake()
        {
            _thisGameObject = gameObject;
            ShowActionSlot(_showOnAwake);
            SetActionSlotButton(_actionSlotButton);
        }

        /// <summary>
        /// Interally maps the action to the button click
        /// </summary>
        private void OnClick_UseAction()
        {
            if (_actionModel != null)
                _actionModel.UseAction();
        }
        // Reads the IActionObject item and causes the appropriate updates
        private void UpdateActionSlotInternal()
        {
            if (_actionModel == null)
            {
                if (_enableDebugging)
                    Debug.LogError(Debugger.actionModelIsNull);

                return;
            }
            _actionActiveConfiguration.UpdateInternal(_actionModel.remainingDurationTime, _actionModel.totalDurationTime, _enableDebugging);
            _actionCooldownConfiguration.UpdateInternal(_actionModel.remainingCooldownTime, _actionModel.totalCooldownTime, _enableDebugging);
            _disabledConfiguration.ShowInternal(!_isEnabled);
        }

        // Automatically updates the Action Slot
        private IEnumerator UpdateActionSlotRoutine()
        {
            _automaticUpdatesRunning = true;
            while (_automaticUpdatesRunning)
            {
                yield return null;
                UpdateActionSlotInternal();
            }
        }
        /// <summary>
        /// Holds UI references as well as Text optiosn for text display
        /// </summary>
        [System.Serializable]
        public class Configuration
        {
            public TextFormattingOption textFormattingOption = TextFormattingOption.SecondsOnly;

            [SerializeField] private bool _showText = false;

            private GameObject _textCachedGameObject = null;

            [SerializeField] private bool _useConfiguration = true;

            [SerializeField] private Image filledImage = null;

            [SerializeField] private GameObject rootObject = null;

            [SerializeField] private Text textObject = null;

            public enum TextFormattingOption
            {
                SecondsOnly,    // Shows only seconds regardless of how much time the Action Slot is in the "Active" state or "Cooldown" state for
                MinutesOnly,    // Shows only minutes regardless of how much time the Action Slot is in the "Active" state or "Cooldown" state for
                HoursOnly,      // Shows only hours regardless of how much time the Action Slot is in the "Active" state or "Cooldown" state for
                HoursThenMinutesThenSeconds // If the remaining time is greater than an hour, this shows hours. If it is greater than a minute, it shows minutes. All other cases show seconds. Under 1 second shows the thousandths place
            }

            /// <summary>
            /// If true, this will show the Text object if there is a text object attached to this class
            /// </summary>
            public bool showText { get { return _showText; } set { _showText = true; } }

            /// <summary>
            /// If flagged to use this configuration, it will show when a current state is encountered.
            /// For instance, if the Action Slot is in the "Active" state it will show "Active"
            /// </summary>
            public bool useConfiguration { get { return _useConfiguration; } set { _useConfiguration = true; } }

            /// <summary>
            /// Updates the values in the configuration to show/hide appropriately
            /// </summary>
            /// <param name="remainingTime"></param>
            /// <param name="totalTime"></param>
            public void UpdateInternal(float remainingTime, float totalTime, bool _enableDebugging)
            {
                if (remainingTime == 0)
                    ShowInternal(false);
                else
                {
                    if (useConfiguration && remainingTime > 0)
                    {
                        ShowInternal(true);
                        SetTextInternal(remainingTime);
                        SetFillAmountInternal(remainingTime, totalTime, _enableDebugging);
                    }
                    else
                        ShowInternal(false);
                }
            }

            /// <summary>
            /// Sets the image's fill percent based on the remaining time / total time
            /// </summary>
            /// <param name="remainingTime"></param>
            /// <param name="totalTime"></param>
            private void SetFillAmountInternal(float remainingTime, float totalTime, bool _debuggingEnabled)
            {
                if (filledImage == null)
                {
                    if (_debuggingEnabled)
                        Debug.LogError(Debugger.fillImageIsNull);

                    return;
                }

                filledImage.type = Image.Type.Filled;
                filledImage.fillAmount = remainingTime / totalTime;
            }

            /// <summary>
            /// If there's a text object attached here, it sets it so long as it is allowed to be used according to the showText flag.
            /// </summary>
            /// <param name="remainingSeconds"></param>
            private void SetTextInternal(float remainingSeconds)
            {
                // Hide and cache the text GameObject to get better performance (provided this object needs to be hidden)
                if (textObject != null)
                {
                    if (_textCachedGameObject == null)
                        _textCachedGameObject = textObject.gameObject;

                    _textCachedGameObject.SetActive(_showText && remainingSeconds > 0);
                }

                if (_showText)
                {
                    // If the text object is null, we can't show the text
                    if (textObject == null)
                    {
                        Debug.LogError(Debugger.textObjectIsNull);
                        return;
                    }

                    // Calculate the text value to display on the textObject UI
                    int hourConversion = 3600;
                    int minuteConversion = 60;
                    int fixedAddition = 1;
                    float totalHours = Mathf.Floor(remainingSeconds / hourConversion);
                    float totalMinutes = Mathf.Floor(remainingSeconds / minuteConversion);
                    float totalSeconds = Mathf.Floor(remainingSeconds);
                    double totalRoundedSeconds = System.Math.Round(remainingSeconds, 2);
                    switch (textFormattingOption)
                    {
                        case TextFormattingOption.SecondsOnly:
                            float ceilingSeconds = Mathf.Ceil(remainingSeconds);
                            textObject.text = ceilingSeconds.ToString() + "s";
                            break;

                        case TextFormattingOption.MinutesOnly:
                            textObject.text = (totalMinutes + fixedAddition).ToString() + "m";
                            break;

                        case TextFormattingOption.HoursOnly:
                            textObject.text = (totalHours + fixedAddition).ToString() + "h";
                            break;

                        case TextFormattingOption.HoursThenMinutesThenSeconds:
                            {
                                // Check if we can show hours / minutes / seconds
                                if (totalHours > 0)
                                {
                                    textObject.text = totalHours.ToString() + "h";
                                }

                                // Check if we can show minutes instead
                                else if (totalMinutes > 0)
                                {
                                    textObject.text = totalMinutes.ToString() + "m";
                                }

                                // Check if we can show seconds instead
                                else if (remainingSeconds > 1)
                                {
                                    textObject.text = totalSeconds.ToString() + "s";
                                }

                                // We're showing partial seconds lower than 1
                                else
                                {
                                    textObject.text = totalRoundedSeconds.ToString() + "s";
                                }
                            }
                            break;
                    }
                }
            }

            private void ShowInternal(bool show)
            {
                if (rootObject == null)
                {
                    Debug.LogError(Debugger.rootObjectIsNull);
                    return;
                }

                rootObject.SetActive(show);
            }
        }

        /// <summary>
        /// The configuration for other Action Slot states not including "Active" and "Cooldown"
        /// </summary>
        [System.Serializable]
        public class StateConfiguration
        {
            [SerializeField] private Image _imageObjectUI = null;
            [SerializeField] private GameObject _rootObject = null;
            [SerializeField] private Text _textObjectUI = null;

            /// <summary>
            /// Sets the sprite for the ActionSlot's disabled state
            /// </summary>
            /// <param name="sprite"></param>
            public void SetSpriteInternal(Sprite sprite)
            {
                if (_imageObjectUI == null)
                {
                    Debug.LogError(Debugger.textObjectIsNullAndCantBeSet.Replace("%%custom%%", "SetSpriteInternal()"));
                    return;
                }

                _imageObjectUI.sprite = sprite;
            }

            /// <summary>
            /// Sets the text for this configuration's "_textObjectUI" field
            /// </summary>
            /// <param name="text"></param>
            public void SetTextInternal(string text)
            {
                if (_textObjectUI == null)
                {
                    Debug.LogError(Debugger.textObjectIsNullAndCantBeSet.Replace("%%custom%%", "SetTextInternal()"));
                    return;
                }

                _textObjectUI.text = text;
            }

            /// <summary>
            /// Shows the UI object internally
            /// </summary>
            public void ShowInternal(bool isShown)
            {
                if (_rootObject != null)
                {
                    _rootObject.SetActive(isShown);
                }
            }
        }

        /// <summary>
        /// Contains general debugging strings to give the user some indication of what went wrong
        /// </summary>
        private class Debugger
        {
            public const string actionModelIsNull = "Error: The ActionModel associated with this action slot is null. Please set an Action model in order to use this Slot!";
            public const string fillImageIsNull = "Error! Cannot set the value of the image because it doesn't exist!";
            public const string imageObjectIsNullAndCantBeSet = "Error: the UnityEngine.UI.Image object inside of %%custom%% is null, so the sprite cannot be set";
            public const string rootObjectIsNull = "Error! The root object attached to this Configuration script is null! You must assign this value for the 'ShowConfiguration' script to work!";
            public const string textObjectIsNull = "Error! The text object is null inside of this ActionSlotUI configuration object, so no text can be shown!";
            public const string textObjectIsNullAndCantBeSet = "Error: the UnityEngine.UI.Text object inside of %%custom%% is null, so the text cannot be set";
        }
    }
}