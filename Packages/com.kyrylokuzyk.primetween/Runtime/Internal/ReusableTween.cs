#if PRIME_TWEEN_SAFETY_CHECKS && UNITY_ASSERTIONS
#define SAFETY_CHECKS
#endif
using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using SerializeField = UnityEngine.SerializeField;
using HideInInspector = UnityEngine.HideInInspector;
using TweenType = PrimeTween.TweenAnimation.TweenType;
using TypeUnion = PrimeTween.TweenAnimation.TypeUnion;

namespace PrimeTween {
    [Serializable]
    internal class ReusableTween {
        #if UNITY_EDITOR
        [SerializeField, HideInInspector] internal string debugDescription;
        [SerializeField, CanBeNull, UsedImplicitly] internal UnityEngine.Object unityTarget;
        #endif
        internal long id = -1;
        /// Holds a reference to tween's target. If the target is UnityEngine.Object, the tween will gracefully stop when the target is destroyed. That is, destroying object with running tweens is perfectly ok.
        /// Keep in mind: when animating plain C# objects (not derived from UnityEngine.Object), the plugin will hold a strong reference to the object for the entire tween duration.
        ///     If the plain C# target holds a reference to UnityEngine.Object and animates its properties, then it's the user's responsibility to ensure that UnityEngine.Object still exists.
        [CanBeNull] internal object target;
        [SerializeField] internal bool _isPaused;
        internal bool _isAlive {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.IsAlive);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.IsAlive, value);
        }
        [SerializeField] internal float elapsedTimeTotal;
        [SerializeField] internal float easedInterpolationFactor;
        internal float cycleDuration;

        [SerializeField] internal ValueContainerStartEnd startEndValue;

        internal PropType propType => Utils.TweenTypeToTweenData(startEndValue.tweenType).Item1;
        internal ref TweenType tweenType => ref startEndValue.tweenType;
        internal ref TypeUnion startValue => ref startEndValue.startValue;
        internal ref TypeUnion endValue => ref startEndValue.endValue;
        internal TypeUnion diff;
        internal bool isAdditive {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.Additive);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.Additive, value);
        }
        internal bool resetBeforeComplete {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.ResetBeforeComplete);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.ResetBeforeComplete, value);
        }
        internal TypeUnion prevVal;
        [SerializeField] internal TweenSettings settings;
        [SerializeField] internal int cyclesDone;
        const int iniCyclesDone = -1;

        internal object customOnValueChange;
        internal long longParam;
        internal int intParam {
            get => (int)longParam;
            set => longParam = value;
        }
        Action<ReusableTween> onValueChange;

        [CanBeNull] internal Action<ReusableTween> onComplete;
        [CanBeNull] object onCompleteCallback;
        [CanBeNull] object onCompleteTarget;

        internal float waitDelay;
        internal Sequence sequence;
        internal Tween prev;
        internal Tween next;
        internal Tween prevSibling;
        internal Tween nextSibling;

        internal Func<ReusableTween, TypeUnion> getter;
        internal ref bool startFromCurrent => ref startEndValue.startFromCurrent;

        bool stoppedEmergently {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.StoppedEmergently);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.StoppedEmergently, value);
        }
        internal readonly TweenCoroutineEnumerator coroutineEnumerator = new TweenCoroutineEnumerator();
        internal float timeScale = 1f;
        bool warnIgnoredOnCompleteIfTargetDestroyed {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.WarnIgnoredOnCompleteIfTargetDestroyed);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.WarnIgnoredOnCompleteIfTargetDestroyed, value);
        }
        internal ShakeData shakeData;
        internal bool shakeSign {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.ShakeSign);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.ShakeSign, value);
        }
        internal bool isPunch {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.ShakePunch);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.ShakePunch, value);
        }
        bool warnEndValueEqualsCurrent {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.WarnEndValueEqualsCurrent);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.WarnEndValueEqualsCurrent, value);
        }

        internal bool updateAndCheckIfRunning(float dt) {
            if (!_isAlive) {
                return sequence.IsCreated; // don't release a tween until sequence.releaseTweens()
            }
            if (!_isPaused) {
                SetElapsedTimeTotal(elapsedTimeTotal + dt * timeScale);
            } else if (isUnityTargetDestroyed()) {
                EmergencyStop(true);
                return false;
            }
            return _isAlive;
        }

        bool isUpdating { // it's not possible to place this check only on calls that come from Tween.Custom() because then it would not be possible to call .Complete() on custom tweens
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetFlag(Flags.IsUpdating);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetFlag(Flags.IsUpdating, value);
        }

        internal void SetElapsedTimeTotal(float newElapsedTimeTotal, bool earlyExitSequenceIfPaused = true) {
            if (isUpdating) {
                Debug.LogError(Constants.recursiveCallError);
                return;
            }
            isUpdating = true;
            if (!sequence.IsCreated) {
                setElapsedTimeTotal(newElapsedTimeTotal, out int cyclesDiff, false);
                if (!stoppedEmergently && _isAlive && isDone(cyclesDiff)) {
                    if (!_isPaused) {
                        kill();
                    }
                    ReportOnComplete();
                }
            } else {
                Assert.IsTrue(sequence.isAlive, id);
                if (isMainSequenceRoot()) {
                    Assert.IsTrue(sequence.root.id == id, id);
                    updateSequence(newElapsedTimeTotal, false, earlyExitSequenceIfPaused, true, false);
                }
            }
            isUpdating = false;
        }

        internal void updateSequence(float _elapsedTimeTotal, bool isRestart, bool earlyExitSequenceIfPaused, bool allowSkipChildrenUpdate, bool invertEase) {
            Assert.IsTrue(isSequenceRoot());
            bool isInvert = startValue.single > 0f && endValue.single == 0f;
            bool isRootBackwardCycle = (clampCyclesDone(cyclesDone) % 2 != 0) ^ isInvert;
            float prevElapsedTime = FloatVal;
            bool invertRootEase = (settings.cycles == 1 && isRootBackwardCycle && settings.cycleMode == CycleMode.Rewind) ^ invertEase;
            if (!setElapsedTimeTotal(_elapsedTimeTotal, out int cyclesDiff, invertRootEase) && allowSkipChildrenUpdate) { // update sequence root
                return;
            }

            if (settings.cycleMode == (CycleMode)_CycleMode.YoyoChildren && isRootBackwardCycle) {
                invertEase = !invertEase;
            }
            bool isRestartToBeginning = isRestart && cyclesDiff < 0;
            Assert.IsTrue(!isRestartToBeginning || cyclesDone == 0 || cyclesDone == iniCyclesDone);
            if (cyclesDiff != 0 && !isRestartToBeginning) {
                // print($"           sequence cyclesDiff: {cyclesDiff}");
                if (isRestart) {
                    Assert.IsTrue(cyclesDiff > 0 && cyclesDone == settings.cycles);
                    cyclesDiff = 1;
                }
                int cyclesDiffAbs = Mathf.Abs(cyclesDiff);
                int newCyclesDone = cyclesDone;
                cyclesDone -= cyclesDiff;
                int cyclesDelta = cyclesDiff > 0 ? 1 : -1;
                float interpolationFactor = cyclesDelta > 0 ? 1f : 0f;
                for (int i = 0; i < cyclesDiffAbs; i++) {
                    Assert.IsTrue(!isRestart || i == 0);
                    if (cyclesDone == settings.cycles || cyclesDone == iniCyclesDone) {
                        // do nothing when moving backward from the last cycle or forward from the -1 cycle
                        cyclesDone += cyclesDelta;
                        continue;
                    }

                    float easedT = calcEasedT(interpolationFactor, cyclesDone, false);
                    bool isForwardCycle = (easedT > 0.5f) ^ isInvert;
                    const float negativeElapsedTime = -1000f;
                    if (!forceChildrenToPos()) {
                        return;
                    }
                    bool forceChildrenToPos() {
                        // complete the previous cycles by forcing all children tweens to 0f or 1f
                        // print($" (i:{i}) force to pos: {isForwardCycle}");
                        float simulatedSequenceElapsedTime = isForwardCycle ? float.MaxValue : negativeElapsedTime;
                        foreach (var t in getSequenceSelfChildren(isForwardCycle)) {
                            var tween = t.tween;
                            tween.updateSequenceChild(simulatedSequenceElapsedTime, isRestart, invertEase);
                            if (isEarlyExitAfterChildUpdate()) {
                                return false;
                            }
                        }
                        return true;
                    }

                    cyclesDone += cyclesDelta;
                    var sequenceCycleMode = settings.cycleMode;
                    if (sequenceCycleMode == CycleMode.Restart && cyclesDone != settings.cycles && cyclesDone != iniCyclesDone) { // '&& cyclesDone != 0' check is wrong because we should do the restart when moving from 1 to 0 cyclesDone
                        if (!restartChildren()) {
                            return;
                        }
                        bool restartChildren() {
                            // print($"restart to pos: {!isForwardCycle}");
                            var simulatedSequenceElapsedTime = !isForwardCycle ? float.MaxValue : negativeElapsedTime;
                            prevElapsedTime = simulatedSequenceElapsedTime;
                            foreach (var t in getSequenceSelfChildren(!isForwardCycle)) {
                                var tween = t.tween;
                                tween.updateSequenceChild(simulatedSequenceElapsedTime, true, invertEase);
                                if (isEarlyExitAfterChildUpdate()) {
                                    return false;
                                }
                                Assert.IsTrue(isForwardCycle || tween.cyclesDone == tween.settings.cycles, id);
                                Assert.IsTrue(!isForwardCycle || tween.cyclesDone <= 0, id);
                                Assert.IsTrue(isForwardCycle || tween.GetFlag(Flags.StateAfter), id);
                                Assert.IsTrue(!isForwardCycle || tween.GetFlag(Flags.StateBefore), id);
                            }
                            return true;
                        }
                    }
                }
                Assert.IsTrue(newCyclesDone == cyclesDone, id);
                if (isDone(cyclesDiff)) {
                    if (resetBeforeComplete && isMainSequenceRoot()) {
                        // reset Sequence
                        foreach (var t in getSequenceSelfChildren(false)) {
                            t.tween.updateSequenceChild(0f, true, invertEase);
                            if (isEarlyExitAfterChildUpdate()) {
                                goto EarlyExit;
                            }
                        }
                        EarlyExit:;
                    }
                    if (isMainSequenceRoot() && !_isPaused) {
                        sequence.releaseTweens();
                    }
                    ReportOnComplete();
                    return;
                }
            }

            float sequenceElapsedTime = Mathf.Clamp(FloatVal, 0f, cycleDuration);
            bool isForward = sequenceElapsedTime > prevElapsedTime;
            foreach (Tween t in getSequenceSelfChildren(isForward)) {
                t.tween.updateSequenceChild(sequenceElapsedTime, isRestart, invertEase);
                if (isEarlyExitAfterChildUpdate()) {
                    return;
                }
            }

            bool isEarlyExitAfterChildUpdate() {
                if (!sequence.isAlive) {
                    return true;
                }
                return earlyExitSequenceIfPaused && sequence.root.tween._isPaused; // access isPaused via root tween to bypass the cantManipulateNested check
            }
        }

        Sequence.SequenceDirectEnumerator getSequenceSelfChildren(bool isForward) {
            Assert.IsTrue(sequence.isAlive, id);
            return sequence.getSelfChildren(isForward);
        }

        bool isDone(int cyclesDiff) {
            Assert.IsTrue(settings.cycles == -1 || cyclesDone <= settings.cycles);
            if (timeScale > 0f) {
                return cyclesDiff > 0 && cyclesDone == settings.cycles;
            } else {
                return cyclesDiff < 0 && cyclesDone == iniCyclesDone;
            }
        }

        internal void updateSequenceChild(float encompassingElapsedTime, bool isRestart, bool invertEase) {
            if (isSequenceRoot()) {
                updateSequence(encompassingElapsedTime, isRestart, true, true, invertEase);
            } else {
                setElapsedTimeTotal(encompassingElapsedTime, out int cyclesDiff, invertEase);
                if (!stoppedEmergently && _isAlive && isDone(cyclesDiff)) {
                    ReportOnComplete();
                }
            }
        }

        internal bool isMainSequenceRoot() => tweenType == TweenType.MainSequence;
        internal bool isSequenceRoot() => tweenType == TweenType.MainSequence || tweenType == TweenType.NestedSequence;

        bool setElapsedTimeTotal(float _elapsedTimeTotal, out int cyclesDiff, bool invertEase) {
            elapsedTimeTotal = _elapsedTimeTotal;
            int oldCyclesDone = cyclesDone;
            float t = calcTFromElapsedTimeTotal(_elapsedTimeTotal, out var newState);
            cyclesDiff = cyclesDone - oldCyclesDone;
            if (newState == Flags.StateRunning || (flags & newState) == 0) {
                if (isUnityTargetDestroyed()) {
                    EmergencyStop(true);
                    return false;
                }
                float easedT = calcEasedT(t, cyclesDone, invertEase);
                // print($"state: {flags}/{newState}, cycles: {cyclesDone}/{settings.cycles} (diff: {cyclesDiff}), elapsedTimeTotal: {elapsedTimeTotal}, interpolation: {t}/{easedT}");
                flags &= ~(Flags.StateAfter | Flags.StateBefore | Flags.StateRunning);
                flags |= newState;
                ReportOnValueChange(easedT);
                return true;
            }
            return false;
        }

        float calcTFromElapsedTimeTotal(float _elapsedTimeTotal, out Flags newState) {
            // key timeline points: 0 | startDelay | duration | 1 | endDelay | onComplete
            int cyclesTotal = settings.cycles;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_elapsedTimeTotal == float.MaxValue) {
                Assert.AreNotEqual(-1, cyclesTotal);
                Assert.IsTrue(cyclesDone <= cyclesTotal);
                cyclesDone = cyclesTotal;
                newState = Flags.StateAfter;
                return 1f;
            }
            _elapsedTimeTotal -= waitDelay; // waitDelay is applied before calculating cycles
            if (_elapsedTimeTotal < 0f) {
                cyclesDone = iniCyclesDone;
                newState = Flags.StateBefore;
                return 0f;
            }
            Assert.IsTrue(_elapsedTimeTotal >= 0f);
            Assert.AreNotEqual(float.MaxValue, _elapsedTimeTotal);
            if (cycleDuration == 0f) {
                if (cyclesTotal == -1) {
                    // add max one cycle per frame
                    if (timeScale > 0f) {
                        if (cyclesDone == iniCyclesDone) {
                            cyclesDone = 1;
                        } else {
                            cyclesDone++;
                        }
                    } else if (timeScale != 0f) {
                        cyclesDone--;
                        if (cyclesDone == iniCyclesDone) {
                            newState = Flags.StateBefore;
                            return 0f;
                        }
                    }
                    newState = Flags.StateRunning;
                    return 1f;
                }
                Assert.AreNotEqual(-1, cyclesTotal);
                if (_elapsedTimeTotal == 0f) {
                    cyclesDone = iniCyclesDone;
                    newState = Flags.StateBefore;
                    return 0f;
                }
                Assert.IsTrue(cyclesDone <= cyclesTotal);
                cyclesDone = cyclesTotal;
                newState = Flags.StateAfter;
                return 1f;
            }
            Assert.AreNotEqual(0f, cycleDuration);
            cyclesDone = (int) (_elapsedTimeTotal / cycleDuration);
            if (cyclesTotal != -1 && cyclesDone > cyclesTotal) {
                cyclesDone = cyclesTotal;
            }
            if (cyclesTotal != -1 && cyclesDone == cyclesTotal) {
                newState = Flags.StateAfter;
                return 1f;
            }
            float elapsedTimeInCycle = _elapsedTimeTotal - cycleDuration * cyclesDone - settings.startDelay;
            if (elapsedTimeInCycle < 0f) {
                newState = Flags.StateBefore;
                return 0f;
            }
            Assert.IsTrue(elapsedTimeInCycle >= 0f);
            float animationDuration = settings.duration; // duration without startDelay and endDelay
            if (animationDuration == 0f) {
                newState = Flags.StateAfter;
                return 1f;
            }
            Assert.AreNotEqual(0f, animationDuration);
            float result = elapsedTimeInCycle / animationDuration;
            if (result > 1f) {
                newState = Flags.StateAfter;
                return 1f;
            }
            newState = Flags.StateRunning;
            Assert.IsTrue(result >= 0f);
            return result;
        }

        // void print(string msg) => Debug.Log($"[{Time.frameCount}]  id {id}  {msg}");

        internal void Reset() {
            Assert.IsFalse(isUpdating);
            Assert.IsFalse(_isAlive);
            Assert.IsFalse(sequence.IsCreated);
            Assert.IsFalse(prev.IsCreated);
            Assert.IsFalse(next.IsCreated);
            Assert.IsFalse(prevSibling.IsCreated);
            Assert.IsFalse(nextSibling.IsCreated);
            Assert.IsFalse(IsInSequence());
            if (shakeData.isAlive) {
                shakeData.Reset(this);
            }
            #if UNITY_EDITOR
            debugDescription = null;
            unityTarget = null;
            #endif
            id = -1;
            target = null;
            settings.customEase = null;
            customOnValueChange = null;
            onValueChange = null;
            onComplete = null;
            onCompleteCallback = null;
            onCompleteTarget = null;
            getter = null;
            stoppedEmergently = false;
            waitDelay = 0f;
            coroutineEnumerator.resetEnumerator();
            tweenType = TweenType.Disabled;
            timeScale = 1f;
            warnIgnoredOnCompleteIfTargetDestroyed = true;
            clearOnUpdate();
            resetBeforeComplete = false;
        }

        /// <param name="warnIfTargetDestroyed">https://github.com/KyryloKuzyk/PrimeTween/discussions/4</param>
        internal void OnComplete([CanBeNull] Action _onComplete, bool warnIfTargetDestroyed) {
            if (_onComplete == null) {
                return;
            }
            validateOnCompleteAssignment();
            warnIgnoredOnCompleteIfTargetDestroyed = warnIfTargetDestroyed;
            onCompleteCallback = _onComplete;
            onComplete = tween => {
                var callback = tween.onCompleteCallback as Action;
                Assert.IsNotNull(callback);
                try {
                    callback();
                } catch (Exception e) {
                    tween.handleOnCompleteException(e);
                }
            };
        }

        internal void OnComplete<T>([CanBeNull] T _target, [CanBeNull] Action<T> _onComplete, bool warnIfTargetDestroyed) where T : class {
            if (_target == null || isDestroyedUnityObject(_target)) {
                Debug.LogError($"{nameof(_target)} is null or has been destroyed. {Constants.onCompleteCallbackIgnored}");
                return;
            }
            if (_onComplete == null) {
                return;
            }
            validateOnCompleteAssignment();
            warnIgnoredOnCompleteIfTargetDestroyed = warnIfTargetDestroyed;
            onCompleteTarget = _target;
            onCompleteCallback = _onComplete;
            onComplete = tween => {
                var callback = tween.onCompleteCallback as Action<T>;
                Assert.IsNotNull(callback);
                var _onCompleteTarget = tween.onCompleteTarget as T;
                if (isDestroyedUnityObject(_onCompleteTarget)) {
                    tween.warnOnCompleteIgnored(true);
                    return;
                }
                try {
                    callback(_onCompleteTarget);
                } catch (Exception e) {
                    tween.handleOnCompleteException(e);
                }
            };
        }

        void handleOnCompleteException(Exception e) {
            // Design decision: if a tween is inside a Sequence and user's tween.OnComplete() throws an exception, the Sequence should continue
            Assert.LogError($"Tween's onComplete callback raised exception, tween: {GetDescription()}, exception:\n{e}\n", id, target as UnityEngine.Object);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        static bool isDestroyedUnityObject<T>(T obj) where T: class => obj is UnityEngine.Object unityObject && unityObject == null;

        void validateOnCompleteAssignment() {
            const string msg = "Tween already has an onComplete callback. Adding more callbacks is not allowed.\n" +
                               "Workaround: wrap a tween in a Sequence by calling Sequence.Create(tween) and use multiple ChainCallback().\n";
            Assert.IsNull(onCompleteTarget, msg);
            Assert.IsNull(onCompleteCallback, msg);
            Assert.IsNull(onComplete, msg);
        }

        /// _getter is null for custom tweens
        internal void Setup([CanBeNull] object _target, ref TweenSettings _settings, [NotNull] Action<ReusableTween> _onValueChange, [CanBeNull] Func<ReusableTween, TypeUnion> _getter, bool _startFromCurrent, TweenType _tweenType) {
            Assert.IsTrue(_settings.cycles >= -1);
            Assert.IsNotNull(_onValueChange);
            Assert.IsNull(getter);
            Assert.AreNotEqual(TweenType.Disabled, _tweenType);;
            tweenType = _tweenType;
            var propertyType = propType;
            Assert.AreNotEqual(PropType.None, propertyType);
            if (_settings.ease == Ease.Default) {
                _settings.ease = PrimeTweenManager.Instance.defaultEase;
            } else if (_settings.ease == Ease.Custom && _settings.parametricEase == ParametricEase.None) {
                if (_settings.customEase == null || !TweenSettings.ValidateCustomCurveKeyframes(_settings.customEase)) {
                    Debug.LogError($"Ease type is Ease.Custom, but {nameof(TweenSettings.customEase)} is not configured correctly.");
                    _settings.ease = PrimeTweenManager.Instance.defaultEase;
                }
            }
            flags &= ~(Flags.StateAfter | Flags.StateRunning);
            flags |= Flags.StateBefore;
            target = _target;
            setUnityTarget(_target);
            elapsedTimeTotal = 0f;
            easedInterpolationFactor = float.MinValue;
            _isPaused = false;
            revive();

            cyclesDone = iniCyclesDone;
            _settings.SetValidValues();
            settings.CopyFrom(ref _settings); // p2 todo log warning if cyclesTotal == 1 && cycleMode != CycleModeInternal.Restart
            calculateCycleDuration();
            Assert.IsTrue(cycleDuration >= 0);
            onValueChange = _onValueChange;
            Assert.IsFalse(_startFromCurrent && _getter == null);
            startFromCurrent = _startFromCurrent;
            getter = _getter;
            if (!_startFromCurrent) {
                cacheDiff();
            }
            if (propertyType == PropType.Quaternion) {
                // Quaternion.identity
                prevVal.x = prevVal.y = prevVal.z = 0f;
                prevVal.w = 1f;
            } else {
                prevVal.Reset();
            }
            warnEndValueEqualsCurrent = PrimeTweenManager.Instance.warnEndValueEqualsCurrent;
        }

        internal void setUnityTarget(object _target) {
            #if UNITY_EDITOR
            unityTarget = _target as UnityEngine.Object;
            #endif
        }

        /// Tween.Custom and Tween.ShakeCustom try-catch the <see cref="onValueChange"/> and calls <see cref="ReusableTween.EmergencyStop"/> if an exception occurs.
        /// <see cref="ReusableTween.EmergencyStop"/> sets <see cref="stoppedEmergently"/> to true.
        internal void ReportOnValueChange(float _easedInterpolationFactor) {
            // Debug.Log($"id {id}, ReportOnValueChange {_easedInterpolationFactor}");
            Assert.IsFalse(isUnityTargetDestroyed());
            if (startFromCurrent) {
                startFromCurrent = false;
                if (!ShakeData.TryTakeStartValueFromOtherShake(this)) {
                    startValue = getter(this);
                }
                if (startValue.vector4 == endValue.vector4 && warnEndValueEqualsCurrent && !shakeData.isAlive) {
                    Assert.LogWarning($"Tween's 'endValue' equals to the current animated value: {startValue.vector4}, tween: {GetDescription()}.\n" +
                                      $"{Constants.buildWarningCanBeDisabledMessage(nameof(PrimeTweenConfig.warnEndValueEqualsCurrent))}\n", id);
                }
                cacheDiff();
            }
            easedInterpolationFactor = _easedInterpolationFactor;
            try {
                // The value setter can fail even if the Unity target is not destroyed. For example, ScrollRect.SetNormalizedPosition can throw null ref if m_Content is not populated: https://github.com/needle-mirror/com.unity.ugui/blob/a601a2bf30161c47959231b627a8c40f64d69a68/Runtime/UI/Core/ScrollRect.cs#L1031.
                // Also, this try-catch catches exceptions in user-provided setter callbacks in Custom tweens.
                onValueChange(this);
            } catch (Exception e) {
                Debug.LogException(e, target as UnityEngine.Object);
                Assert.LogWarning($"Tween was stopped because of exception in '{nameof(onValueChange)}', tween: {GetDescription()}\n", id, target as UnityEngine.Object);
                EmergencyStop();
            }
            if (stoppedEmergently || !_isAlive) {
                return;
            }
            onUpdate?.Invoke(this);
        }

        void ReportOnComplete() {
            // Debug.Log($"[{Time.frameCount}] id {id} ReportOnComplete() {easedInterpolationFactor}");
            Assert.IsFalse(startFromCurrent);
            Assert.IsTrue(timeScale < 0 || cyclesDone == settings.cycles);
            Assert.IsTrue(timeScale >= 0 || cyclesDone == iniCyclesDone);
            if (resetBeforeComplete && !sequence.IsCreated) {
                // reset Tween
                setElapsedTimeTotal(0f, out _, false);
            }
            onComplete?.Invoke(this);
        }

        internal bool isUnityTargetDestroyed() {
            // must use target here instead of unityTarget
            // unityTarget has the SerializeField attribute, so if ReferenceEquals(unityTarget, null), then Unity will populate the field with non-null UnityEngine.Object when a new scene is loaded in the Editor
            // https://github.com/KyryloKuzyk/PrimeTween/issues/32
            return isDestroyedUnityObject(target);
        }

        internal bool HasOnComplete => onComplete != null;
        readonly System.Text.StringBuilder _sb = new System.Text.StringBuilder();

        [NotNull]
        internal string GetDescription() {
            _sb.Clear();
            if (!_isAlive) {
                _sb.Append(" - ");
            }
        
            if (sequence.IsCreated) {
                var currentSequence = sequence;
                while (true) {
                    if (id != currentSequence.root.id) {
                        _sb.Append(" · ");
                    }
                    var _prev = currentSequence.root.tween.prev;
                    if (!_prev.IsCreated) {
                        break;
                    }
                    var parent = _prev.tween.sequence;
                    if (!parent.IsCreated) {
                        break;
                    }
                    currentSequence = parent;
                }
            }
            float duration = settings.duration;
            bool isCallback = false;
            if (tweenType == TweenType.Delay) {
                if (duration == 0f && onComplete != null) {
                    isCallback = true;
                    _sb.Append("Callback");
                } else {
                    _sb.Append("Delay");
                }
            } else {
                if (tweenType == TweenType.MainSequence || tweenType == TweenType.NestedSequence) {
                    _sb.Append("Sequence ");
                } else {
                    _sb.Append(tweenType);
                }
            }
            if (target != PrimeTweenManager.dummyTarget) {
                _sb.Append(" / ");
                _sb.Append(target is UnityEngine.Object unityObject && unityObject != null ? unityObject.name : target?.GetType().Name);
            }
            if (!isCallback) {
                _sb.Append(" / ").AppendFormat("{0:F2}", duration);
            }
            return _sb.ToString();
        }

        internal float calcDurationWithWaitDependencies() {
            var cycles = settings.cycles;
            Assert.AreNotEqual(-1, cycles, "It's impossible to calculate the duration of an infinite tween (cycles == -1).");
            Assert.AreNotEqual(0, cycles);
            return waitDelay + cycleDuration * cycles;
        }

        internal void calculateCycleDuration() {
            cycleDuration = settings.startDelay + settings.duration + settings.endDelay;
        }

        internal float FloatVal => startValue.x + diff.x * easedInterpolationFactor;
        internal double DoubleVal => startValue.DoubleVal + diff.DoubleVal * easedInterpolationFactor;
        internal Vector2 Vector2Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector2(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT);
            }
        }
        internal Vector3 Vector3Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector3(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT);
            }
        }
        internal Vector4 Vector4Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector4(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Color ColorVal {
            get {
                var easedT = easedInterpolationFactor;
                return new Color(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Rect RectVal {
            get {
                var easedT = easedInterpolationFactor;
                return new Rect(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Quaternion QuaternionVal => Quaternion.SlerpUnclamped(startValue.quaternion, endValue.quaternion, easedInterpolationFactor);

        float calcEasedT(float t, int cyclesDone_, bool invertEase) {
            return invertEase ?
                1f - calcEasedTInternal(1f - t, cyclesDone_) :
                calcEasedTInternal(t, cyclesDone_);
        }

        float calcEasedTInternal(float t, int cyclesDone_) {
            switch (settings.cycleMode) {
                case CycleMode.Restart:
                    return evaluate(t);
                case CycleMode.Incremental:
                    return evaluate(t) + clampCyclesDone(cyclesDone_);
                case (CycleMode)_CycleMode.YoyoChildren:
                case CycleMode.Yoyo: {
                    bool isForwardCycle = clampCyclesDone(cyclesDone_) % 2 == 0;
                    return isForwardCycle ? evaluate(t) : 1 - evaluate(t);
                }
                case CycleMode.Rewind: {
                    bool isForwardCycle = clampCyclesDone(cyclesDone_) % 2 == 0;
                    return isForwardCycle ? evaluate(t) : evaluate(1 - t);
                }
                default:
                    throw new Exception();
            }
        }

        int clampCyclesDone(int cyclesDone_) {
            if (cyclesDone_ == iniCyclesDone) {
                return 0;
            }
            int cyclesTotal = settings.cycles;
            if (cyclesDone_ == cyclesTotal) {
                Assert.AreNotEqual(-1, cyclesTotal);
                return cyclesTotal - 1;
            }
            return cyclesDone_;
        }

        float evaluate(float t) {
            if (settings.ease == Ease.Custom) {
                if (settings.parametricEase != ParametricEase.None) {
                    return Easing.EvaluateParametricEase(t, this);
                }
                return settings.customEase.Evaluate(t);
            }
            return StandardEasing.Evaluate(t, settings.ease);
        }

        internal void cacheDiff() {
            Assert.IsFalse(startFromCurrent);
            var propertyType = propType;
            Assert.AreNotEqual(PropType.None, propertyType);
            switch (propertyType) {
                case PropType.Quaternion:
                    startValue.QuaternionNormalize();
                    endValue.QuaternionNormalize();
                    break;
                case PropType.Double:
                    diff.DoubleVal = endValue.DoubleVal - startValue.DoubleVal;
                    diff.z = 0;
                    diff.w = 0;
                    break;
                default:
                    diff.x = endValue.x - startValue.x;
                    diff.y = endValue.y - startValue.y;
                    diff.z = endValue.z - startValue.z;
                    diff.w = endValue.w - startValue.w;
                    break;
            }
        }

        internal void ForceComplete() {
            Assert.IsFalse(sequence.IsCreated);
            kill(); // protects from recursive call
            if (isUnityTargetDestroyed()) {
                warnOnCompleteIgnored(true);
                return;
            }
            var cyclesTotal = settings.cycles;
            if (cyclesTotal == -1) {
                // same as SetRemainingCycles(1)
                cyclesTotal = getCyclesDone() + 1;
                settings.cycles = cyclesTotal;
            }
            cyclesDone = cyclesTotal;
            ReportOnValueChange(calcEasedT(1f, cyclesTotal, false));
            if (stoppedEmergently) {
                return;
            }
            ReportOnComplete();
            Assert.IsFalse(_isAlive);
        }

        internal void warnOnCompleteIgnored(bool isTargetDestroyed) {
            if (HasOnComplete && warnIgnoredOnCompleteIfTargetDestroyed) {
                onComplete = null;
                var msg = $"{Constants.onCompleteCallbackIgnored} Tween: {GetDescription()}.\n";
                if (isTargetDestroyed) {
                    msg += "\nIf you use tween.OnComplete(), Tween.Delay(), or sequence.ChainDelay() only for cosmetic purposes, you can turn off this error by passing 'warnIfTargetDestroyed: false' to the method.\n" +
                           "More info: https://github.com/KyryloKuzyk/PrimeTween/discussions/4\n";
                }
                Assert.LogError(msg, id, target as UnityEngine.Object);
            }
        }

        internal void EmergencyStop(bool isTargetDestroyed = false) {
            if (sequence.IsCreated) {
                var mainSequence = sequence;
                while (true) {
                    var _prev = mainSequence.root.tween.prev;
                    if (!_prev.IsCreated) {
                        break;
                    }
                    var parent = _prev.tween.sequence;
                    if (!parent.IsCreated) {
                        break;
                    }
                    mainSequence = parent;
                }
                Assert.IsTrue(mainSequence.isAlive);
                Assert.IsTrue(mainSequence.root.tween.isMainSequenceRoot());
                mainSequence.emergencyStop();
            } else if (_isAlive) {
                // EmergencyStop() can be called after ForceComplete() and a caught exception in Tween.Custom()
                kill();
            }
            stoppedEmergently = true;
            warnOnCompleteIgnored(isTargetDestroyed);
            Assert.IsFalse(_isAlive);
            Assert.IsFalse(sequence.isAlive);
        }

        internal void kill() {
            // print($"kill {GetDescription()}");
            Assert.IsTrue(_isAlive);
            _isAlive = false;
            #if UNITY_EDITOR
            debugDescription = null;
            #endif
        }

        void revive() {
            // print($"revive {GetDescription()}");
            Assert.IsFalse(_isAlive);
            _isAlive = true;
            #if UNITY_EDITOR
            debugDescription = null;
            #endif
        }

        internal bool IsInSequence() {
            var result = sequence.IsCreated;
            Assert.IsTrue(result || !nextSibling.IsCreated);
            return result;
        }

        internal bool canManipulate() => !IsInSequence() || isMainSequenceRoot();

        internal bool trySetPause(bool isPaused) {
            if (_isPaused == isPaused) {
                return false;
            }
            _isPaused = isPaused;
            return true;
        }

        [CanBeNull] object onUpdateTarget;
        object onUpdateCallback;
        Action<ReusableTween> onUpdate;

        internal void SetOnUpdate<T>(T _target, [NotNull] Action<T, Tween> _onUpdate) where T : class {
            Assert.IsNull(onUpdate, "Only one OnUpdate() is allowed for one tween.");
            Assert.IsNotNull(_onUpdate, nameof(_onUpdate) + " is null!");
            onUpdateTarget = _target;
            onUpdateCallback = _onUpdate;
            onUpdate = reusableTween => reusableTween.invokeOnUpdate<T>();
        }

        void invokeOnUpdate<T>() where T : class {
            var callback = onUpdateCallback as Action<T, Tween>;
            Assert.IsNotNull(callback);
            var _onUpdateTarget = onUpdateTarget as T;
            if (isDestroyedUnityObject(_onUpdateTarget)) {
                Assert.LogError($"OnUpdate() will not be called again because OnUpdate()'s target has been destroyed, tween: {GetDescription()}", id, target as UnityEngine.Object);
                clearOnUpdate();
                return;
            }
            try {
                callback(_onUpdateTarget, new Tween(this));
            } catch (Exception e) {
                Assert.LogError($"OnUpdate() will not be called again because it thrown exception, tween: {GetDescription()}, exception:\n{e}", id, target as UnityEngine.Object);
                clearOnUpdate();
            }
        }

        void clearOnUpdate() {
            onUpdateTarget = null;
            onUpdateCallback = null;
            onUpdate = null;
        }

        public override string ToString() {
            return GetDescription();
        }

        internal int getCyclesDone() {
            int result = cyclesDone;
            if (result == iniCyclesDone) {
                return 0;
            }
            Assert.IsTrue(result >= 0);
            return result;
        }

        [Flags]
        internal enum Flags : ushort {
            Additive = 1 << 0,
            ShakeSign = 1 << 1,
            ShakePunch = 1 << 2,
            WarnEndValueEqualsCurrent = 1 << 3,
            WarnIgnoredOnCompleteIfTargetDestroyed = 1 << 4,
            ResetBeforeComplete = 1 << 5,
            IsUpdating = 1 << 6,
            StoppedEmergently = 1 << 7,
            IsAlive = 1 << 8,
            StateBefore = 1 << 9,
            StateRunning = 1 << 10,
            StateAfter = 1 << 11
        }
        Flags flags = Flags.WarnIgnoredOnCompleteIfTargetDestroyed;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] bool GetFlag(Flags flag) => (flags & flag) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] internal void SetFlag(Flags flag, bool value) {
            if (value) {
                flags |= flag;
            } else {
                flags &= ~flag;
            }
        }
    }

    internal enum _CycleMode : byte {
        Restart = 0,
        Yoyo = 1,
        Incremental = 2,
        Rewind = 3,
        YoyoChildren = 4
    }
}
