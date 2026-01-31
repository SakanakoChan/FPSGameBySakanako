// Copyright (c) 2024 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#pragma warning disable 0649

namespace Rewired.Glyphs.UnityUI {
    using System;
    using System.Collections.Generic;
    using Rewired;
    using Rewired.Glyphs;

    /// <summary>
    /// Base class for a Unity UI Player Controller Element Glyph component.
    /// Displays glyphs / text for a specific Action for a Player.
    /// </summary>
    public abstract class UnityUIPlayerControllerElementGlyphBase : UnityUIControllerElementGlyphBase {

        [UnityEngine.Tooltip("Optional reference to an object that defines options. If blank, the global default options will be used.")]
        [UnityEngine.SerializeField]
        private ControllerElementGlyphSelectorOptionsSOBase _options;

        [UnityEngine.Tooltip("The range of the Action for which to show glyphs / text. This determines whether to show the glyph for an axis-type Action " +
            "(ex: Move Horizontal), or the positive/negative pole of an Action (ex: Move Right). For button-type Actions, Full and Positive are equivalent. " +
            "This value has no effect when displaying two Actions.")]
        [UnityEngine.SerializeField]
        private AxisRange _actionRange = AxisRange.Full;

        [UnityEngine.Tooltip("Optional parent Transform of the first group of instantiated glyph / text objects. " +
            "For a single Action query, if an axis-type Action is bound to multiple elements, the glyphs bound to the negative pole of the Action will be instantiated under this Transform. " +
            "For a two Action query, if multiple glyphs are returned, the glyphs bound to the first Action will be instantiated under this Transform. " +
            "If a single glyph is returned, it will be instantiated under this Transform as well." +
            "This allows you to separate results by negative / positive binding or Action 1 / Action 2 in order to stack glyph groups horizontally or vertically, for example. " +
            "If an Action is only bound to one element, the glyph will be instantiated under this transform. " +
            "If blank, objects will be created as children of this object's Transform.")]
        [UnityEngine.SerializeField]
        private UnityEngine.Transform _group1;

        [UnityEngine.Tooltip("Optional parent Transform of the second group of instantiated glyph / text objects. " +
            "For a single Action query, if an axis-type Action is bound to multiple elements, the glyphs bound to the positive pole of the Action will be instantiated under this Transform. " +
            "For a two Action query, if multiple glyphs are returned, the glyphs bound to the second Action will be instantiated under this Transform " + 
            "unless there were no results found for the first Action, in which case they will be displayed under group1. " +
            "Otherwise, if a single glyph is returned, it will be instantiated under group1." +
            "This allows you to separate results by negative / positive binding or Action 1 / Action 2 in order to stack glyph groups horizontally or vertically, for example. " +
            "If an Action is only bound to one element, the glyph will be instantiated under group1 instead. " +
            "If blank, objects will be created as children of either group1 if set or the object's Transform.")]
        [UnityEngine.SerializeField]
        private UnityEngine.Transform _group2;

        [UnityEngine.Tooltip("The index of the result to return. This can be used to return, for example, the second matching glpyh(s) instead of the first found. " +
            "This will be ignored if you are using a custom result selector."
        )]
        [UnityEngine.SerializeField]
        private int _resultIndex;

        [UnityEngine.Tooltip("Determines the display order of split-axis and button glyphs for the first Action." +
            "When two glyphs for an axis-type action are displayed, this determines which pole is displayed first.")]
        [UnityEngine.SerializeField]
        private Pole _action1FirstPole = Pole.Negative;

        [UnityEngine.Tooltip("Determines the display order of split-axis and button glyphs for the second Action." +
            "When two glyphs for an axis-type action are displayed, this determines which pole is displayed first.")]
        [UnityEngine.SerializeField]
        private Pole _action2FirstPole = Pole.Negative;

        [NonSerialized]
        private List<Pair<ActionElementMapPair>> _temp2dResults;

        [NonSerialized]
        private List<ActionElementMap> _tempCombinedElementAems = new List<ActionElementMap>();

        [NonSerialized]
        private List<ActionElementMapPair> _tempResults;

        [NonSerialized]
        private readonly List<GlyphOrTextObject> _group1Objects = new List<GlyphOrTextObject>();

        [NonSerialized]
        private readonly List<GlyphOrTextObject> _group2Objects = new List<GlyphOrTextObject>();

        [NonSerialized]
        private ResultSelectionHandler _resultSelectionHandler;

        [NonSerialized]
        private Result2DSelectionHandler _result2dSelectionHandler;

        /// <summary>
        /// Optional reference to an object that defines options.
        /// If blank, the global default options will be used.
        /// </summary>
        public virtual ControllerElementGlyphSelectorOptionsSOBase options {
            get {
                return _options;
            }
            set {
                _options = value;
                RequireRebuild();
            }
        }

        /// <summary>
        /// The Player id.
        /// </summary>
        public abstract int playerId { get; set; }

        /// <summary>
        /// The Action id.
        /// </summary>
        public abstract int actionId { get; set; }

        /// <summary>
        /// The second Action id (for 2D Actions).
        /// </summary>
        public abstract int actionId2 { get; set; }

        /// <summary>
        /// The range of the Action for which to show glyphs / text. 
        /// This determines whether to show the glyph for an axis-type Action (ex: Move Horizontal), 
        /// or the positive/negative pole of an Action (ex: Move Right).
        /// For button-type Actions, Full and Positive are equivalent.
        /// This value has no effect when displaying two Actions.
        /// </summary>
        public virtual AxisRange actionRange {
            get {
                return _actionRange;
            }
            set {
                _actionRange = value;
            }
        }

        /// <summary>
        /// Optional parent Transform of the first group of instantiated glyph / text objects.
        /// For a single Action query, if an axis-type Action is bound to multiple elements, the glyphs bound to the negative pole of the Action will be instantiated under this Transform.
        /// For a two Action query, if multiple glyphs are returned, the glyphs bound to the first Action will be instantiated under this Transform.
        /// If a single glyph is returned, it will be instantiated under this Transform as well.
        /// This allows you to separate results by negative / positive binding or Action 1 / Action 2 in order to stack glyph groups horizontally or vertically, for example.
        /// If an Action is only bound to one element, the glyph will be instantiated under this transform.
        /// If blank, objects will be created as children of this object's Transform.
        /// </summary>
        public virtual UnityEngine.Transform group1 {
            get {
                return _group1;
            }
            set {
                _group1 = value;
                RequireRebuild();
            }
        }

        /// <summary>
        /// Optional parent Transform of the second group of instantiated glyph / text objects.
        /// For a single Action query, if an axis-type Action is bound to multiple elements, the glyphs bound to the positive pole of the Action will be instantiated under this Transform. 
        /// For a two Action query, if multiple glyphs are returned, the glyphs bound to the second Action will be instantiated under this Transform
        /// unless there were no results found for the first Action, in which case they will be displayed under group1.
        /// Otherwise, if a single glyph is returned, it will be instantiated under group1.
        /// This allows you to separate results by negative / positive binding or Action 1 / Action 2 in order to stack glyph groups horizontally or vertically, for example.
        /// If an Action is only bound to one element, the glyph will be instantiated under group1 instead.
        /// If blank,If blank, objects will be created as children of either group1 if set or the object's Transform.
        /// </summary>
        public virtual UnityEngine.Transform group2 {
            get {
                return _group2;
            }
            set {
                _group2 = value;
                RequireRebuild();
            }
        }

        /// <summary>
        /// The index of the result to return. This can be used to return, for example, the second matching glpyh(s) instead of the first found.
        /// This will be ignored if you are using a custom result selector.
        /// </summary>
        public int resultIndex {
            get {
                return _resultIndex;
            }
            set {
                if (value < 0) value = 0;
                _resultIndex = value;
            }
        }

        /// <summary>
        /// Determines the display order of split-axis and button glyphs for the first Action.
        /// When two glyphs for a axis-type action are displayed, this determines which pole is displayed first.
        /// </summary>
        public Pole action1FirstPole { get { return _action1FirstPole; } set { _action1FirstPole = value; } }

        /// <summary>
        /// Determines the display order of split-axis and button glyphs for the second Action.
        /// When two glyphs for an axis-type action are displayed, this determines which pole is displayed first.
        /// </summary>
        public Pole action2FirstPole { get { return _action2FirstPole; } set { _action2FirstPole = value; } }

        /// <summary>
        /// Allows you to evaluate all the results and decide which to display.
        /// This only works for single Actions. If combining two Actions, use <see cref="result2dSelectionHandler"/> instead.
        /// After searching for mappings in the Player and collecting all possible results, the handler will be invoked.
        /// This can be used, for example, to display the second result found instead of the first.
        /// It could also be used to choose results from any user-defined criteria such as controller type, controller map cateogry, etc.
        /// <see cref="Rewired.ActionElementMap"/> for properties which can be used as selection criteria.
        /// 
        /// The list contains all results from the search of Action Element Maps given a particular Action Range.
        /// Each result can contain either one or two Action Element Maps.
        /// The number of Action Element Maps returned depends on the Action Range used for the search and the bindings found.
        /// For a full Action Range search, if a full-axis binding is found, it will be returned in the <see cref="a"/> field.
        /// If negative and positive split-axis bindings are found instead, at least one Action Element Map will be returned
        /// in the <see cref="a"/> (negative) or <see cref="b"/> (positive) field.
        /// For a positive/negative Action Range search, a single Action Element Map will be returned in the <see cref="a"/> field.
        /// </summary>
        public virtual ResultSelectionHandler resultSelectionHandler {
            get {
                return _resultSelectionHandler;
            }
            set {
                _resultSelectionHandler = value;
            }
        }

        /// <summary>
        /// Allows you to evaluate all the results and decide which to display for two Actions.
        /// This only works if combining two Actions. If using a single Action, use <see cref="resultSelectionHandler"/> instead.
        /// After searching for mappings in the Player and collecting all possible results, the handler will be invoked.
        /// This can be used, for example, to display the second result found instead of the first.
        /// It could also be used to choose results from any user-defined criteria such as controller type, controller map cateogry, etc.
        /// <see cref="Rewired.ActionElementMap"/> for properties which can be used as selection criteria.
        /// 
        /// The list contains all results from the search of Action Element Maps.
        /// Each result can contain one result for each Action.
        /// Each Action result can contain either one or two Action Element Maps.
        /// The number of Action Element Maps returned depends on the bindings found.
        /// If a full-axis binding is found, it will be returned in the <see cref="a"/> field.
        /// If negative and positive split-axis bindings are found instead, at least one Action Element Map will be returned
        /// in the <see cref="a"/> (negative) or <see cref="b"/> (positive) field.
        /// </summary>
        public virtual Result2DSelectionHandler result2dSelectionHandler {
            get {
                return _result2dSelectionHandler;
            }
            set {
                _result2dSelectionHandler = value;
            }
        }

        protected virtual bool isMousePrioritizedOverKeyboard {
            get {
                ControllerType controllerType;
                for (int i = 0; TryGetControllerTypeOrder(i, out controllerType); i++) {
                    if (controllerType == ControllerType.Mouse) return true;
                    if (controllerType == ControllerType.Keyboard) return false;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the controller type order at the index.
        /// Lower values are evaulated first.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="controllerType">Controller type</param>
        /// <returns>Controller type at index.</returns>
        protected virtual bool TryGetControllerTypeOrder(int index, out ControllerType controllerType) {
            return GetOptionsOrDefault().TryGetControllerTypeOrder(index, out controllerType);
        }

        protected override void Update() {
            base.Update();
            if (!ReInput.isReady) return;

            ActionElementMapPair result1 = new ActionElementMapPair();
            ActionElementMapPair result2 = new ActionElementMapPair();

            ControllerElementGlyphSelectorOptions options = GetOptionsOrDefault();

            bool result = false;
            bool isAction2d = actionId2 >= 0 && actionId2 != actionId;

            if (isAction2d) actionRange = AxisRange.Full; // require full axis for Action 2d

            bool hasSelectionHandler = isAction2d ? _result2dSelectionHandler != null : _resultSelectionHandler != null;

            if (hasSelectionHandler || _resultIndex > 0) { // manual selection

                if (isAction2d) {

                    if (_temp2dResults == null) _temp2dResults = new List<Pair<ActionElementMapPair>>();

                    result |= TryGetActionElementMaps(
                        playerId,
                        actionId,
                        actionId2,
                        options,
                        _resultIndex,
                        _result2dSelectionHandler,
                        _temp2dResults,
                        out result1,
                        out result2
                    );

                    _temp2dResults.Clear();

                } else {

                    if (_tempResults == null) _tempResults = new List<ActionElementMapPair>();

                    result |= TryGetActionElementMaps(
                        playerId,
                        actionId,
                        actionRange,
                        options,
                        _resultIndex,
                        _resultSelectionHandler,
                        _tempResults,
                        out result1
                    );

                    _tempResults.Clear();
                }

            } else { // default -- use first found

                if (isAction2d) {

                    result |= GlyphTools.TryGetActionElementMaps(
                        playerId,
                        actionId,
                        actionId2,
                        options,
                        null,
                        out result1,
                        out result2
                    );

                } else {

                    ActionElementMap aem1;
                    ActionElementMap aem2;

                    result |= GlyphTools.TryGetActionElementMaps(
                        playerId,
                        actionId,
                        actionRange,
                        options,
                        null,
                        out aem1,
                        out aem2
                    );

                    result1 = new ActionElementMapPair(aem1, aem2);
                }
            }

            if (!result) {
                Hide();
                return;
            }

            // Show bindings
            if (isAction2d && (result1.Count > 0 || result2.Count > 0)) { // action 2D Stick or full D-pad
                ShowAction2DBindings(result1, result2);
            } else if (result1.a != null && result1.b != null) { // two split axis bindings
                ShowSplitAxisBindings(result1.a, result1.b);
            } else if (result1.a != null) {
                ShowBinding(result1.a);
            } else if (result1.b != null) {
                ShowBinding(result1.b);
            }
        }

        private static bool TryGetActionElementMaps(
            int playerId,
            int actionId,
            AxisRange actionRange,
            ControllerElementGlyphSelectorOptions options,
            int resultIndex,
            ResultSelectionHandler resultSelectionHandler,
            List<ActionElementMapPair> tempResults,
            out ActionElementMapPair result
        ) {
            tempResults.Clear();

            GlyphTools.GetActionElementMaps(
                playerId,
                actionId,
                actionRange,
                options,
                null,
                tempResults
            );

            try {
                if (resultSelectionHandler != null) { // local handler takes priority
                    resultIndex = resultSelectionHandler(tempResults);
                } else if (resultIndex > 0) { // fall back to user-specified result index
                    // already set
                } else { // error
                    result = new ActionElementMapPair();
                    return false;
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogError("Rewired: An exception was thrown in resultSortingHandler callback. This exception was thrown by your code.\n" + ex);
                result = new ActionElementMapPair();
                return false;
            }

            if (resultIndex < 0 || resultIndex >= tempResults.Count) { // invalid result
                result = new ActionElementMapPair();
                return false;
            }

            result = tempResults[resultIndex];
            return tempResults[resultIndex].Count > 0;
        }

        private static bool TryGetActionElementMaps(
            int playerId,
            int actionId,
            int actionId2,
            ControllerElementGlyphSelectorOptions options,
            int resultIndex,
            Result2DSelectionHandler resultSelectionHandler,
            List<Pair<ActionElementMapPair>> tempResults,
            out ActionElementMapPair action1Result,
            out ActionElementMapPair action2Result
        ) {
            
            action1Result = new ActionElementMapPair();
            action2Result = new ActionElementMapPair();

            tempResults.Clear();

            GlyphTools.GetActionElementMaps(
                playerId,
                actionId,
                actionId2,
                options,
                null,
                tempResults
            );

            if (tempResults.Count == 0) goto End;
            
            try {
                if (resultSelectionHandler != null) { // local handler takes priority
                    resultIndex = resultSelectionHandler(tempResults);
                } else if (resultIndex > 0) { // fall back to user-specified result index
                    // already set
                } else { // error
                    goto End;
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogError("Rewired: An exception was thrown in resultSortingHandler callback. This exception was thrown by your code.\n" + ex);
                goto End;
            }

            if (resultIndex < 0 || resultIndex >= tempResults.Count) { // invalid result
                goto End;
            }

            action1Result = tempResults[resultIndex].a;
            action2Result = tempResults[resultIndex].b;

            End:

            // Clean up so as to not hold any ActionElementMap references in memory
            tempResults.Clear();

            return action1Result.Count > 0 || action2Result.Count > 0;
        }

        protected override void ClearObjects() {
            _group1Objects.Clear();
            _group2Objects.Clear();
            base.ClearObjects();
        }

        protected virtual bool ShowBinding(ActionElementMap actionElementMap) {
            if (actionElementMap == null) return false;

            // Display glyph or text for a single Action Element Map

            int count = ShowGlyphsOrText(actionElementMap, GetObjectGroupTransform(0), _group1Objects);

            EvaluateObjectVisibility();

            return count > 0;
        }

        protected virtual bool ShowSplitAxisBindings(ActionElementMap negativeAem, ActionElementMap positiveAem) {
            if (negativeAem == null && positiveAem == null) return false;

            // Display glyph or text for up to two Action Element Maps

            int count = 0;

            // Handle special combined glyphs (D-Pad Left + D-Pad Right = D-Pad Horizontal)
            if (negativeAem != null && positiveAem != null) {
                _tempCombinedElementAems.Clear();
                _tempCombinedElementAems.Add(negativeAem);
                _tempCombinedElementAems.Add(positiveAem);
                count = ShowGlyphsOrText(_tempCombinedElementAems, GetObjectGroupTransform(0), _group1Objects);
            }
            
            // Positive and negative bindings
            if (count == 0) {
                count += ShowGlyphsOrText(_action1FirstPole == Pole.Negative ? negativeAem : positiveAem, GetObjectGroupTransform(0), _group1Objects);
                count += ShowGlyphsOrText(_action1FirstPole == Pole.Negative ? positiveAem : negativeAem, GetObjectGroupTransform(1), _group2Objects);
            }

            EvaluateObjectVisibility();

            return count > 0;
        }

        protected virtual bool ShowAction2DBindings(ActionElementMapPair result1, ActionElementMapPair result2) {
            if (result1.Count == 0 && result2.Count == 0) return false;

            // Display glyph or text for up to four Action Element Maps

            int group1ObjectCount = 0;
            int group2ObjectCount = 0;
            int count;

            // Handle each result
            // 2D glyph: group 1
            // Glyphs for both Actions: group1 = Action1, group2 = Action2
            // Glyphs for only one Action: group1

            // Handle special combined 2d glyphs (D-Pad, Stick)
            _tempCombinedElementAems.Clear();
            if (result1.a != null) _tempCombinedElementAems.Add(result1.a);
            if (result1.b != null) _tempCombinedElementAems.Add(result1.b);
            if (result2.a != null) _tempCombinedElementAems.Add(result2.a);
            if (result2.b != null) _tempCombinedElementAems.Add(result2.b);
            count = ShowGlyphsOrText(_tempCombinedElementAems, GetObjectGroupTransform(0), _group1Objects, ref group1ObjectCount);
            
            if (count == 0) { // no 2D glyph, fall back to individual
                if (result1.Count > 0 && result2.Count > 0) {
                    // Try to display Action1 in group1
                    int newGroup1Count = ShowAction2dBindings_ShowResultBindings(0, _action1FirstPole, result1, ref group1ObjectCount);
                    if (newGroup1Count == 0) { // no results for Action1 so show Action2 in group1 instead
                        count += ShowAction2dBindings_ShowResultBindings(0, _action2FirstPole, result2, ref group1ObjectCount);
                    } else { // Action1 used group1, put Action2 in group2
                        count += ShowAction2dBindings_ShowResultBindings(1, _action2FirstPole, result2, ref group2ObjectCount);
                    }
                    count += newGroup1Count;
                } else if (result1.Count > 0) {
                    count += ShowAction2dBindings_ShowResultBindings(0, _action1FirstPole, result1, ref group1ObjectCount);
                } else if (result2.Count > 0) {
                    count += ShowAction2dBindings_ShowResultBindings(0, _action2FirstPole, result2, ref group1ObjectCount);
                }
            }

            EvaluateObjectVisibility();

            return count > 0;
        }

        private int ShowAction2dBindings_ShowResultBindings(int groupIndex, Pole poleOrder, ActionElementMapPair result, ref int groupObjectCount) {
            if (groupIndex > 1) throw new ArgumentOutOfRangeException("groupIndex");

            int count = 0;
            
            List<GlyphOrTextObject> groupObjects = groupIndex == 0 ? _group1Objects : _group2Objects;

            // Handle special combined 1d glyphs (D-Pad Left + D-Pad Right = D-Pad Horizontal)
            if (result.a != null && result.b != null) {
                _tempCombinedElementAems.Clear();
                _tempCombinedElementAems.Add(result.a);
                _tempCombinedElementAems.Add(result.b);
                int newCount = ShowGlyphsOrText(_tempCombinedElementAems, GetObjectGroupTransform(groupIndex), groupObjects, ref groupObjectCount);
                count += newCount;
                if (newCount > 0) {
                    return count;
                }
            }

            // Positive and negative bindings
            ActionElementMap aem1;
            ActionElementMap aem2;
            if (poleOrder == Pole.Negative) {
                aem1 = result.a;
                aem2 = result.b;
            } else {
                aem1 = result.b;
                aem2 = result.a;
            }

            if (aem1 != null) {
                count += ShowGlyphsOrText(aem1, GetObjectGroupTransform(groupIndex), groupObjects, ref groupObjectCount);
            }
            if (aem2 != null) {
                count += ShowGlyphsOrText(aem2, GetObjectGroupTransform(groupIndex), groupObjects, ref groupObjectCount);
            }

            return count;
        }

        protected override void EvaluateObjectVisibility() {
            base.EvaluateObjectVisibility();
            
            // Enable / disable groups
            var transform1 = GetObjectGroupTransform(0);
            var transform2 = GetObjectGroupTransform(1);

            if (transform1 == transform2) {
                EvaluateObjectVisibility(transform1);
            } else {
                EvaluateObjectVisibility(transform1, _group1Objects);
                EvaluateObjectVisibility(transform2, _group2Objects);
            }
        }

        protected virtual int ShowGlyphsOrText(IList<ActionElementMap> bindings, UnityEngine.Transform parent, List<GlyphOrTextObject> objects) {
            int currentUsedObjectCount = 0;
            return ShowGlyphsOrText(bindings, parent, objects, ref currentUsedObjectCount);
        }
        protected virtual int ShowGlyphsOrText(IList<ActionElementMap> bindings, UnityEngine.Transform parent, List<GlyphOrTextObject> objects, ref int currentUsedObjectCount) {
            if (bindings == null) return 0;
            if (currentUsedObjectCount < 0) currentUsedObjectCount = 0;

            // Show combined glyph or text for multiple bindings.
            // This does not support modifier keys.

            object glyph;
            string name;

            if (IsAllowed(AllowedTypes.Glyphs) && ActionElementMap.TryGetCombinedElementIdentifierGlyph(bindings, out glyph)) {
                if (!CreateObjectsAsNeeded(parent, objects, currentUsedObjectCount + 1)) return 0;
                objects[currentUsedObjectCount].ShowGlyph(glyph);
                currentUsedObjectCount += 1;
                return 1;
            } else if (IsAllowed(AllowedTypes.Text) && ActionElementMap.TryGetCombinedElementIdentifierName(bindings, out name)) {
                if (!CreateObjectsAsNeeded(parent, objects, currentUsedObjectCount + 1)) return 0;
                objects[currentUsedObjectCount].ShowText(name);
                currentUsedObjectCount += 1;
                return 1;
            }

            return 0;
        }

        protected override void Hide() {
            base.Hide();
            if (_group1 != null && _group1 != this.transform) _group1.gameObject.SetActive(false);
            if (_group2 != null && _group2 != this.transform) _group2.gameObject.SetActive(false);
        }

        protected virtual UnityEngine.Transform GetObjectGroupTransform(int groupIndex) {
            if ((uint)groupIndex > 1u) throw new ArgumentOutOfRangeException();
            switch (groupIndex) {
                case 0: return _group1 != null ? _group1 : this.transform;
                case 1:
                    if (_group1 == null) return this.transform; // ignore 2 if 1 not set
                    if (_group2 != null) return _group2;
                    if (_group1 != null) return _group1;
                    return this.transform;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the Controller Element Glyph Options if set, otherwise the default options.
        /// </summary>
        /// <returns>The Controller Element Glyph Options if set, otherwise the default options.</returns>
        protected virtual ControllerElementGlyphSelectorOptions GetOptionsOrDefault() {
            if (_options != null && _options.options == null) {
                UnityEngine.Debug.LogError("Rewired: Options missing on " + typeof(ControllerElementGlyphSelectorOptions).Name + ". Global default options will be used instead.");
                return ControllerElementGlyphSelectorOptions.defaultOptions;
            }
            return _options != null ? _options.options : ControllerElementGlyphSelectorOptions.defaultOptions;
        }


        /// <summary>
        /// Delegate for setting a custom result selection handler.
        /// </summary>
        /// <param name="results">The list of result candidates.</param>
        /// <returns>The index of the chosen result.</returns>
        public delegate int ResultSelectionHandler(IList<ActionElementMapPair> results);

        /// <summary>
        /// Delegate for setting a custom result selection handler for a 2D Action.
        /// </summary>
        /// <param name="results">The list of result candidates.</param>
        /// <returns>The index of the chosen result.</returns>
        public delegate int Result2DSelectionHandler(IList<Pair<ActionElementMapPair>> results);
    }
}
