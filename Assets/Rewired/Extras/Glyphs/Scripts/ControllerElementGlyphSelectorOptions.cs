// Copyright (c) 2024 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#pragma warning disable 0649

namespace Rewired.Glyphs {
    using Rewired;

    /// <summary>
    /// Options for Controller Element Glyph selection.
    /// </summary>
    [System.Serializable]
    public class ControllerElementGlyphSelectorOptions {

        private static readonly ControllerElementType[] s_defaultControllerElementTypeOrder = new ControllerElementType[] {
            ControllerElementType.Axis,
            ControllerElementType.Button,
        };

        [UnityEngine.Tooltip("Determines if the Player's last active controller is used for glyph selection.")]
        [UnityEngine.SerializeField]
        private bool _useLastActiveController = true;

        [UnityEngine.Tooltip(
            "If enabled, results will be returned only for the first controller found that has at least one matching binding. " +
            "This only has an effect on the result list returned when doing manual result selection or when selecting a result index greater than 0." +
            "This prevents results from being returned from multiple different devices, for example, when trying to get the second result " +
            "from a single controller, excluding any other controllers such as the default controller. " +
            "Note that Keyboard and Mouse are considered a single controller for the purposes of the glyph system."
        )]
        [UnityEngine.SerializeField]
        private bool _useFirstControllerResults;

        [UnityEngine.Tooltip(
            "Controller type priority. " +
            "First in list corresponds to highest priority. " +
            "This determines which controller types take precedence when displaying glyphs. " +
            "If use last active controller is enabled, the active controller will always take priority, " + 
            "however, if there is no last active controller, selection will fall back based on this priority. " +
            "In addition, keyboard and mouse are treated as a single controller for the purposes of glyph handling, " +
            "so to prioritze keyboard over mouse or vice versa, the one that is lower in the list will take precedence."
        )]
        [UnityEngine.SerializeField]
        private ControllerType[] _controllerTypeOrder = new ControllerType[] {
            ControllerType.Joystick,
            ControllerType.Custom,
            ControllerType.Mouse,
            ControllerType.Keyboard
        };

        [UnityEngine.Tooltip(
            "Controller element type priority. " +
            "First in list corresponds to highest priority. " +
            "This determines which controller element types take precedence when displaying glyphs."
        )]
        [UnityEngine.SerializeField]
        private ControllerElementType[] _controllerElementTypeOrder = (ControllerElementType[])s_defaultControllerElementTypeOrder.Clone();

        [UnityEngine.Tooltip(
            "If enabled, the default controllers will be used if no matching mappings are found in the Player for other controllers. " +
            "The purpose of this is to allow glyphs to be displayed for a controller that is not connected. " +
            "Controllers will be evaluated in the order in which they appear in the list."
        )]
        [UnityEngine.SerializeField]
        private bool _useDefaultControllers;

        [UnityEngine.Tooltip(
            "Determines which controller will be used if no matching mappings are found in the Player for other controllers. " +
            "The purpose of this is to allow glyphs to be displayed for a controller that is not connected. " +
            "Use Default Controllers must be enabled for this to have any effect. " +
            "Controllers will be evaluated in the order in which they appear in the list.\n\n" +
            "For recognized controllers, set only the Controller Type and the Hardware Type Guid, do not specify a Hardware Identifier which is only useful " +
            "for unrecognized controllers and differs based on the platform and input source in use. The Hardware Type Guid of recognized controllers can " +
            "be found in the Hardware Joystick Map controller definition located at Rewired/Internal/Data/Controllers/HardwareMaps/Joysticks/ or in the " +
            "exported controllers CSV file which can be found in the glyphs documentation."
        )]
        [UnityEngine.SerializeField]
        private System.Collections.Generic.List<ControllerSelector> _defaultControllers;

        [System.NonSerialized]
        private System.Predicate<Rewired.ActionElementMap> _isActionElementMapAllowedHandler;

        /// <summary>
        /// Determines if the Player's last active controller is used for glyph selection.
        /// </summary>
        public bool useLastActiveController {
            get {
                return _useLastActiveController;
            }
             set {
                _useLastActiveController = value;
            }
        }

        /// <summary>
        /// If true, results will be returned only for the first controller found that has at least one matching binding.
        /// This only has an effect on the result list returned when doing manual result selection or when selecting a result index greater than 0.
        /// This prevents results from being returned from multiple different devices, for example, when trying to get the second result
        /// from a single controller, excluding any other controllers such as the default controller.
        /// Note that Keyboard and Mouse are considered a single controller for the purposes of the glyph system.
        /// </summary>
        public bool useFirstControllerResults {
            get {
                return _useFirstControllerResults;
            }
            set {
                _useFirstControllerResults = value;
            }
        }

        /// <summary>
        /// Controller type priority.
        /// First in list corresponds to highest priority.
        /// This determines which controller types take precedence when displaying glyphs.
        /// If use last active controller is enabled, the active controller will always take priority,
        /// however, if there is no last active controller, selection will fall back based on this priority.
        /// In addition, keyboard and mouse are treated as a single controller for the purposes of glyph handling,
        /// so to prioritze keyboard over mouse or vice versa, the one that is lower in the list will take precedence.
        /// </summary>
        public ControllerType[] controllerTypeOrder {
            get {
                return _controllerTypeOrder;
            }
            set {
                _controllerTypeOrder = value;
            }
        }

        /// <summary>
        /// "Controller element type priority.
        /// First in list corresponds to highest priority.
        /// This determines which controller element types take precedence when displaying glyphs.
        /// </summary>
        public ControllerElementType[] controllerElementTypeOrder {
            get {
                return _controllerElementTypeOrder;
            }
            set {
                if (value == null || value.Length == 0) value = (ControllerElementType[])s_defaultControllerElementTypeOrder.Clone();
                _controllerElementTypeOrder = value;
            }
        }

        /// <summary>
        /// If true, the default controllers will be used if no matching mappings are found in the Player for other controllers.
        /// The purpose of this is to allow glyphs to be displayed for a controller that is not connected.
        /// </summary>
        public bool useDefaultControllers {
            get {
                return _useDefaultControllers;
            }
            set {
                _useDefaultControllers = value;
            }
        }

        /// <summary>
        /// Determines which controller will be used if no matching mappings are found in the Player for other controllers.
        /// The purpose of this is to allow glyphs to be displayed for a controller that is not connected.
        /// Controllers will be evaluated in the order in which they appear in the list.
        // For recognized controllers, set only the Controller Type and the Hardware Type Guid, do not specify a Hardware Identifier which is only useful
        // for unrecognized controllers and differs based on the platform and input source in use. The Hardware Type Guid of recognized controllers can
        /// be found in the Hardware Joystick Map controller definition located at Rewired/Internal/Data/Controllers/HardwareMaps/Joysticks/ or in the
        /// exported controllers CSV file which can be found in the glyphs documentation.
        /// <see cref="useDefaultControllers"/> must be true for this to have any effect.
        /// </summary>
        public System.Collections.Generic.List<ControllerSelector> defaultControllers {
            get {
                return _defaultControllers;
            }
            set {
                _defaultControllers = value;
            }
        }

        /// <summary>
        /// Allows you to filter which Action Element Maps are displayed.
        /// When searching for mappings in the Player, the handler will be invoked for each Action Element Map found.
        /// This allows you to, for example, allow only Action Element Maps belonging to a Controller Map in particular Map Category.
        /// <see cref="Rewired.ActionElementMap"/> for properties which can be used as filtering criteria.
        /// IMPORTANT: By setting this value, you are taking over the responsibility for filtering mappings results entirely. The default
        /// filter removes disabled Action Element Maps and mappings in disabled Controller Maps. Disabled mappings are no longer removed
        /// when this handler is overriden with your own, so you must check the enabled states if you want to exclude these mappings.
        public System.Predicate<Rewired.ActionElementMap> isActionElementMapAllowedHandler {
            get {
                return _isActionElementMapAllowedHandler;
            }
            set {
                _isActionElementMapAllowedHandler = value;
            }
        }

        /// <summary>
        /// Gets the controller type priority for the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="controllerType">The result.</param>
        /// <returns>True if the controller type was found, false if not.</returns>
        public virtual bool TryGetControllerTypeOrder(int index, out ControllerType controllerType) {
            if ((uint)index >= (uint)_controllerTypeOrder.Length) {
                controllerType = ControllerType.Keyboard;
                return false;
            }
            controllerType = _controllerTypeOrder[index];
            return true;
        }

        // Static

        private static ControllerElementGlyphSelectorOptions s_defaultOptions;
        /// <summary>
        /// The default options.
        /// Set this to override the default selector options.
        /// </summary>
        public static ControllerElementGlyphSelectorOptions defaultOptions {
            get {
                return s_defaultOptions != null ? s_defaultOptions : s_defaultOptions = new ControllerElementGlyphSelectorOptions();
            }
            set {
                s_defaultOptions = value;
            }
        }

        // Classes

        /// <summary>
        /// Provides information necessary to obtain glyhps for controllers not currently connected.
        /// </summary>
        [System.Serializable]
        public struct ControllerSelector {

            [UnityEngine.Tooltip(
                "The controller type of the Controller."
            )]
            [UnityEngine.SerializeField]
            private ControllerType _controllerType;

            [UnityEngine.Tooltip(
                "The hardware type GUID of the Controller. This value is used to identify recognized Controllers."
            )]
            [UnityEngine.SerializeField]
            private string _hardwareTypeGuid;

            [UnityEngine.Tooltip(
                "The hardware identifier of the Controller. " +
                "This is used primarily by Unknown Controllers to identify the controller using various information gathered from the controller. " +
                "This value varies depending on the platform, input source in use, and the device."
            )]
            [UnityEngine.SerializeField]
            private string _hardwareIdentifier;

            [UnityEngine.Tooltip(
                "The list of Controller Map selectors. This provides necessary information about the Controller Maps to load."
            )]
            [UnityEngine.SerializeField]
            private System.Collections.Generic.List<ControllerMapSelector> _controllerMapSelectors;

            [System.NonSerialized]
            private bool _isHardwareTypeGuidCached;

            [System.NonSerialized]
            private System.Guid _hardwareTypeGuidCache;

            /// <summary>
            /// The controller type of the Controller.
            /// Get this value from <see cref="Rewired.Controller.type"/>.
            /// </summary>
            public ControllerType controllerType { get { return _controllerType; } set { _controllerType = value; } }
            /// <summary>
            /// The hardware type GUID of the Controller.
            /// Get this value from <see cref="Rewired.Controller.hardwareTypeGuid"/>.
            /// This value is used to identify recognized Controllers.
            /// </summary>
            public System.Guid hardwareTypeGuid {
                get {
                    if (_isHardwareTypeGuidCached) return _hardwareTypeGuidCache;
                    UpdateHardwareTypeGuidCache();
                    return _hardwareTypeGuidCache;
                }
                set {
                    _hardwareTypeGuid = value.ToString();
                    UpdateHardwareTypeGuidCache();
                }
            }
            /// <summary>
            /// The hardware identifier of the Controller.
            /// Get this value from <see cref="Rewired.Controller.hardwareIdentifier"/>.
            /// This is used primarily by Unknown Controllers to identify the controller using various information gathered from the controller.
            /// This value varies depending on the platform, input source in use, and the device.
            /// </summary>
            public string hardwareIdentifier { get { return _hardwareIdentifier; } set { _hardwareIdentifier = value; } }
            /// <summary>
            /// The list of Controller Map selectors.
            /// This provides necessary information about the Controller Maps to load.
            /// </summary>
            public System.Collections.Generic.List<ControllerMapSelector> controllerMapSelectors { get { return _controllerMapSelectors; } set { _controllerMapSelectors = value; } }

            private void UpdateHardwareTypeGuidCache() {
                try {
                    _hardwareTypeGuidCache = new System.Guid(_hardwareTypeGuid);
                } catch {
                }
                _isHardwareTypeGuidCached = true;
            }
        }

        /// <summary>
        /// Provides necessary information about the Controller Maps to load.
        /// </summary>
        [System.Serializable]
        public struct ControllerMapSelector {

            [UnityEngine.Tooltip("The Controller Map Category name.")]
            [UnityEngine.SerializeField]
            private string _mapCategoryName;

            [UnityEngine.Tooltip("The Controller Map Layout name.")]
            [UnityEngine.SerializeField]
            private string _layoutName;

            /// <summary>
            /// The Controller Map Category name.
            /// </summary>
            public string mapCategoryName { get { return _mapCategoryName; } set { _mapCategoryName = value; } }

            /// <summary>
            /// The Controller Map Layout name.
            /// </summary>
            public string layoutName { get { return _layoutName; } set { _layoutName = value; } }

            /// <summary>
            /// The Controller Map Category id.
            /// This property can only be accessed while Rewired is initialized.
            /// </summary>
            public int mapCategoryId {
                get {
                    if (!ReInput.isReady) {
                        UnityEngine.Debug.LogError(errorMessage_notInitialized_mapCategoryId);
                        return -1;
                    }
                    return ReInput.mapping.GetMapCategoryId(_mapCategoryName);
                }
                set {
                    if (!ReInput.isReady) {
                        UnityEngine.Debug.LogError(errorMessage_notInitialized_mapCategoryId);
                        return;
                    }
                    var result = ReInput.mapping.GetMapCategory(value);
                    if (result != null) {
                        _mapCategoryName = result.name;
                    } else {
                        UnityEngine.Debug.LogError(errorMessage_invalidMapCategoryId + value);
                        _mapCategoryName = string.Empty;
                    }
                }
            }

            /// <summary>
            /// Gets the Controller Map Layout id.
            /// This function can only be accessed while Rewired is initialized.
            /// </summary>
            /// <param name="controllerType">The controller type.</param>
            /// <returns>Layout id.</returns>
            public int GetLayoutId(ControllerType controllerType) {
                if (!ReInput.isReady) {
                    UnityEngine.Debug.LogError(errorMessage_notInitialized_layoutId);
                    return -1;
                }
                var result = ReInput.mapping.GetLayout(controllerType, _layoutName);
                if (result == null) return -1;
                return result.id;
            }

            /// <summary>
            /// Sets the Controller Map Layout id.
            /// This function can only be accessed while Rewired is initialized.
            /// </summary>
            /// <param name="controllerType">The controller type.</param>
            /// <param name="layoutId">The layout id.</param>
            public void SetLayoutId(ControllerType controllerType, int layoutId) {
                if (!ReInput.isReady) {
                    UnityEngine.Debug.LogError(errorMessage_notInitialized_layoutId);
                    return;
                }
                var result = ReInput.mapping.GetLayout(controllerType, layoutId);
                if (result != null) {
                    _layoutName = result.name;
                } else {
                    UnityEngine.Debug.LogError(errorMessage_invalidLayoutId + layoutId);
                    _layoutName = string.Empty;
                }
            }

            private const string errorMessage_notInitializedMemberAccess = "Rewired: Rewired must be initialized before accessing ";
            private static string errorMessage_notInitialized_mapCategoryId { get { return errorMessage_notInitializedMemberAccess + typeof(ControllerMapSelector).FullName + ".mapCategoryId."; } }
            private static string errorMessage_notInitialized_layoutId { get { return errorMessage_notInitializedMemberAccess + typeof(ControllerMapSelector).FullName + ".layoutId."; } }
            private static string errorMessage_invalidMapCategoryId { get { return "Rewired: Invalid map category id: "; } }
            private static string errorMessage_invalidLayoutId { get { return "Rewired: Invalid layout id: "; } }
        }
    }
}
