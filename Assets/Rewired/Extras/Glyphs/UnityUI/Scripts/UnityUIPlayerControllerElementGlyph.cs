// Copyright (c) 2024 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#pragma warning disable 0649

namespace Rewired.Glyphs.UnityUI {
    using Rewired;

    /// <summary>
    /// Displays the controller element glyph(s) for a particular Action for a particular Player.
    /// </summary>
    [UnityEngine.AddComponentMenu("Rewired/Glyphs/Unity UI/Unity UI Player Controller Element Glyph")]
    public class UnityUIPlayerControllerElementGlyph : UnityUIPlayerControllerElementGlyphBase {

        [UnityEngine.Tooltip("The Player id.")]
        [UnityEngine.SerializeField]
        private int _playerId;

        [UnityEngine.Tooltip("The Action name.")]
        [UnityEngine.SerializeField]
        private string _actionName;

        [UnityEngine.Tooltip("The second Action name for 2D Actions. (Optional)")]
        [UnityEngine.SerializeField]
        private string _actionName2;

        [System.NonSerialized]
        private int _actionId = -1;
        [System.NonSerialized]
        private bool _actionIdCached = false;
        [System.NonSerialized]
        private int _actionId2 = -1;
        [System.NonSerialized]
        private bool _actionId2Cached = false;

        /// <summary>
        /// The Player id.
        /// </summary>
        public override int playerId { get { return _playerId; } set { _playerId = value; } }

        /// <summary>
        /// The Action id.
        /// </summary>
        public override int actionId {
            get {
                if (!_actionIdCached) CacheActionId();
                return _actionId;
            }
            set {
                if (!ReInput.isReady) return;
                if (value >= 0) {
                    var action = ReInput.mapping.GetAction(value);
                    if (action == null) {
                        UnityEngine.Debug.LogError("Invalid Action id: " + value);
                        return;
                    }
                    _actionName = action.name;
                } else {
                    _actionName = string.Empty;
                }
                CacheActionId();
            }
        }

        /// <summary>
        /// The second Action id for 2D Actions. (Optional)
        /// </summary>
        public override int actionId2 {
            get {
                if (!_actionId2Cached) CacheActionId2();
                return _actionId2;
            }
            set {
                if (!ReInput.isReady) return;
                if (value >= 0) {
                    var action = ReInput.mapping.GetAction(value);
                    if (action == null) {
                        UnityEngine.Debug.LogError("Invalid Action id 2: " + value);
                        return;
                    }
                    _actionName2 = action.name;
                } else {
                    _actionName2 = string.Empty;
                }
                CacheActionId2();
            }
        }

        /// <summary>
        /// The Action name.
        /// </summary>
        public string actionName {
            get {
                return _actionName;
            }
            set {
                if (ReInput.isReady) {
                    if (!string.IsNullOrEmpty(value)) {
                        var action = ReInput.mapping.GetAction(value);
                        if (action == null) {
                            UnityEngine.Debug.LogError("Invalid Action Name: " + value);
                            return;
                        }
                        value = action.name;
                    }
                }
                _actionName = value;
                CacheActionId();
            }
        }

        /// <summary>
        /// The second Action name for 2D Actions. (Optional)
        /// </summary>
        public string actionName2 {
            get {
                return _actionName2;
            }
            set {
                if (ReInput.isReady) {
                    if (!string.IsNullOrEmpty(value)) {
                        var action = ReInput.mapping.GetAction(value);
                        if (action == null) {
                            UnityEngine.Debug.LogError("Invalid Action Name 2: " + value);
                            return;
                        }
                        value = action.name;
                    }
                }
                _actionName2 = value;
                CacheActionId2();
            }
        }

        private void CacheActionId() {
            if (!ReInput.isReady) return;
            var action = ReInput.mapping.GetAction(_actionName);
            _actionId = action != null ? action.id : -1;
            _actionIdCached = true;
        }

        private void CacheActionId2() {
            if (!ReInput.isReady) return;
            var action = ReInput.mapping.GetAction(_actionName2);
            _actionId2 = action != null ? action.id : -1;
            _actionId2Cached = true;
        }

#if UNITY_EDITOR

        private Rewired.Utils.Classes.Data.InspectorValue<string> _inspector_actionName = new Rewired.Utils.Classes.Data.InspectorValue<string>();
        private Rewired.Utils.Classes.Data.InspectorValue<string> _inspector_actionName2 = new Rewired.Utils.Classes.Data.InspectorValue<string>();

        protected override void CheckInspectorValues(ref System.Action actions) {
            base.CheckInspectorValues(ref actions);
            if (_inspector_actionName.SetIfChanged(_actionName)) {
                actions += () => actionName = _actionName;
            }
            if (_inspector_actionName2.SetIfChanged(_actionName2)) {
                actions += () => actionName2 = _actionName2;
            }
        }
#endif
    }
}
