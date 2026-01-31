// Copyright (c) 2024 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

namespace Rewired.Glyphs {
    using Rewired;
    using System.Collections.Generic;

    public static class GlyphTools {

        private static readonly ObjectPool<FastList<ActionElementMap>> aemFastListPool = new ObjectPool<FastList<ActionElementMap>>(() => new FastList<ActionElementMap>(16), x => x.Clear());
        private static readonly ObjectPool<List<ActionElementMapPair>> aemPairListPool = new ObjectPool<List<ActionElementMapPair>>(() => new List<ActionElementMapPair>(16), x => x.Clear());
        private static readonly ObjectPool<List<Pair<ActionElementMapPair>>> aemPair2dListPool = new ObjectPool<List<Pair<ActionElementMapPair>>>(() => new List<Pair<ActionElementMapPair>>(8), x => x.Clear());
        private static readonly ObjectPool<FastList<bool>> boolFastListPool = new ObjectPool<FastList<bool>>(() => new FastList<bool>(16), x => x.Clear());
        private static readonly ObjectPool<FastList<ControllerInfo>> controllerInfoFastListPool = new ObjectPool<FastList<ControllerInfo>>(() => new FastList<ControllerInfo>(8), x => x.Clear());

        #region Public

        // Gets the first result
        public static bool TryGetActionElementMaps(
            int playerId,
            int actionId,
            AxisRange actionRange,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            out ActionElementMap aemResult1,
            out ActionElementMap aemResult2
            ) {

            List<ActionElementMapPair> results = aemPairListPool.Get();

            if (GetActionElementMaps(playerId, actionId, actionRange, options, isAemAllowedHandlerOverride, results, 1) > 0) {
                aemResult1 = results[0].a;
                aemResult2 = results[0].b;
            } else {
                aemResult1 = null;
                aemResult2 = null;
            }

            aemPairListPool.Return(results);

            return aemResult1 != null || aemResult2 != null;
        }
        // Gets the first result
        // Legacy overload
        public static bool TryGetActionElementMaps(
            InputAction action,
            AxisRange actionRange,
            List<ActionElementMap> aems,
            out ActionElementMap aemResult1,
            out ActionElementMap aemResult2
            ) {
            return TryGetActionElementMaps(action, actionRange, aems, null, out aemResult1, out aemResult2);
        }
        // Gets the first result
        public static bool TryGetActionElementMaps(
            InputAction action,
            AxisRange actionRange,
            List<ActionElementMap> aems,
            ControllerElementGlyphSelectorOptions options,
            out ActionElementMap aemResult1,
            out ActionElementMap aemResult2
            ) {

            List<ActionElementMapPair> results = aemPairListPool.Get();
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();

            int resultsRemainingCount = 1;
            tempAems.ReplaceFrom(aems);

            if (GetActionElementMaps(action, actionRange, tempAems, false, options, results, ref resultsRemainingCount) > 0) {
                aemResult1 = results[0].a;
                aemResult2 = results[0].b;
            } else {
                aemResult1 = null;
                aemResult2 = null;
            }
            
            aemPairListPool.Return(results);
            aemFastListPool.Return(tempAems);

            return aemResult1 != null || aemResult2 != null;
        }
        // Gets the first result for 2D Action
        public static bool TryGetActionElementMaps(
            int playerId,
            int actionId,
            int actionId2,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            out ActionElementMapPair aemResult1,
            out ActionElementMapPair aemResult2
        ) {

            List<Pair<ActionElementMapPair>> results = aemPair2dListPool.Get();

            if (GetActionElementMaps(playerId, actionId, actionId2, options, isAemAllowedHandlerOverride, results, 1) > 0) {
                aemResult1 = results[0].a;
                aemResult2 = results[0].b;
            } else {
                aemResult1 = new ActionElementMapPair();
                aemResult2 = new ActionElementMapPair();
            }

            aemPair2dListPool.Return(results);

            return aemResult1.Count > 0 || aemResult2.Count > 0;
        }

        // Gets all results
        public static int GetActionElementMaps(
            int playerId,
            int actionId,
            AxisRange actionRange,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            List<ActionElementMapPair> results
            ) {

            int result = GetActionElementMaps(playerId, actionId, actionRange, options, isAemAllowedHandlerOverride, results, 0);
            return result;
        }

        // Gets all results for 2D Action
        public static int GetActionElementMaps(
            int playerId,
            int actionId,
            int actionId2,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            List<Pair<ActionElementMapPair>> results
        ) {
            return GetActionElementMaps(playerId, actionId, actionId2, options, isAemAllowedHandlerOverride, results, 0);
        }

        public static ActionElementMap FindFirstFullAxisBinding(List<ActionElementMap> actionElementMaps) {
            return FindFirstFullAxisBinding(actionElementMaps, null);
        }
        public static ActionElementMap FindFirstFullAxisBinding(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            List<ActionElementMapPair> results = aemPairListPool.Get();
            ActionElementMap r;
            int resultsRemainingCount = 1;
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            if (FindFullAxisBindingsOnly(tempAems, usedAems, results, ref resultsRemainingCount) > 0) {
                r = results[0].a;
            } else {
                r = null;
            }
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            aemPairListPool.Return(results);
            return r;
        }

        public static int FindFullAxisBindings(List<ActionElementMap> actionElementMaps, List<ActionElementMapPair> results) {
            return FindFullAxisBindings(actionElementMaps, null, results);
        }
        public static int FindFullAxisBindings(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, List<ActionElementMapPair> results) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = -1; // infinite
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            int result = FindFullAxisBindingsOnly(tempAems, usedAems, results, ref resultsRemainingCount);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return result;
        }

        public static ActionElementMap FindFirstBinding(List<ActionElementMap> actionElementMaps, AxisRange actionRange) {
            return FindFirstBinding(actionElementMaps, actionRange);
        }
        public static ActionElementMap FindFirstBinding(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, AxisRange actionRange) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            List<ActionElementMapPair> results = aemPairListPool.Get();
            ActionElementMap r;
            int resultsRemainingCount = 1;
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            if (FindBindings(tempAems, usedAems, actionRange, results, ref resultsRemainingCount) > 0) {
                r = results[0].a != null ? results[0].a : results[0].b;
            } else {
                r = null;
            }
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            aemPairListPool.Return(results);
            return r;
        }

        public static int FindBindings(List<ActionElementMap> actionElementMaps, AxisRange actionRange, List<ActionElementMapPair> results) {
            return FindBindings(actionElementMaps, actionRange, null, results);
        }
        public static int FindBindings(List<ActionElementMap> actionElementMaps, AxisRange actionRange, ControllerElementGlyphSelectorOptions options, List<ActionElementMapPair> results) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = -1; // infinite
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            int result = FindBindings(tempAems, usedAems, actionRange, results, ref resultsRemainingCount);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return result;
        }

        public static int FindSplitAxisBindingPairs(List<ActionElementMap> actionElementMaps, List<ActionElementMapPair> results) {
            return FindSplitAxisBindingPairs(actionElementMaps, null, results);
        }
        public static int FindSplitAxisBindingPairs(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, List<ActionElementMapPair> results) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = -1; // infinite
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            int result = FindSplitAxisBindingPairsOnly(tempAems, usedAems, results, ref resultsRemainingCount);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return result;
        }

        public static bool FindFirstSplitAxisBindingPair(List<ActionElementMap> actionElementMaps, out ActionElementMap negativeAem, out ActionElementMap positiveAem) {
            return FindFirstSplitAxisBindingPair(actionElementMaps, null, out negativeAem, out positiveAem);
        }
        public static bool FindFirstSplitAxisBindingPair(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, out ActionElementMap negativeAem, out ActionElementMap positiveAem) {
            List<ActionElementMapPair> results = aemPairListPool.Get();
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = 1;
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            if (FindSplitAxisBindingPairsOnly(tempAems, usedAems, results, ref resultsRemainingCount) > 0) {
                negativeAem = Get(results[0], Pole.Negative);
                positiveAem = Get(results[0], Pole.Positive);
            } else {
                negativeAem = null;
                positiveAem = null;
            }
            aemPairListPool.Return(results);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return negativeAem != null || positiveAem != null;
        }

        public static int FindButtonBindingPairs(List<ActionElementMap> actionElementMaps, List<ActionElementMapPair> results) {
            return FindSplitAxisBindingPairs(actionElementMaps, null, results);
        }
        public static int FindButtonBindingPairs(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, List<ActionElementMapPair> results) {
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = -1; // infinite
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            int result = FindButtonBindingPairsOnly(tempAems, usedAems, results, ref resultsRemainingCount);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return result;
        }

        public static bool FindFirstButtonBindingPair(List<ActionElementMap> actionElementMaps, out ActionElementMap negativeAem, out ActionElementMap positiveAem) {
            return FindFirstSplitAxisBindingPair(actionElementMaps, null, out negativeAem, out positiveAem);
        }
        public static bool FindFirstButtonBindingPair(List<ActionElementMap> actionElementMaps, ControllerElementGlyphSelectorOptions options, out ActionElementMap negativeAem, out ActionElementMap positiveAem) {
            List<ActionElementMapPair> results = aemPairListPool.Get();
            FastList<ActionElementMap> tempAems = aemFastListPool.Get();
            FastList<bool> usedAems = GetUsedPooledList(actionElementMaps.Count);
            int resultsRemainingCount = 1;
            tempAems.ReplaceFrom(actionElementMaps);
            if (options != null) SortByElementType(tempAems, options.controllerElementTypeOrder);
            if (FindButtonBindingPairsOnly(tempAems, usedAems, results, ref resultsRemainingCount) > 0) {
                negativeAem = Get(results[0], Pole.Negative);
                positiveAem = Get(results[0], Pole.Positive);
            } else {
                negativeAem = null;
                positiveAem = null;
            }
            aemPairListPool.Return(results);
            aemFastListPool.Return(tempAems);
            ReturnUsedPoolList(usedAems);
            return negativeAem != null || positiveAem != null;
        }

        public static bool IsMousePrioritizedOverKeyboard(ControllerElementGlyphSelectorOptions options) {
            if (options == null) return false;
            ControllerType controllerType;
            for (int i = 0; options.TryGetControllerTypeOrder(i, out controllerType); i++) {
                if (controllerType == ControllerType.Mouse) return true;
                if (controllerType == ControllerType.Keyboard) return false;
            }
            return false;
        }

        #endregion

        #region Private

        private static int GetActionElementMaps(
            int playerId,
            int actionId,
            AxisRange actionRange,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            List<ActionElementMapPair> results,
            int maxResultCount
            ) {

            if (!ReInput.isReady) return 0;
            if (options == null) return 0;
            if (results == null) return 0;
            if (maxResultCount < 0) maxResultCount = 0;

            InputAction action = ReInput.mapping.GetAction(actionId);
            if (action == null) return 0;

            Player player = ReInput.players.GetPlayer(playerId);
            if (player == null) return 0;

            ControllerElementType[] controllerElementTypeOrder = options.controllerElementTypeOrder;

            int origResultCount = results.Count;
            int resultsRemainingCount = maxResultCount > 0 ? maxResultCount : -1;

            bool useFirstControllerResults = options.useFirstControllerResults;

            // Get Player's last active controller
            Controller lastActiveController = player.controllers.GetLastActiveController();

            FastList<ActionElementMap> workingActionElementMaps = aemFastListPool.Get();
            FastList<ControllerInfo> usedControllers = controllerInfoFastListPool.Get(); // prevent checking the same controller multiple times to avoid redundant results
            try {
                // Get the isAllowed handler
                System.Predicate<ActionElementMap> isAemAllowedHandler = null;
                if (isAemAllowedHandlerOverride != null) { // override is preferred if set
                    isAemAllowedHandler = isAemAllowedHandlerOverride;
                } else if (options != null) { // use the handler supplied in options
                    isAemAllowedHandler = options.isActionElementMapAllowedHandler;
                }
                // Fall back to default handler if none set
                if (isAemAllowedHandler == null) isAemAllowedHandler = defaultGetElementMapsWithActionisAllowedHandler;

                // Get all action element maps for the Action

                if (options.useLastActiveController && lastActiveController != null) {

                    Controller otherController = null;

                    // Treat keyboard and mouse as a single controller because most people want it to work this way.
                    // Override last active controller with either the keyboard or mouse, whichever is higher priority.
                    // This avoids glyphs switching as keyboard and mouse are used alternately.
                    if (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Mouse) {
                        if (IsMousePrioritizedOverKeyboard(options)) {
                            if (ReInput.controllers.Mouse.enabled && player.controllers.hasMouse) {
                                lastActiveController = ReInput.controllers.Mouse;
                                otherController = ReInput.controllers.Keyboard;
                            }
                        } else {
                            if (ReInput.controllers.Keyboard.enabled && player.controllers.hasKeyboard) {
                                lastActiveController = ReInput.controllers.Keyboard;
                                otherController = ReInput.controllers.Mouse;
                            }
                        }
                    }

                    // Prioritize last active controller
                    if (!Contains(usedControllers, lastActiveController.type, lastActiveController.id)) {
                        if (GetElementMapsWithAction(player, lastActiveController.type, lastActiveController.id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                            if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                            }
                        }
                        usedControllers.Add(new ControllerInfo(lastActiveController.type, lastActiveController.id));
                    }

                    // Fallback to secondary for keyboard/mouse
                    if (otherController != null) {
                        if (!Contains(usedControllers, otherController.type, otherController.id)) {
                            if (GetElementMapsWithAction(player, otherController.type, otherController.id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                    if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                }
                            }
                            usedControllers.Add(new ControllerInfo(otherController.type, otherController.id));
                        }
                    }

                    if (useFirstControllerResults && results.Count - origResultCount > 0) return results.Count - origResultCount;

                    // Fall back to last-known active controller type without the controller id.
                    // This allows falling back to other Joysticks if binding is not found in the active one.
                    {
                        ControllerType controllerType = lastActiveController.type;

                        switch (controllerType) {
                            case ControllerType.Joystick: {
                                    int id;
                                    for (int i = 0; i < player.controllers.joystickCount; i++) {
                                        id = player.controllers.Joysticks[i].id;
                                        if (Contains(usedControllers, controllerType, id)) continue;
                                        if (GetElementMapsWithAction(player, controllerType, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                            if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                            }
                                        }
                                        usedControllers.Add(new ControllerInfo(controllerType, id));
                                    }
                                }
                                break;
                            case ControllerType.Custom: {
                                    int id;
                                    for (int i = 0; i < player.controllers.customControllerCount; i++) {
                                        id = player.controllers.CustomControllers[i].id;
                                        if (Contains(usedControllers, controllerType, id)) continue;
                                        if (GetElementMapsWithAction(player, controllerType, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                            if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                            }
                                        }
                                        usedControllers.Add(new ControllerInfo(controllerType, id));
                                    }
                                }
                                break;
                                // Keyboard and Mouse would have been handled above
                        }
                    }
                }

                // Fall back to other controller types in order of priority
                {
                    const int flag_joysticks = 1;
                    const int flag_keyboard = 2;
                    const int flag_mouse = 4;
                    const int flag_custom = 8;

                    ControllerType controllerType;

                    // Fall back to other controller types in order of priority
                    // Go through all controllers individually and return when any valid results found

                    int remainingControllerTypeFlags =
                        flag_joysticks |
                        flag_keyboard |
                        flag_mouse |
                        flag_custom;

                    ControllerType[] controllerTypes = options.controllerTypeOrder;

                    int currentIndex = 0;

                    // Search controllers in order of type
                    while (true) {
                        if (remainingControllerTypeFlags == 0) break;

                        if (currentIndex < controllerTypes.Length) { // process the sorted list first
                            controllerType = controllerTypes[currentIndex];

                        } else { // process any remaining types last

                            // User may have excluded some controller types from controller type order list.
                            // This list should not be used as an inclusion/exclusion list, only ordering.
                            // Process any missing controller types in a fixed order.
                            if ((remainingControllerTypeFlags & flag_joysticks) != 0) controllerType = ControllerType.Joystick;
                            else if ((remainingControllerTypeFlags & flag_mouse) != 0) controllerType = ControllerType.Mouse;
                            else if ((remainingControllerTypeFlags & flag_keyboard) != 0) controllerType = ControllerType.Keyboard;
                            else if ((remainingControllerTypeFlags & flag_custom) != 0) controllerType = ControllerType.Custom;
                            else throw new System.NotImplementedException();
                        }
                        {
                            int id;

                            switch (controllerType) {
                                case ControllerType.Joystick:
                                    if ((remainingControllerTypeFlags & flag_joysticks) != 0) {
                                        for (int j = 0; j < player.controllers.joystickCount; j++) {
                                            id = player.controllers.Joysticks[j].id;
                                            if (Contains(usedControllers, controllerType, id)) continue;
                                            if (GetElementMapsWithAction(player, controllerType, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                                if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                    if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                }
                                            }
                                            usedControllers.Add(new ControllerInfo(controllerType, id));
                                        }
                                        remainingControllerTypeFlags &= ~flag_joysticks;
                                    }
                                    break;
                                case ControllerType.Custom:
                                    if ((remainingControllerTypeFlags & flag_custom) != 0) {
                                        for (int j = 0; j < player.controllers.customControllerCount; j++) {
                                            id = player.controllers.CustomControllers[j].id;
                                            if (Contains(usedControllers, controllerType, id)) continue;
                                            if (GetElementMapsWithAction(player, controllerType, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                                if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                    if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                }
                                            }
                                            usedControllers.Add(new ControllerInfo(controllerType, id));
                                        }
                                        remainingControllerTypeFlags &= ~flag_custom;
                                    }
                                    break;
                                case ControllerType.Mouse:
                                case ControllerType.Keyboard: {

                                        bool done = false;
                                        bool doBoth = useFirstControllerResults; // get results for both if use first controller so KB/Mouse are treated as one controller

                                        if ((controllerType == ControllerType.Mouse || doBoth) && (remainingControllerTypeFlags & flag_mouse) != 0) {
                                            if (player.controllers.hasMouse) {
                                                id = ReInput.controllers.Mouse.id;
                                                if (!Contains(usedControllers, ControllerType.Mouse, id)) {
                                                    if (GetElementMapsWithAction(player, ControllerType.Mouse, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                                        if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                            if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                                            done = true;
                                                        }
                                                    }
                                                    usedControllers.Add(new ControllerInfo(ControllerType.Mouse, id));
                                                }
                                            }
                                            remainingControllerTypeFlags &= ~flag_mouse;
                                        }

                                        if ((controllerType == ControllerType.Keyboard || doBoth) && (remainingControllerTypeFlags & flag_keyboard) != 0) {
                                            if (player.controllers.hasKeyboard) {
                                                id = ReInput.controllers.Keyboard.id;
                                                if (!Contains(usedControllers, ControllerType.Keyboard, id)) {
                                                    if (GetElementMapsWithAction(player, ControllerType.Keyboard, id, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                                        if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                                            if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                                            done = true;
                                                        }
                                                    }
                                                    usedControllers.Add(new ControllerInfo(ControllerType.Keyboard, id));
                                                }
                                            }
                                            remainingControllerTypeFlags &= ~flag_keyboard;
                                        }

                                        if (useFirstControllerResults && done) {
                                            return results.Count - origResultCount;
                                        }

                                        break;
                                    }
                            }
                        }
                        currentIndex += 1;
                    }
                }

                // Fall back to default controllers if set
                if (options.useDefaultControllers) {
                    List<ControllerElementGlyphSelectorOptions.ControllerSelector> defaultControllers = options.defaultControllers;
                    int defaultControllerCount = defaultControllers != null ? defaultControllers.Count : 0;
                    ControllerIdentifier identifier;
                    ControllerElementGlyphSelectorOptions.ControllerSelector defaultController;
                    List<ControllerElementGlyphSelectorOptions.ControllerMapSelector> maps;
                    ControllerMap controllerMap;
                    int mapCount;
                    for (int controllerIndex = 0; controllerIndex < defaultControllerCount; controllerIndex++) {
                        defaultController = defaultControllers[controllerIndex];
                        maps = defaultController.controllerMapSelectors;
                        if (maps != null) {
                            mapCount = maps.Count;
                            identifier = ControllerIdentifier.Blank;
                            identifier.controllerType = defaultController.controllerType;
                            identifier.hardwareTypeGuid = defaultController.hardwareTypeGuid;
                            identifier.hardwareIdentifier = defaultController.hardwareIdentifier;
                            for (int i = 0; i < mapCount; i++) {
                                controllerMap = DefaultControllerMapCache.instance.GetControllerMap(player.id, identifier, maps[i].mapCategoryName, maps[i].layoutName);
                                if (controllerMap != null) {
                                    controllerMap.enabled = true; // loaded maps start disabled so enable it
                                    if (GetElementMapsWithAction(controllerMap, actionId, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps) > 0) {
                                        if (GetActionElementMaps(action, actionRange, workingActionElementMaps, true, options, results, ref resultsRemainingCount) > 0) {
                                            if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                        }
                                    }
                                }
                            }
                        }
                        if (useFirstControllerResults && results.Count - origResultCount > 0) return results.Count - origResultCount;
                    }
                }

                return results.Count - origResultCount;

            } finally {

                // Clean up memory
                aemFastListPool.Return(workingActionElementMaps);
                controllerInfoFastListPool.Return(usedControllers);
            }
        }
        private static int GetActionElementMaps(
            InputAction action,
            AxisRange actionRange,
            FastList<ActionElementMap> aems,
            bool isSorted,
            ControllerElementGlyphSelectorOptions options,
            List<ActionElementMapPair> results,
            ref int resultsRemainingCount
            ) {

            if (aems == null || results == null) throw new System.ArgumentNullException();
            if (resultsRemainingCount == 0) return 0;

            FastList<bool> usedAems = GetUsedPooledList(aems.Count);

            int origResultCount = results.Count;

            // An axis-type Action may be bound to multiple buttons / keys (hotzontal = dpad left, dpad right),
            // so this must be taken into account. Prioritize full-axis bindings. If none found, try to find binding pairs.
            bool isAxisAction = action.type == InputActionType.Axis;

            // Sort AEMs by controller element type
            if (!isSorted && options != null) {
                SortByElementType(aems, options.controllerElementTypeOrder);
            }

            // Find first valid type, default to axis
            ControllerElementType controllerElementTypePriority = ControllerElementType.Axis;
            if(options != null) {
                var order = options.controllerElementTypeOrder;
                for (int i = 0; i < order.Length; i++) {
                    if (order[i] == ControllerElementType.Axis) {
                        controllerElementTypePriority = ControllerElementType.Axis;
                        break;
                    }
                    if (order[i] == ControllerElementType.Button) {
                        controllerElementTypePriority = ControllerElementType.Button;
                        break;
                    }
                }
            }

            // For axis-type Actions, must be able to support displaying split axis bindings to the Action (positive / negative).
            if (isAxisAction) {

                if (actionRange == AxisRange.Full) {

                    if (controllerElementTypePriority == ControllerElementType.Button) {
                        FindButtonBindingPairsOnly(aems, usedAems, results, ref resultsRemainingCount);
                        if (resultsRemainingCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                    }

                    // Try to get full binding first
                    FindFullAxisBindingsOnly(aems, usedAems, results, ref resultsRemainingCount);
                    if (resultsRemainingCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;

                    // Fall back to split axis binding pairs
                    FindSplitAxisBindingPairsOnly(aems, usedAems, results, ref resultsRemainingCount);
                    if (resultsRemainingCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;

                    if (controllerElementTypePriority != ControllerElementType.Button) {
                        FindButtonBindingPairsOnly(aems, usedAems, results, ref resultsRemainingCount);
                        if (resultsRemainingCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                    }

                    // Get mixed split axis and button binding pairs and anything leftover
                    FindSplitAxisAndButtonBindingPairsAndRemaining(aems, usedAems, results, ref resultsRemainingCount);
                    if (resultsRemainingCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;

                } else {
                    FindBindings(aems, usedAems, actionRange, results, ref resultsRemainingCount);
                }

            } else { // button type Action
                FindBindings(aems, usedAems, actionRange, results, ref resultsRemainingCount);
            }

            boolFastListPool.Return(usedAems);

            return results.Count - origResultCount;
        }
        private static int GetActionElementMaps(
            int playerId,
            int actionId,
            int actionId2,
            ControllerElementGlyphSelectorOptions options,
            System.Predicate<ActionElementMap> isAemAllowedHandlerOverride,
            List<Pair<ActionElementMapPair>> results,
            int maxResultCount
        ) {
            if (!ReInput.isReady) return 0;
            if (options == null) return 0;
            if (results == null) return 0;
            if (maxResultCount < 0) maxResultCount = 0;

            InputAction action = ReInput.mapping.GetAction(actionId);
            if (action == null) return 0;
            InputAction action2 = ReInput.mapping.GetAction(actionId2);
            if (action2 == null) return 0;
            if (action2 == action) return 0;

            Player player = ReInput.players.GetPlayer(playerId);
            if (player == null) return 0;

            ControllerElementType[] controllerElementTypeOrder = options.controllerElementTypeOrder;

            int origResultCount = results.Count;
            int resultsRemainingCount = maxResultCount > 0 ? maxResultCount : -1;

            bool useFirstControllerResults = options.useFirstControllerResults;

            // Get Player's last active controller
            Controller lastActiveController = player.controllers.GetLastActiveController();

            FastList<ActionElementMap> workingActionElementMaps = aemFastListPool.Get();
            FastList<ActionElementMap> workingActionElementMaps2 = aemFastListPool.Get();
            FastList<ControllerInfo> usedControllers = controllerInfoFastListPool.Get();  // prevent checking the same controller multiple times to avoid redundant results
            try {

                // Get the isAllowed handler
                System.Predicate<ActionElementMap> isAemAllowedHandler = null;
                if (isAemAllowedHandlerOverride != null) { // override is preferred if set
                    isAemAllowedHandler = isAemAllowedHandlerOverride;
                } else if (options != null) { // use the handler supplied in options
                    isAemAllowedHandler = options.isActionElementMapAllowedHandler;
                }
                // Fall back to default handler if none set
                if (isAemAllowedHandler == null) isAemAllowedHandler = defaultGetElementMapsWithActionisAllowedHandler;

                // Get all action element maps for the Action

                if (options.useLastActiveController && lastActiveController != null) {

                    Controller otherController = null;

                    // Treat keyboard and mouse as a single controller because most people want it to work this way.
                    // Override last active controller with either the keyboard or mouse, whichever is higher priority.
                    // This avoids glyphs switching as keyboard and mouse are used alternately.
                    if (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Mouse) {
                        if (IsMousePrioritizedOverKeyboard(options)) {
                            if (ReInput.controllers.Mouse.enabled && player.controllers.hasMouse) {
                                lastActiveController = ReInput.controllers.Mouse;
                                otherController = ReInput.controllers.Keyboard;
                            }
                        } else {
                            if (ReInput.controllers.Keyboard.enabled && player.controllers.hasKeyboard) {
                                lastActiveController = ReInput.controllers.Keyboard;
                                otherController = ReInput.controllers.Mouse;
                            }
                        }
                    }

                    // Prioritize last active controller
                    if (!Contains(usedControllers, lastActiveController.type, lastActiveController.id)) {
                        if (GetElementMapsWithAction(player, lastActiveController.type, lastActiveController.id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                            if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                            }
                            usedControllers.Add(new ControllerInfo(lastActiveController.type, lastActiveController.id));
                        }
                    }

                    // Fallback to secondary for keyboard/mouse
                    if (otherController != null) {
                        if (!Contains(usedControllers, otherController.type, otherController.id)) {
                            if (GetElementMapsWithAction(player, otherController.type, otherController.id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                    if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                }
                            }
                            usedControllers.Add(new ControllerInfo(otherController.type, otherController.id));
                        }
                    }

                    if (useFirstControllerResults && results.Count - origResultCount > 0) return results.Count - origResultCount;

                    // Fall back to last-known active controller type without the controller id.
                    // This allows falling back to other Joysticks if binding is not found in the active one.
                    {
                        ControllerType controllerType = lastActiveController.type;

                        switch (controllerType) {
                            case ControllerType.Joystick: {
                                    int id;
                                    for (int i = 0; i < player.controllers.joystickCount; i++) {
                                        id = player.controllers.Joysticks[i].id;
                                        if (Contains(usedControllers, controllerType, id)) continue;
                                        if (GetElementMapsWithAction(player, controllerType, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                            if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                            }
                                        }
                                        usedControllers.Add(new ControllerInfo(controllerType, id));
                                    }
                                }
                                break;
                            case ControllerType.Custom: {
                                    int id;
                                    for (int i = 0; i < player.controllers.customControllerCount; i++) {
                                        id = player.controllers.CustomControllers[i].id;
                                        if (Contains(usedControllers, controllerType, id)) continue;
                                        if (GetElementMapsWithAction(player, controllerType, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                            if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                            }
                                        }
                                        usedControllers.Add(new ControllerInfo(controllerType, id));
                                    }
                                    break;
                                }
                                // Keyboard and Mouse would have been handled above
                        }
                    }
                }

                // Fall back to other controller types in order of priority
                {
                    const int flag_joysticks = 1;
                    const int flag_keyboard = 2;
                    const int flag_mouse = 4;
                    const int flag_custom = 8;

                    ControllerType controllerType;

                    // Fall back to other controller types in order of priority
                    // Go through all controllers individually and return when any valid results found

                    int remainingControllerTypeFlags =
                        flag_joysticks |
                        flag_keyboard |
                        flag_mouse |
                        flag_custom;

                    ControllerType[] controllerTypes = options.controllerTypeOrder;

                    int currentIndex = 0;

                    // Search controllers in order of type
                    while (true) {
                        if (remainingControllerTypeFlags == 0) break;

                        if (currentIndex < controllerTypes.Length) { // process the sorted list first
                            controllerType = controllerTypes[currentIndex];

                        } else { // process any remaining types last

                            // User may have excluded some controller types from controller type order list.
                            // This list should not be used as an inclusion/exclusion list, only ordering.
                            // Process any missing controller types in a fixed order.
                            if ((remainingControllerTypeFlags & flag_joysticks) != 0) controllerType = ControllerType.Joystick;
                            else if ((remainingControllerTypeFlags & flag_mouse) != 0) controllerType = ControllerType.Mouse;
                            else if ((remainingControllerTypeFlags & flag_keyboard) != 0) controllerType = ControllerType.Keyboard;
                            else if ((remainingControllerTypeFlags & flag_custom) != 0) controllerType = ControllerType.Custom;
                            else throw new System.NotImplementedException();
                        }

                        {
                            int id;

                            switch (controllerType) {
                                case ControllerType.Joystick:
                                    if ((remainingControllerTypeFlags & flag_joysticks) != 0) {
                                        for (int j = 0; j < player.controllers.joystickCount; j++) {
                                            id = player.controllers.Joysticks[j].id;
                                            if (Contains(usedControllers, controllerType, id)) continue;
                                            if (GetElementMapsWithAction(player, controllerType, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                                if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                    if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                }
                                            }
                                            usedControllers.Add(new ControllerInfo(controllerType, id));
                                        }
                                        remainingControllerTypeFlags &= ~flag_joysticks;
                                    }
                                    break;
                                case ControllerType.Custom:
                                    if ((remainingControllerTypeFlags & flag_custom) != 0) {
                                        for (int j = 0; j < player.controllers.customControllerCount; j++) {
                                            id = player.controllers.CustomControllers[j].id;
                                            if (Contains(usedControllers, controllerType, id)) continue;
                                            if (GetElementMapsWithAction(player, controllerType, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                                if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                    if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                }
                                            }
                                            usedControllers.Add(new ControllerInfo(controllerType, id));
                                        }
                                        remainingControllerTypeFlags &= ~flag_custom;
                                    }
                                    break;
                                case ControllerType.Mouse:
                                case ControllerType.Keyboard: {

                                        bool done = false;
                                        bool doBoth = useFirstControllerResults; // get results for both if use first controller so KB/Mouse are treated as one controller

                                        if ((controllerType == ControllerType.Mouse || doBoth) && (remainingControllerTypeFlags & flag_mouse) != 0) {
                                            if (player.controllers.hasMouse) {
                                                id = ReInput.controllers.Mouse.id;
                                                if (!Contains(usedControllers, ControllerType.Mouse, id)) {
                                                    if (GetElementMapsWithAction(player, ControllerType.Mouse, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                                        if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                            if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                            done = true;
                                                        }
                                                    }
                                                    usedControllers.Add(new ControllerInfo(ControllerType.Mouse, id));
                                                }
                                            }
                                            remainingControllerTypeFlags &= ~flag_mouse;
                                        }

                                        if ((controllerType == ControllerType.Keyboard || doBoth) && (remainingControllerTypeFlags & flag_keyboard) != 0) {
                                            if (player.controllers.hasKeyboard) {
                                                id = ReInput.controllers.Keyboard.id;
                                                if (!Contains(usedControllers, ControllerType.Keyboard, id)) {
                                                    if (GetElementMapsWithAction(player, ControllerType.Keyboard, id, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                                        if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                                            if (useFirstControllerResults || (maxResultCount > 0 && resultsRemainingCount <= 0)) return results.Count - origResultCount;
                                                            done = true;
                                                        }
                                                    }
                                                    usedControllers.Add(new ControllerInfo(ControllerType.Keyboard, id));
                                                }
                                            }
                                            remainingControllerTypeFlags &= ~flag_keyboard;
                                        }

                                        if (useFirstControllerResults && done) {
                                            return results.Count - origResultCount;
                                        }

                                        break;
                                    }
                            }
                        }
                        currentIndex += 1;
                    }
                }

                // Fall back to default controllers if set
                if (options.useDefaultControllers) {
                    List<ControllerElementGlyphSelectorOptions.ControllerSelector> defaultControllers = options.defaultControllers;
                    int defaultControllerCount = defaultControllers != null ? defaultControllers.Count : 0;
                    ControllerIdentifier identifier;
                    ControllerElementGlyphSelectorOptions.ControllerSelector defaultController;
                    List<ControllerElementGlyphSelectorOptions.ControllerMapSelector> maps;
                    ControllerMap controllerMap;
                    int mapCount;
                    for (int controllerIndex = 0; controllerIndex < defaultControllerCount; controllerIndex++) {
                        defaultController = defaultControllers[controllerIndex];
                        maps = defaultController.controllerMapSelectors;
                        if (maps != null) {
                            mapCount = maps.Count;
                            identifier = ControllerIdentifier.Blank;
                            identifier.controllerType = defaultController.controllerType;
                            identifier.hardwareTypeGuid = defaultController.hardwareTypeGuid;
                            identifier.hardwareIdentifier = defaultController.hardwareIdentifier;
                            for (int i = 0; i < mapCount; i++) {
                                controllerMap = DefaultControllerMapCache.instance.GetControllerMap(player.id, identifier, maps[i].mapCategoryName, maps[i].layoutName);
                                if (controllerMap != null) {
                                    controllerMap.enabled = true; // loaded maps start disabled so enable it
                                    if (GetElementMapsWithAction(controllerMap, actionId, actionId2, isAemAllowedHandler, controllerElementTypeOrder, workingActionElementMaps, workingActionElementMaps2) > 0) {
                                        if (Action2DHelper.GetActionElementMaps(workingActionElementMaps, workingActionElementMaps2, controllerElementTypeOrder, results, ref resultsRemainingCount) > 0) {
                                            if (maxResultCount > 0 && resultsRemainingCount <= 0) return results.Count - origResultCount;
                                        }
                                    }
                                }
                            }
                        }
                        if (useFirstControllerResults && results.Count - origResultCount > 0) return results.Count - origResultCount;
                    }
                }

                return results.Count - origResultCount;

            } finally {

                // Clean up memory
                aemFastListPool.Return(workingActionElementMaps);
                aemFastListPool.Return(workingActionElementMaps2);
                controllerInfoFastListPool.Return(usedControllers);
            }
        }

        private static class Action2DHelper {

            private static readonly FastList<bool> _usedAction1Aems = new FastList<bool>(16);
            private static readonly FastList<bool> _usedAction2Aems = new FastList<bool>(16);

            private static FastList<ActionElementMap> _action1Aems;
            private static FastList<ActionElementMap> _action2Aems;
            private static ControllerElementType[] _controllerElementTypeOrder;
            private static List<Pair<ActionElementMapPair>> _results;
            private static int _resultsRemainingCount;

            public static int GetActionElementMaps(
                FastList<ActionElementMap> action1Aems,
                FastList<ActionElementMap> action2Aems,
                ControllerElementType[] controllerElementTypeOrder,
                List<Pair<ActionElementMapPair>> results,
                ref int resultsRemainingCount
            ) {
                _action1Aems = action1Aems;
                _action2Aems = action2Aems;
                _controllerElementTypeOrder = controllerElementTypeOrder;
                _results = results;
                _resultsRemainingCount = resultsRemainingCount;

                int origResultCount = results.Count;

                _usedAction1Aems.SetCount(action1Aems.Count);
                _usedAction2Aems.SetCount(action2Aems.Count);

                try {
                    var steps = GetSteps();
                    for (int i = 0; i < steps.Length; i++) {
                        if (steps[i]() == Result.Quit) break;
                    }

                    // Could do a second sorted match pass that doesn't require all 4 directions to match.
                    // This would handle, for example, a d-pad with only 3 directions bound.

                    return results.Count - origResultCount;

                } finally {
                    resultsRemainingCount = _resultsRemainingCount;
                    _usedAction1Aems.Clear();
                    _usedAction2Aems.Clear();
                }
            }

            private static Result GetCompleteFullAxisPairs() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                ActionElementMap thisAem;
                ActionElementMap otherAem;
                FastList<ActionElementMap> thisAems = _action1Aems;
                FastList<bool> thisUsedAems = _usedAction1Aems;
                FastList<bool> otherUsedAems = _usedAction2Aems;
                int otherAemIndex;
                const ControllerElementType controllerElementType = ControllerElementType.Axis;
                const AxisType axisType = AxisType.Normal;
                int count = thisAems.Count;

                for (int thisIndex = 0; thisIndex < count; thisIndex++) {
                    if (thisUsedAems.Array[thisIndex]) continue;
                    thisAem = thisAems.Array[thisIndex];
                    if (thisAem.elementType != controllerElementType || thisAem.axisType != axisType) continue;
                    if ((otherAem = Find(_action2Aems, 0, controllerElementType, axisType, out otherAemIndex, otherUsedAems)) == null) continue;
                    _results.Add(new Pair<ActionElementMapPair>(new ActionElementMapPair(thisAem, null), new ActionElementMapPair(otherAem, null)));
                    thisUsedAems.Array[thisIndex] = true;
                    otherUsedAems.Array[otherAemIndex] = true;
                    if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                }
                return Result.GoNext;
            }

            private static Result GetMixedFullAxisAndSplitAxisPairs() {
                if (_resultsRemainingCount == 0) return Result.Quit;
                
                // Prioritizes full-axis if available

                FastList<ActionElementMap> thisAems;
                FastList<ActionElementMap> otherAems;
                FastList<bool> thisUsedAems;
                FastList<bool> otherUsedAems;
                ActionElementMap aem1;
                ActionElementMap otherAem1;
                ActionElementMap otherAem2;
                int otherAemIndex1;
                int otherAemIndex2;
                const ControllerElementType controllerElementType = ControllerElementType.Axis;
                int list1Index = 0;
                int list2Index = 0;
                int thisAemIndex;

                do {
                    // Determine which list to search prioritizing full-axis bindings over split-axis
                    if (Find(_action1Aems, list1Index, controllerElementType, AxisType.Normal, out thisAemIndex, _usedAction1Aems) != null) {
                        thisAems = _action1Aems;
                        otherAems = _action2Aems;
                        list1Index = thisAemIndex;
                        thisUsedAems = _usedAction1Aems;
                        otherUsedAems = _usedAction2Aems;
                    } else if (Find(_action2Aems, list2Index, controllerElementType, AxisType.Normal, out thisAemIndex, _usedAction2Aems) != null) {
                        thisAems = _action2Aems;
                        otherAems = _action1Aems;
                        list2Index = thisAemIndex;
                        thisUsedAems = _usedAction2Aems;
                        otherUsedAems = _usedAction1Aems;
                    } else {
                        break; // not in either list, quit
                    }

                    aem1 = thisAems.Array[thisAemIndex];

                    if ((otherAem1 = Find(otherAems, 0, controllerElementType, AxisType.Split, Pole.Negative, out otherAemIndex1, otherUsedAems)) != null && // require split
                        ((otherAem2 = Find(otherAems, 0, controllerElementType, AxisType.Split, Pole.Positive, out otherAemIndex2, otherUsedAems)) != null)) {

                        _results.Add(Create(new ActionElementMapPair(aem1, null), new ActionElementMapPair(otherAem1, otherAem2), thisAems == _action2Aems));
                        thisUsedAems.Array[thisAemIndex] = true;
                        otherUsedAems.Array[otherAemIndex1] = true;
                        otherUsedAems.Array[otherAemIndex2] = true;
                        if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                    }

                    // Increment index
                    if (thisAems == _action1Aems) {
                        list1Index = thisAemIndex + 1;
                    } else {
                        list2Index = thisAemIndex + 1;
                    }
                } while (list1Index < _action1Aems.Count && list2Index < _action2Aems.Count);

                return Result.GoNext;
            }

            private static Result GetCompleteSplitAxisQuadSets() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                ActionElementMap thisAem1;
                ActionElementMap thisAem2;
                ActionElementMap otherAem1;
                ActionElementMap otherAem2;
                FastList<ActionElementMap> thisAems = _action1Aems;
                FastList<ActionElementMap> otherAems = _action2Aems;
                FastList<bool> thisUsedAems = _usedAction1Aems;
                FastList<bool> otherUsedAems = _usedAction2Aems;
                int thisIndex2;
                int otherAemIndex1;
                int otherAemIndex2;
                const ControllerElementType controllerElementType = ControllerElementType.Axis;
                int count = thisAems.Count;

                for (int thisIndex = 0; thisIndex < count; thisIndex++) {
                    if (thisUsedAems.Array[thisIndex]) continue;
                    thisAem1 = thisAems.Array[thisIndex];
                    if (thisAem1.elementType != controllerElementType || thisAem1.axisType != AxisType.Split || thisAem1.axisContribution != Pole.Negative) continue;
                    if ((thisAem2 = Find(thisAems, 0, controllerElementType, AxisType.Split, Pole.Positive, out thisIndex2, thisUsedAems)) == null) continue; // no matching positive
                    if ((otherAem1 = Find(otherAems, 0, controllerElementType, AxisType.Split, Pole.Negative, out otherAemIndex1, otherUsedAems)) == null) continue;
                    if ((otherAem2 = Find(otherAems, 0, controllerElementType, AxisType.Split, Pole.Positive, out otherAemIndex2, otherUsedAems)) == null) continue;
                    _results.Add(new Pair<ActionElementMapPair>(new ActionElementMapPair(thisAem1, thisAem2), new ActionElementMapPair(otherAem1, otherAem2)));
                    thisUsedAems.Array[thisIndex] = true;
                    thisUsedAems.Array[thisIndex2] = true;
                    otherUsedAems.Array[otherAemIndex1] = true;
                    otherUsedAems.Array[otherAemIndex2] = true;
                    if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                }
                return Result.GoNext;
            }

            private static Result GetCompleteButtonQuadSets() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                ActionElementMap aem1;
                ActionElementMap aem2;
                ActionElementMap otherAem1;
                ActionElementMap otherAem2;
                FastList<ActionElementMap> thisAems = _action1Aems;
                FastList<ActionElementMap> otherAems = _action2Aems;
                FastList<bool> thisUsedAems = _usedAction1Aems;
                FastList<bool> otherUsedAems = _usedAction2Aems;
                int thisIndex2;
                int otherAemIndex1;
                int otherAemIndex2;
                const ControllerElementType controllerElementType = ControllerElementType.Button;
                int count = thisAems.Count;

                for (int thisIndex = 0; thisIndex < count; thisIndex++) {
                    if (thisUsedAems.Array[thisIndex]) continue;
                    aem1 = _action1Aems.Array[thisIndex];
                    if (aem1.elementType != controllerElementType || aem1.axisContribution != Pole.Negative) continue;
                    if ((aem2 = Find(thisAems, 0, controllerElementType, AxisType.None, Pole.Positive, out thisIndex2, thisUsedAems)) == null) continue; // no matching positive
                    if ((otherAem1 = Find(otherAems, 0, controllerElementType, AxisType.None, Pole.Negative, out otherAemIndex1, otherUsedAems)) == null) continue;
                    if ((otherAem2 = Find(otherAems, 0, controllerElementType, AxisType.None, Pole.Positive, out otherAemIndex2, otherUsedAems)) == null) continue;
                    _results.Add(new Pair<ActionElementMapPair>(new ActionElementMapPair(aem1, aem2), new ActionElementMapPair(otherAem1, otherAem2)));
                    thisUsedAems.Array[thisIndex] = true;
                    thisUsedAems.Array[thisIndex2] = true;
                    otherUsedAems.Array[otherAemIndex1] = true;
                    otherUsedAems.Array[otherAemIndex2] = true;
                    if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                }
                return Result.GoNext;
            }

            private static Result GetMixedFullAxisAndButtonPairs() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                FastList<ActionElementMap> thisAems;
                FastList<ActionElementMap> otherAems;
                FastList<bool> thisUsedAems;
                FastList<bool> otherUsedAems;
                ActionElementMap aem1;
                ActionElementMap otherAem1;
                ActionElementMap otherAem2;
                int otherAemIndex1;
                int otherAemIndex2;
                int list1Index = 0;
                int list2Index = 0;
                int thisAemIndex;
                ActionElementMapPair r;

                if (elementTypePriority == ControllerElementType.Button) {
                    ActionElementMap aem2;
                    int thisAemIndex2;
                    do {
                        // Determine which list to search prioritizing buttons over full-axis bindings
                        if (Find(_action1Aems, list1Index, ControllerElementType.Button, AxisType.None, out thisAemIndex, _usedAction1Aems) != null) {
                            thisAems = _action1Aems;
                            otherAems = _action2Aems;
                            list1Index = thisAemIndex;
                            thisUsedAems = _usedAction1Aems;
                            otherUsedAems = _usedAction2Aems;
                        } else if (Find(_action2Aems, list2Index, ControllerElementType.Button, AxisType.None, out thisAemIndex, _usedAction2Aems) != null) {
                            thisAems = _action2Aems;
                            otherAems = _action1Aems;
                            list2Index = thisAemIndex;
                            thisUsedAems = _usedAction2Aems;
                            otherUsedAems = _usedAction1Aems;
                        } else {
                            break; // not in either list, quit
                        }

                        aem1 = thisAems.Array[thisAemIndex];

                        if ((aem2 = Find(thisAems, 0, ControllerElementType.Button, AxisType.None, aem1.axisContribution == Pole.Positive ? Pole.Negative : Pole.Positive, out thisAemIndex2, thisUsedAems)) != null &&
                            (otherAem1 = Find(otherAems, 0, ControllerElementType.Axis, AxisType.Normal, out otherAemIndex1, otherUsedAems)) != null) { // require full-axis

                            if (TryCreate(aem1, aem2, out r)) {
                                _results.Add(Create(r, new ActionElementMapPair(otherAem1, null), thisAems == _action2Aems));
                                thisUsedAems.Array[thisAemIndex] = true;
                                thisUsedAems.Array[thisAemIndex2] = true;
                                otherUsedAems.Array[otherAemIndex1] = true;
                                if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                            }
                        }

                        // Increment index
                        if (thisAems == _action1Aems) {
                            list1Index = thisAemIndex + 1;
                        } else {
                            list2Index = thisAemIndex + 1;
                        }
                    } while (list1Index < _action1Aems.Count && list2Index < _action2Aems.Count);

                } else { // axis

                    do {
                        // Determine which list to search prioritizing full-axis bindings over buttons
                        if (Find(_action1Aems, list1Index, ControllerElementType.Axis, AxisType.Normal, out thisAemIndex, _usedAction1Aems) != null) {
                            thisAems = _action1Aems;
                            otherAems = _action2Aems;
                            list1Index = thisAemIndex;
                            thisUsedAems = _usedAction1Aems;
                            otherUsedAems = _usedAction2Aems;
                        } else if (Find(_action2Aems, list2Index, ControllerElementType.Axis, AxisType.Normal, out thisAemIndex, _usedAction2Aems) != null) {
                            thisAems = _action2Aems;
                            otherAems = _action1Aems;
                            list2Index = thisAemIndex;
                            thisUsedAems = _usedAction2Aems;
                            otherUsedAems = _usedAction1Aems;
                        } else {
                            break; // not in either list, quit
                        }

                        aem1 = thisAems.Array[thisAemIndex];

                        if ((otherAem1 = Find(otherAems, 0, ControllerElementType.Button, AxisType.None, Pole.Negative, out otherAemIndex1, otherUsedAems)) != null && // require button
                            ((otherAem2 = Find(otherAems, 0, ControllerElementType.Button, AxisType.None, Pole.Positive, out otherAemIndex2, otherUsedAems)) != null)) {

                            if (TryCreate(otherAem1, otherAem2, out r)) {
                                _results.Add(Create(new ActionElementMapPair(aem1, null), r, thisAems == _action2Aems));
                                thisUsedAems.Array[thisAemIndex] = true;
                                otherUsedAems.Array[otherAemIndex1] = true;
                                otherUsedAems.Array[otherAemIndex2] = true;
                                if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                            }
                        }

                        // Increment index
                        if (thisAems == _action1Aems) {
                            list1Index = thisAemIndex + 1;
                        } else {
                            list2Index = thisAemIndex + 1;
                        }
                    } while (list1Index < _action1Aems.Count && list2Index < _action2Aems.Count);
                }

                return Result.GoNext;
            }

            private static Result GetMixedSplitAxisAndButtonPairs() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                // No type prioritization. Displays in the order found.

                ActionElementMap aem1;
                ActionElementMap aem2;
                ActionElementMap otherAem1;
                ActionElementMap otherAem2;
                FastList<ActionElementMap> thisAems = _action1Aems;
                FastList<ActionElementMap> otherAems = _action2Aems;
                FastList<bool> thisUsedAems = _usedAction1Aems;
                FastList<bool> otherUsedAems = _usedAction2Aems;
                int thisIndex2;
                int otherAemIndex1;
                int otherAemIndex2;
                int count = _action1Aems.Count;

                for (int thisIndex = 0; thisIndex < count; thisIndex++) {
                    if (thisUsedAems.Array[thisIndex]) continue;
                    aem1 = _action1Aems.Array[thisIndex];
                    if (aem1.elementType == ControllerElementType.Axis && aem1.axisType == AxisType.Split && aem1.axisContribution == Pole.Negative) {
                        if ((aem2 = Find(_action1Aems, 0, ControllerElementType.Axis, AxisType.Split, Pole.Positive, out thisIndex2, thisUsedAems)) == null) continue; // no matching positive
                        if ((otherAem1 = Find(_action2Aems, 0, ControllerElementType.Button, AxisType.None, Pole.Negative, out otherAemIndex1, otherUsedAems)) == null) continue;
                        if ((otherAem2 = Find(_action2Aems, 0, ControllerElementType.Button, AxisType.None, Pole.Positive, out otherAemIndex2, otherUsedAems)) == null) continue;
                        _results.Add(new Pair<ActionElementMapPair>(new ActionElementMapPair(aem1, aem2), new ActionElementMapPair(otherAem1, otherAem2)));
                        thisUsedAems.Array[thisIndex] = true;
                        thisUsedAems.Array[thisIndex2] = true;
                        otherUsedAems.Array[otherAemIndex1] = true;
                        otherUsedAems.Array[otherAemIndex2] = true;
                        if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                    } else if (aem1.elementType == ControllerElementType.Button && aem1.axisContribution == Pole.Negative) {
                        if ((aem2 = Find(_action1Aems, 0, ControllerElementType.Button, AxisType.None, Pole.Positive, out thisIndex2, thisUsedAems)) == null) continue; // no matching positive
                        if ((otherAem1 = Find(_action2Aems, 0, ControllerElementType.Axis, AxisType.Split, Pole.Negative, out otherAemIndex1, otherUsedAems)) == null) continue;
                        if ((otherAem2 = Find(_action2Aems, 0, ControllerElementType.Axis, AxisType.Split, Pole.Positive, out otherAemIndex2, otherUsedAems)) == null) continue;
                        _results.Add(new Pair<ActionElementMapPair>(new ActionElementMapPair(aem1, aem2), new ActionElementMapPair(otherAem1, otherAem2)));
                        thisUsedAems.Array[thisIndex] = true;
                        thisUsedAems.Array[thisIndex2] = true;
                        otherUsedAems.Array[otherAemIndex1] = true;
                        otherUsedAems.Array[otherAemIndex2] = true;
                        if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                    }
                }
                return Result.GoNext;
            }

            private static Result GetRemaining() {
                if (_resultsRemainingCount == 0) return Result.Quit;

                // Remaining mixed results

                FastList<ActionElementMap> thisAems;
                FastList<bool> thisUsedAems;
                ActionElementMap aem1;
                ActionElementMap aem2;
                int thisIndex2;
                Pair<ActionElementMapPair> workingResult = new Pair<ActionElementMapPair>();
                int actionSlotIndex;
                bool isPositive;
                int i;
                int list1Index = 0;
                int list2Index = 0;
                int thisIndex;
                bool r;

                // Whatever is left must be added even if incomplete from both lists.
                // Results will be merged in the order in which they appear in the lists.

                do {
                    for (i = 0; i < 2; i++) {
                        if (i == 0) {
                            thisAems = _action1Aems;
                            thisIndex = list1Index;
                            actionSlotIndex = 0;
                            thisUsedAems = _usedAction1Aems;
                        } else {
                            thisAems = _action2Aems;
                            thisIndex = list2Index;
                            actionSlotIndex = 1;
                            thisUsedAems = _usedAction2Aems;
                        }

                        if (thisIndex >= thisAems.Count) continue; // no more results in this list

                        if (!thisUsedAems.Array[thisIndex]) {

                            aem1 = thisAems.Array[thisIndex];
                        
                            if (aem1.elementType == ControllerElementType.Axis) {
                                if (aem1.axisType == AxisType.Normal) {
                                    r = SetAndAddIfFull(new ActionElementMapPair(aem1, null), actionSlotIndex, ref workingResult, _results);
                                    thisUsedAems.Array[thisIndex] = true;
                                    if (r && !AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;

                                } else if (aem1.axisType == AxisType.Split) {

                                    isPositive = aem1.axisContribution == Pole.Positive;

                                    // Try to get the opposite split-axis
                                    aem2 = Find(thisAems, 0, ControllerElementType.Axis, AxisType.Split, isPositive ? Pole.Negative : Pole.Positive, out thisIndex2, thisUsedAems);

                                    // Fall back to button
                                    if (aem2 == null) {
                                        aem2 = Find(thisAems, 0, ControllerElementType.Button, AxisType.None, isPositive ? Pole.Negative : Pole.Positive, out thisIndex2, thisUsedAems);
                                    }

                                    // Place negative in a field for consistency
                                    r = isPositive ?
                                        SetAndAddIfFull(new ActionElementMapPair(aem2, aem1), actionSlotIndex, ref workingResult, _results) :
                                        SetAndAddIfFull(new ActionElementMapPair(aem1, aem2), actionSlotIndex, ref workingResult, _results);

                                    thisUsedAems.Array[thisIndex] = true;
                                    if (aem2 != null) thisUsedAems.Array[thisIndex2] = true;

                                    if (r && !AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                                }
                            } else if (aem1.elementType == ControllerElementType.Button) {

                                isPositive = aem1.axisContribution == Pole.Positive;

                                // Try to get the opposite button
                                aem2 = Find(thisAems, 0, ControllerElementType.Button, AxisType.None, isPositive ? Pole.Negative : Pole.Positive, out thisIndex2, thisUsedAems);

                                // Fall back to split-axis
                                if (aem2 == null) {
                                    aem2 = Find(thisAems, 0, ControllerElementType.Axis, AxisType.Split, isPositive ? Pole.Negative : Pole.Positive, out thisIndex2, thisUsedAems);
                                }

                                // Place negative in a field for consistency
                                r = isPositive ? SetAndAddIfFull(new ActionElementMapPair(aem2, aem1), actionSlotIndex, ref workingResult, _results) :
                                    SetAndAddIfFull(new ActionElementMapPair(aem1, aem2), actionSlotIndex, ref workingResult, _results);

                                thisUsedAems.Array[thisIndex] = true;
                                if (aem2 != null) thisUsedAems.Array[thisIndex2] = true;

                                if (r && !AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                            }
                        }

                        // Increment index
                        if (thisAems == _action1Aems) {
                            list1Index = thisIndex + 1;
                        } else {
                            list2Index = thisIndex + 1;
                        }
                    }
                } while (list1Index < _action1Aems.Count || list2Index < _action2Aems.Count);

                // Add the last one
                if (workingResult.a.Count > 0 || workingResult.b.Count > 0) {
                    _results.Add(workingResult);
                    if (!AllowMoreResultsDecrement(ref _resultsRemainingCount)) return Result.Quit;
                }

                return Result.GoNext;
            }

            private static System.Func<Result>[] GetSteps() {
                switch (_controllerElementTypeOrder[0]) {
                    case ControllerElementType.Button:
                        return steps_buttonPriority;
                    default:
                        return steps_axisPriority;
                }
            }

            private static System.Func<Result>[] __steps_axisPriority;
            private static System.Func<Result>[] steps_axisPriority {
                get {
                    if (__steps_axisPriority == null) {
                        // Sort results to prioritize complete sets regardless of ordering in controller map
                        //   Complete full-axis pairs
                        //   Mixed full-axis single and split-axis pairs
                        //   Complete split-axis quad sets
                        //   Complete button quad sets
                        //   Mixed full-axis single and button pairs
                        //   Mixed split-axis pairs and button quad pairs
                        //   Remaining mixed results (all 4 directions not required)
                        __steps_axisPriority = new System.Func<Result>[] {
                            GetCompleteFullAxisPairs,
                            GetMixedFullAxisAndSplitAxisPairs,
                            GetCompleteSplitAxisQuadSets,
                            GetCompleteButtonQuadSets,
                            GetMixedFullAxisAndButtonPairs,
                            GetMixedSplitAxisAndButtonPairs,
                            GetRemaining
                        };
                    }
                    return __steps_axisPriority;
                }
            }

            private static System.Func<Result>[] __steps_buttonPriority;
            private static System.Func<Result>[] steps_buttonPriority {
                get {
                    if (__steps_buttonPriority == null) {
                        __steps_buttonPriority = new System.Func<Result>[] {
                            GetCompleteButtonQuadSets,
                            GetCompleteFullAxisPairs,
                            GetMixedFullAxisAndSplitAxisPairs,
                            GetCompleteSplitAxisQuadSets,
                            GetMixedFullAxisAndButtonPairs,
                            GetMixedSplitAxisAndButtonPairs,
                            GetRemaining
                        };
                    }
                    return __steps_buttonPriority;
                }
            }

            private static ControllerElementType elementTypePriority {
                get {
                    return _controllerElementTypeOrder[0] == ControllerElementType.Button ? ControllerElementType.Button : ControllerElementType.Axis;
                }
            }

            private enum Result {
                GoNext,
                Quit
            }
        }

        private static int FindFullAxisBindingsOnly(FastList<ActionElementMap> actionElementMaps, FastList<bool> usedAems, List<ActionElementMapPair> results, ref int resultsRemainingCount) {
            if (resultsRemainingCount == 0) return 0;
            int origResultCount = results.Count;
            int aemCount = actionElementMaps.Count;
            ActionElementMap aem;
            for (int i = 0; i < aemCount; i++) {
                if (usedAems.Array[i]) continue;
                aem = actionElementMaps.Array[i];
                if (aem.elementType == ControllerElementType.Axis &&
                    aem.axisType == AxisType.Normal) {
                    results.Add(new ActionElementMapPair(aem, null));
                    usedAems.Array[i] = true;
                    if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                }
            }
            return results.Count - origResultCount;
        }

        private static int FindBindings(FastList<ActionElementMap> actionElementMaps, FastList<bool> usedAems, AxisRange actionRange, List<ActionElementMapPair> results, ref int resultsRemainingCount) {
            if (actionElementMaps.Count == 0) return 0;
            if (resultsRemainingCount == 0) return 0;

            int origResultCount = results.Count;
            int aemCount = actionElementMaps.Count;
            ActionElementMap aem;
            Pole pole;

            for (int i = 0; i < aemCount; i++) {
                if (usedAems.Array[i]) continue; // skip done
                aem = actionElementMaps.Array[i];
                switch (actionRange) {
                    case AxisRange.Full:
                        if (aem.axisRange == AxisRange.Full && aem.elementType == ControllerElementType.Axis) {
                            results.Add(new ActionElementMapPair(aem, null));
                            usedAems.Array[i] = true;
                            if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                        }
                        break;
                    case AxisRange.Positive:
                    case AxisRange.Negative:
                        pole = actionRange == AxisRange.Positive ? Pole.Positive : Pole.Negative;
                        if (aem.axisType == AxisType.Split || aem.elementType == ControllerElementType.Button) {
                            if (aem.axisContribution == pole) {
                                results.Add(Create(aem, pole));
                                usedAems.Array[i] = true;
                                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                            }
                        } else if (aem.axisType == AxisType.Normal) {
                            // Map is full-axis but need to get split-axis map
                            // Get special split-axis fake maps using internal function.
                            ActionElementMap positiveAem;
                            ActionElementMap negativeAem;
                            if (Rewired.Internal.Helpers.ActionElementMapHelper.TryGetSplitAxisMaps(aem, out positiveAem, out negativeAem)) {
                                results.Add(Create(positiveAem.axisContribution == pole ? positiveAem : negativeAem, pole));
                                usedAems.Array[i] = true; // add the full-axis one, not the split axis ones because they are just representations for glyphs
                                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // Special case for a full range query to show a positive binding if available.
            // This is used for button bindings so the user doesn't have to explicitly specify
            // a positive Action Range to get a result.
            if (actionRange == AxisRange.Full) {
                bool alreadyAdded;
                for (int i = 0; i < aemCount; i++) {
                    if (usedAems.Array[i]) continue; // skip done
                    alreadyAdded = false;
                    aem = actionElementMaps.Array[i];
                    if (aem.axisType == AxisType.Split || aem.elementType == ControllerElementType.Button) {
                        if (aem.axisContribution == Pole.Positive) {
                            // Make sure the binding wasn't already added above
                            for (int j = origResultCount; j < results.Count; j++) {
                                if (results[j].a == aem && results[j].b == null) {
                                    alreadyAdded = true;
                                    break;
                                }
                            }
                            if (!alreadyAdded) {
                                results.Add(new ActionElementMapPair(aem, null));
                                usedAems.Array[i] = true;
                                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                            }
                        }
                    }
                }
            }

            return results.Count - origResultCount;
        }

        private static int FindSplitAxisBindingPairsOnly(FastList<ActionElementMap> actionElementMaps, FastList<bool> usedAems, List<ActionElementMapPair> results, ref int resultsRemainingCount) {
            if (resultsRemainingCount == 0) return 0;

            int origResultCount = results.Count;
            ActionElementMap aem;
            int aemCount = actionElementMaps.Count;
            ActionElementMapPair currentResult = new ActionElementMapPair();
            int otherIndex;
            ActionElementMap otherAem;
            Pole oppositePole;

            // Prioritize pairs of compatible types regardless of the order in which the elements appear.
            // Prioritize controller element sibling pairs (members of same axis).
            // Cannot do the same for D-Pads, hat switches, etc., because there is no information available know they are members of a compound control.

            // First pass searches for pairs
            for (int aemIndex = 0; aemIndex < aemCount; aemIndex++) {
                if (usedAems.Array[aemIndex]) continue; // skip done
                aem = actionElementMaps.Array[aemIndex];
                if (aem.elementType != ControllerElementType.Axis) continue;
                if (aem.axisType == AxisType.Normal || aem.axisType == AxisType.None) continue;

                oppositePole = aem.axisContribution == Pole.Positive ? Pole.Negative : Pole.Positive;

                // Prioritize split-axis pairs that are part of the same element identifier
                otherAem = Find(actionElementMaps, 0, ControllerElementType.Axis, aem.elementIdentifierId, AxisType.Split, oppositePole, out otherIndex, usedAems);
                if (otherAem == null) {
                    // Fall back to any matching split-axis
                    otherAem = Find(actionElementMaps, 0, ControllerElementType.Axis, AxisType.Split, oppositePole, out otherIndex, usedAems);
                }

                if (otherAem == null) continue; // skip if pair isn't found

                Set(aem, aem.axisContribution, ref currentResult);
                Set(otherAem, oppositePole, ref currentResult);
                results.Add(currentResult);
                Clear(ref currentResult);
                usedAems.Array[aemIndex] = true;
                usedAems.Array[otherIndex] = true;
                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
            }

            return results.Count - origResultCount;
        }

        private static int FindButtonBindingPairsOnly(FastList<ActionElementMap> actionElementMaps, FastList<bool> usedAems, List<ActionElementMapPair> results, ref int resultsRemainingCount) {
            if (resultsRemainingCount == 0) return 0;

            int origResultCount = results.Count;
            ActionElementMap aem;
            int aemCount = actionElementMaps.Count;
            ActionElementMapPair currentResult = new ActionElementMapPair();
            int otherIndex;
            ActionElementMap otherAem;
            Pole oppositePole;

            for (int aemIndex = 0; aemIndex < aemCount; aemIndex++) {
                if (usedAems.Array[aemIndex]) continue; // skip done
                aem = actionElementMaps.Array[aemIndex];
                if (aem.elementType != ControllerElementType.Button) {
                    continue;
                }

                oppositePole = aem.axisContribution == Pole.Positive ? Pole.Negative : Pole.Positive;
                otherAem = Find(actionElementMaps, 0, ControllerElementType.Button, AxisType.None, oppositePole, out otherIndex, usedAems);
                if (otherAem == null) continue; // skip if pair isn't found
                
                Set(aem, aem.axisContribution, ref currentResult);
                Set(otherAem, oppositePole, ref currentResult);
                results.Add(currentResult);
                Clear(ref currentResult);
                usedAems.Array[aemIndex] = true;
                usedAems.Array[otherIndex] = true;
                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
            }

            return results.Count - origResultCount;
        }

        private static int FindSplitAxisAndButtonBindingPairsAndRemaining(FastList<ActionElementMap> actionElementMaps, FastList<bool> usedAems, List<ActionElementMapPair> results, ref int resultsRemainingCount) {
            if (resultsRemainingCount == 0) return 0;

            int origResultCount = results.Count;
            ActionElementMap aem;
            int aemCount = actionElementMaps.Count;
            ActionElementMapPair currentResult = new ActionElementMapPair();
            int otherIndex;
            ActionElementMap otherAem;
            Pole oppositePole;

            // Prioritize pairs of compatible types regardless of the order in which the elements appear.
            // Prioritize controller element sibling pairs (members of same axis).
            // Cannot do the same for D-Pads, hat switches, etc., because there is no information available know they are members of a compound control.

            // First pass searches for pairs
            for (int aemIndex = 0; aemIndex < aemCount; aemIndex++) {
                if (usedAems.Array[aemIndex]) continue; // skip done
                aem = actionElementMaps.Array[aemIndex];
                if (aem.elementType == ControllerElementType.Axis) {
                    if (aem.axisType == AxisType.Normal || aem.axisType == AxisType.None) {
                        continue;
                    }
                } else if (aem.elementType != ControllerElementType.Button) {
                    continue;
                }

                oppositePole = aem.axisContribution == Pole.Positive ? Pole.Negative : Pole.Positive;

                // Search for matching pairs by element type
                if (aem.elementType == ControllerElementType.Axis) {
                    // Prioritize split-axis pairs that are part of the same element identifier
                    otherAem = Find(actionElementMaps, 0, ControllerElementType.Axis, aem.elementIdentifierId, AxisType.Split, oppositePole, out otherIndex, usedAems);
                    if (otherAem == null) {
                        // Fall back to any matching split-axis
                        otherAem = Find(actionElementMaps, 0, ControllerElementType.Axis, AxisType.Split, oppositePole, out otherIndex, usedAems);
                    }
                } else { // button
                    otherAem = Find(actionElementMaps, 0, ControllerElementType.Button, AxisType.None, oppositePole, out otherIndex, usedAems);
                }

                if (otherAem == null) continue; // skip if pair isn't found

                Set(aem, aem.axisContribution, ref currentResult);
                Set(otherAem, oppositePole, ref currentResult);
                results.Add(currentResult);
                Clear(ref currentResult);
                usedAems.Array[aemIndex] = true;
                usedAems.Array[otherIndex] = true;
                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
            }

            // Second pass adds all remaining
            for (int aemIndex = 0; aemIndex < aemCount; aemIndex++) {
                if (usedAems.Array[aemIndex]) continue; // skip done
                aem = actionElementMaps.Array[aemIndex];
                if (aem.elementType == ControllerElementType.Axis) {
                    if (aem.axisType == AxisType.Normal || aem.axisType == AxisType.None) {
                        continue;
                    }
                } else if (aem.elementType != ControllerElementType.Button) {
                    continue;
                }
                if (Get(currentResult, aem.axisContribution) == null) {
                    Set(aem, aem.axisContribution, ref currentResult);
                    usedAems.Array[aemIndex] = true;
                }
                if (currentResult.Count == 2) {
                    results.Add(currentResult);
                    Clear(ref currentResult);
                    if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
                }
            }
            if (currentResult.Count > 0) { // add the last one which may be partial
                results.Add(currentResult);
                if (!AllowMoreResultsDecrement(ref resultsRemainingCount)) return results.Count - origResultCount;
            }

            return results.Count - origResultCount;
        }

        private static readonly List<ActionElementMap> GetElementMapsWithAction_tempAems = new List<ActionElementMap>();

        private static int GetElementMapsWithAction(Rewired.Player player, Rewired.ControllerType controllerType, int controllerId, int actionId, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate, ControllerElementType[] searchOrder, FastList<ActionElementMap> results) {
            results.Clear();

            player.controllers.maps.GetElementMapsWithAction(controllerType, controllerId, actionId, false, GetElementMapsWithAction_tempAems);

            // Sort results because ControllerMap returns Action Element Maps buttons first, axes second due to how they are stored.
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, results);

            RemoveInvalidElementMaps(player, results, 0, isAllowedPredicate);

            GetElementMapsWithAction_tempAems.Clear();
            return results.Count;
        }
        private static int GetElementMapsWithAction(ControllerMap controllerMap, int actionId, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate, ControllerElementType[] searchOrder, FastList<ActionElementMap> results) {
            results.Clear();
            
            if (controllerMap == null) return 0;
            controllerMap.GetElementMapsWithAction(actionId, false, GetElementMapsWithAction_tempAems);

            // Sort results because ControllerMap returns Action Element Maps buttons first, axes second due to how they are stored.
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, results);

            RemoveInvalidElementMaps(results, 0, isAllowedPredicate);

            GetElementMapsWithAction_tempAems.Clear();
            return results.Count;
        }
        private static int GetElementMapsWithAction(Rewired.Player player, Rewired.ControllerType controllerType, int controllerId, int actionId, int actionId2, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate, ControllerElementType[] searchOrder, FastList<ActionElementMap> action1Results, FastList<ActionElementMap> action2Results) {
            action1Results.Clear();
            action2Results.Clear();

            // Sort results because ControllerMap returns Action Element Maps buttons first, axes second due to how they are stored.

            player.controllers.maps.GetElementMapsWithAction(controllerType, controllerId, actionId, false, GetElementMapsWithAction_tempAems);
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, action1Results);
            RemoveInvalidElementMaps(player, action1Results, 0, isAllowedPredicate);

            player.controllers.maps.GetElementMapsWithAction(controllerType, controllerId, actionId2, false, GetElementMapsWithAction_tempAems);
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, action2Results);
            RemoveInvalidElementMaps(player, action2Results, 0, isAllowedPredicate);

            GetElementMapsWithAction_tempAems.Clear();
            return action1Results.Count + action2Results.Count;
        }
        private static int GetElementMapsWithAction(ControllerMap controllerMap, int actionId, int actionId2, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate, ControllerElementType[] searchOrder, FastList<ActionElementMap> action1Results, FastList<ActionElementMap> action2Results) {
            action1Results.Clear();
            action2Results.Clear();
            
            if (controllerMap == null) return 0;

            // Sort results because ControllerMap returns Action Element Maps buttons first, axes second due to how they are stored.

            controllerMap.GetElementMapsWithAction(actionId, false, GetElementMapsWithAction_tempAems);
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, action1Results);
            RemoveInvalidElementMaps(action1Results, 0, isAllowedPredicate);

            controllerMap.GetElementMapsWithAction(actionId2, false, GetElementMapsWithAction_tempAems);
            SortByElementType(GetElementMapsWithAction_tempAems, searchOrder, action2Results);
            RemoveInvalidElementMaps(action2Results, 0, isAllowedPredicate);

            GetElementMapsWithAction_tempAems.Clear();
            return action1Results.Count + action2Results.Count;
        }

        private static int RemoveInvalidElementMaps(Rewired.Player player, FastList<ActionElementMap> results, int startIndex, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate) {

            int origResultCount = results.Count;

            // Default filtering
            for (int i = origResultCount - 1; i >= startIndex; i--) {
                if (!player.controllers.ContainsController(results.Array[i].controllerMap.controller) || // controller is not assigned to Player
                    !results.Array[i].controllerMap.controller.enabled) { // controller is disabled
                    results.RemoveAt(i);
                }
            }

            // User-defined filtering
            RemoveInvalidElementMaps(results, startIndex, isAllowedPredicate);

            return results.Count - origResultCount;
        }
        private static int RemoveInvalidElementMaps(FastList<ActionElementMap> results, int startIndex, System.Predicate<Rewired.ActionElementMap> isAllowedPredicate) {

            int origResultCount = results.Count;

            // User-defined filtering
            if (isAllowedPredicate != null) {
                int currentCount = results.Count;
                bool remove;

                // Iterate in order instead of backwards because this is exposed to the user
                for (int i = startIndex; i < currentCount; i++) {
                    remove = false;
                    try {
                        if (!isAllowedPredicate(results.Array[i])) {
                            remove = true;
                        }
                    } catch (System.Exception ex) {
                        UnityEngine.Debug.LogError("Rewired: An exception was thrown in isAllowedPredicate callback. This exception was thrown by your code.\n" + ex);
                        continue;
                    }
                    if (remove) {
                        results.RemoveAt(i);
                        currentCount -= 1;
                        i -= 1;
                    }
                }
            }

            return results.Count - origResultCount;
        }

        private static System.Predicate<Rewired.ActionElementMap> __defaultGetElementMapsWithActionisAllowedHandler;
        private static System.Predicate<Rewired.ActionElementMap> defaultGetElementMapsWithActionisAllowedHandler {
            get {
                if (__defaultGetElementMapsWithActionisAllowedHandler == null) {
                    __defaultGetElementMapsWithActionisAllowedHandler = (Rewired.ActionElementMap aem) => {
                        if (aem == null || !aem.controllerMap.enabled || !aem.enabled) return false;
                        return true;
                    };
                }
                return __defaultGetElementMapsWithActionisAllowedHandler;
            }
        }

        #endregion

        #region Misc

        private static ActionElementMap Find(FastList<ActionElementMap> list, int startIndex, ControllerElementType controllerElementType, AxisType axisType, out int index, FastList<bool> used) {
            int count = list.Count;
            ActionElementMap aem;
            for (int i = startIndex; i < count; i++) {
                if (used.Array[i]) continue;
                aem = list.Array[i];
                if (aem.elementType == controllerElementType && aem.axisType == axisType) {
                    index = i;
                    return aem;
                }
            }
            index = -1;
            return null;
        }
        private static ActionElementMap Find(FastList<ActionElementMap> list, int startIndex, ControllerElementType controllerElementType, AxisType axisType, Pole axisContribution, out int index, FastList<bool> used) {
            int count = list.Count;
            ActionElementMap aem;
            for (int i = startIndex; i < count; i++) {
                if (used.Array[i]) continue;
                aem = list.Array[i];
                if (aem.elementType == controllerElementType && aem.axisType == axisType && aem.axisContribution == axisContribution) {
                    index = i;
                    return aem;
                }
            }
            index = -1;
            return null;
        }
        private static ActionElementMap Find(FastList<ActionElementMap> list, int startIndex, ControllerElementType controllerElementType, int elementIdentifierId, AxisType axisType, Pole axisContribution, out int index, FastList<bool> used) {
            int count = list.Count;
            ActionElementMap aem;
            for (int i = startIndex; i < count; i++) {
                if (used.Array[i]) continue;
                aem = list.Array[i];
                if (aem.elementType == controllerElementType &&
                    aem.elementIdentifierId == elementIdentifierId &&
                    aem.axisType == axisType &&
                    aem.axisContribution == axisContribution) {
                    index = i;
                    return aem;
                }
            }
            index = -1;
            return null;
        }

        private static bool Contains(FastList<ControllerInfo> list, ControllerType type, int id) {
            for (int i = 0; i < list.Count; i++) {
                if (list.Array[i].type == type && list.Array[i].controllerId == id) return true;
            }
            return false;
        }

        private static Pair<ActionElementMapPair> Create(ActionElementMapPair a, ActionElementMapPair b, bool reverse) {
            if (reverse) {
                return new Pair<ActionElementMapPair>(b, a);
            }
            return new Pair<ActionElementMapPair>(a, b);
        }
        private static ActionElementMapPair Create(ActionElementMap aem, Pole pole) {
            switch (pole) {
                case Pole.Positive:
                    return new ActionElementMapPair(null, aem);
                case Pole.Negative:
                    return new ActionElementMapPair(aem, null);
                default:
                    throw new System.NotImplementedException();
            }
        }
        
        // Creates a pair given a negative and a positive axis contribution. Fails if both are same sign.
        private static bool TryCreate(ActionElementMap aem1, ActionElementMap aem2, out ActionElementMapPair result) {
            result = new ActionElementMapPair();
            bool error = false;
            ActionElementMap aem;
            for (int i = 0; i < 2; i++) {
                aem = i == 0 ? aem1 : aem2;
                if (aem != null) {
                    if (aem.axisContribution == Pole.Negative) {
                        if (result.a != null) error = true;
                        else result.a = aem;
                    } else {
                        if (result.b != null) error = true;
                        else result.b = aem;
                    }
                }
            }
            return !error;
        }

        private static bool SetAndAddIfFull(ActionElementMapPair item, int index, ref Pair<ActionElementMapPair> target, List<Pair<ActionElementMapPair>> items) {
            bool added = false;
            if (!TrySet(item, index, ref target)) {
                // Target is full. Add and go next.
                items.Add(target);
                added = true;
                Clear(ref target);
                TrySet(item, index, ref target);
            }
            // Add to list if full
            if (target.a.Count > 0 && target.b.Count > 0) {
                items.Add(target);
                added = true;
                Clear(ref target);
            }
            return added;
        }
        private static bool TrySet(ActionElementMapPair item, int index, ref Pair<ActionElementMapPair> target) {
            switch (index) {
                case 0:
                    if (target.a.Count > 0) return false;
                    target.a = item;
                    return true;
                case 1:
                    if (target.b.Count > 0) return false;
                    target.b = item;
                    return true;
                default:
                    throw new System.ArgumentOutOfRangeException("index");
            }
        }

        private static void Set(ActionElementMap aem, Pole pole, ref ActionElementMapPair destination) {
            switch (pole) {
                case Pole.Positive:
                    destination.b = aem;
                    return;
                case Pole.Negative:
                    destination.a = aem;
                    return;
                default:
                    throw new System.NotImplementedException();
            }
        }

        private static ActionElementMap Get(ActionElementMapPair source, Pole pole) {
            switch (pole) {
                case Pole.Positive:
                    return source.b;
                case Pole.Negative:
                    return source.a;
                default:
                    throw new System.NotImplementedException();
            }
        }

        private static void Clear(ref Pair<ActionElementMapPair> target) {
            Clear(ref target.a);
            Clear(ref target.b);
        }
        private static void Clear(ref ActionElementMapPair target) {
            target.a = null;
            target.b = null;
        }

        private static void SortByElementType(List<ActionElementMap> aems, ControllerElementType[] controllerElementTypes, FastList<ActionElementMap> results) {
            results.Clear();
            results.ReplaceFrom(aems);
            SortByElementType(results, controllerElementTypes);
        }
        private static void SortByElementType(FastList<ActionElementMap> aems, ControllerElementType[] controllerElementTypes) {

            FastList<ActionElementMap> tempSorted = aemFastListPool.Get();
            FastList<bool> usedAemIds = GetUsedPooledList(aems.Count);

            int j;
            for (int i = 0; i < controllerElementTypes.Length; i++) {
                for (j = 0; j < aems.Count; j++) {
                    if (aems.Array[j].elementType == controllerElementTypes[i]) {
                        tempSorted.Add(aems.Array[j]);
                        usedAemIds.Array[j] = true;
                    }
                }
            }
            if (tempSorted.Count < aems.Count) { // add any remaining
                for (int i = 0; i < aems.Count; i++) {
                    if (usedAemIds.Array[i]) continue; // already added
                    tempSorted.Add(aems.Array[i]);
                }
            }

            aems.ReplaceFrom(tempSorted);

            aemFastListPool.Return(tempSorted);
            boolFastListPool.Return(usedAemIds);
        }

        private static bool AllowMoreResultsDecrement(ref int remainingCount) {
            if (remainingCount < 0) return true; // allow infinite results
            remainingCount -= 1;
            if (remainingCount < 0) remainingCount = 0;
            return remainingCount > 0;
        }

        private static FastList<bool> GetUsedPooledList(int count) {
            var list = boolFastListPool.Get();
            list.SetCount(count);
            return list;
        }

        private static void ReturnUsedPoolList(FastList<bool> list) {
            boolFastListPool.Return(list);
        }

        #endregion

        #region Classes

        private sealed class DefaultControllerMapCache {

            private static DefaultControllerMapCache s_instance;
            public static DefaultControllerMapCache instance {
                get {
                    if (!ReInput.isReady) return null;
                    return s_instance != null ? s_instance : (s_instance = new DefaultControllerMapCache());
                }
            }

            private readonly List<Entry> _cache;

            private DefaultControllerMapCache() {
                _cache = new List<Entry>();
                ReInput.ShutDownEvent += OnRewiredShutDown;
            }

            private void OnRewiredShutDown() {
                ReInput.ShutDownEvent -= OnRewiredShutDown;
                s_instance = null;
            }

            public ControllerMap GetControllerMap(int playerId, ControllerIdentifier controllerIdentifier, string mapCategoryName, string layoutName) {
                if (!ReInput.isReady) return null;
                int mapCategoryId = ReInput.mapping.GetMapCategoryId(mapCategoryName);
                if (mapCategoryId < 0) return null;
                int layoutId = ReInput.mapping.GetLayoutId(controllerIdentifier.controllerType, layoutName);
                if (layoutId < 0) return null;

                Entry entry;
                int index = IndexOf(playerId, controllerIdentifier, mapCategoryId, layoutId);
                if (index < 0) {
                    entry = new Entry(new Selector(playerId, controllerIdentifier, mapCategoryId, layoutId));
                    _cache.Add(entry);
                } else {
                    entry = _cache[index];
                }

                // Clear cache if more than one frame has passed without being called
                if (!IsEqualOrNextFrame(UnityEngine.Time.frameCount, entry.lastTouchedFrame)) {
                    entry.Clear();
                }

                if (!entry.loaded) {
                    entry.controllerMap = ReInput.mapping.GetControllerMapInstanceSavedOrDefault(playerId, controllerIdentifier, mapCategoryId, layoutId);
                    entry.loaded = true; // mark loaded even if null to prevent further futile attempts to load it
                }

                entry.lastTouchedFrame = UnityEngine.Time.frameCount;
                return entry.controllerMap;
            }

            private int IndexOf(int playerId, ControllerIdentifier controllerIdentifier, int mapCategoryId, int layoutId) {
                int count = _cache.Count;
                Selector selector;
                for (int i = 0; i < count; i++) {
                    selector = _cache[i].selector;
                    if (
                        selector.playerId == playerId &&
                        selector.mapCategoryId == mapCategoryId &&
                        selector.layoutId == layoutId &&
                        selector.controllerIdentifier.controllerType == controllerIdentifier.controllerType &&
                        selector.controllerIdentifier.deviceInstanceGuid == controllerIdentifier.deviceInstanceGuid &&
                        string.Equals(selector.controllerIdentifier.hardwareIdentifier, controllerIdentifier.hardwareIdentifier, System.StringComparison.Ordinal)) {
                        return i;
                    }
                }
                return -1;
            }

            private static bool IsEqualOrNextFrame(int a, int b) {
                if (a == b) return true;
                if (b == int.MaxValue) {
                    return a == 0;
                }
                return a == b + 1;
            }

            private struct Selector {
                public readonly int playerId;
                public readonly ControllerIdentifier controllerIdentifier;
                public readonly int mapCategoryId;
                public readonly int layoutId;

                public Selector(
                    int playerId,
                    ControllerIdentifier controllerIdentifier,
                    int mapCategoryId,
                    int layoutId
                ) {
                    this.playerId = playerId;
                    this.controllerIdentifier = controllerIdentifier;
                    this.mapCategoryId = mapCategoryId;
                    this.layoutId = layoutId;
                }
            }

            private class Entry {
                public readonly Selector selector;
                public bool loaded;
                public ControllerMap controllerMap;
                public int lastTouchedFrame;

                public Entry(
                    Selector selector
                ) {
                    this.selector = selector;
                }

                public void Clear() {
                    loaded = false;
                    controllerMap = null;
                }
            }
        }

        private sealed class FastList<T> {

            private const int minCapacity = 2;

            public T[] Array;
            public int Count;
            public int Capacity;

            public FastList(int startingCapacity) {
                if (startingCapacity < minCapacity) startingCapacity = minCapacity;
                Array = new T[startingCapacity];
                Capacity = startingCapacity;
            }

            public void Add(T item) {
                if (Count >= Capacity) Expand(Capacity * 2);
                Array[Count] = item;
                Count += 1;
            }

            public void RemoveAt(int index) {
                if ((uint)index >= (uint)Count) throw new System.IndexOutOfRangeException();
                int lastIndex = Count - 1;
                for (int i = index; i < lastIndex; i++) {
                    Array[i] = Array[i + 1];
                }
                Array[lastIndex] = default(T);
                Count -= 1;
            }

            public void Expand(int size) {
                if (size <= minCapacity) size = minCapacity;
                if (size <= Capacity) return;
                if (!IsPowerOfTwo((uint)size)) {
                    size = (int)RoundUpToPowerOf2((uint)size);
                }
                T[] newArray = new T[size];
                int min = Capacity < size ? Capacity : size;
                for (int i = 0; i < min; i++) {
                    newArray[i] = Array[i];
                }
                Array = newArray;
                Capacity = Array.Length;
            }

            public void SetCount(int size) {
                if (size < 0) size = 0;
                if (size == Count) return;
                if (size < Count) System.Array.Clear(Array, size, Count - size);
                if (size > Capacity) Expand(size);
                Count = size;
            }

            public void ReplaceFrom(IList<T> source) {
                Clear();
                int count = source.Count;
                Expand(count);
                for (int i = 0; i < count; i++) {
                    Array[i] = source[i];
                }
                this.Count = count;
            }
            public void ReplaceFrom(FastList<T> source) {
                Clear();
                int count = source.Count;
                Expand(count);
                for (int i = 0; i < count; i++) {
                    Array[i] = source.Array[i];
                }
                this.Count = count;
            }

            /*public void ReplaceTo(IList<T> destination) {
                destination.Clear();
                for (int i = 0; i < Count; i++) {
                    destination.Add(Array[i]);
                }
            }*/

            public void Clear() {
                if (Count > 0) System.Array.Clear(Array, 0, Count);
                Count = 0;
            }

            private static uint RoundUpToPowerOf2(uint value) {
                if (value == 0) return 1;
                value--;
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                value++;
                return value;
            }

            private static bool IsPowerOfTwo(uint x) {
                return (x != 0) && ((x & (x - 1)) == 0);
            }
        }

        private sealed class ObjectPool<T> where T : class {

            private readonly List<T> _objects;
            private readonly System.Func<T> _createDelegate;
            private readonly System.Action<T> _onReturnDelegate;

            public ObjectPool(System.Func<T> createDelegate, System.Action<T> onReturnDelegate) {
                _createDelegate = createDelegate;
                _onReturnDelegate = onReturnDelegate;
                _objects = new List<T>();
            }

            public T Get() {
                if (_objects.Count != 0) {
                    int index = _objects.Count - 1;
                    T r = _objects[index];
                    _objects.RemoveAt(index);
                    return r;
                }
                return _createDelegate();
            }

            public void Return(T obj) {
                if (obj == null) return;
                if (_objects.Contains(obj)) return;
                _objects.Add(obj);
                _onReturnDelegate(obj);
            }
        }

        private struct ControllerInfo {

            public ControllerType type;
            public int controllerId;

            public ControllerInfo(
                ControllerType type,
                int controllerId
            ) {
                this.type = type;
                this.controllerId = controllerId;
            }
        }

        #endregion
    }
}
