using System.Collections.Generic;
using UniVRM10;
using UnityEngine;

namespace Genesis
{
    // Local preview only. Does not alter the imported model or access player saves.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(Vrm10Instance))]
    [DefaultExecutionOrder(10000)]
    public sealed class GenesisVrmIdle : MonoBehaviour
    {
        private static readonly ExpressionPreset[] Moods =
        {
            ExpressionPreset.happy, ExpressionPreset.relaxed,
            ExpressionPreset.sad, ExpressionPreset.angry, ExpressionPreset.surprised
        };

        private readonly Dictionary<Transform, Quaternion> rest = new Dictionary<Transform, Quaternion>();
        private readonly Dictionary<ExpressionKey, float> previousWeights = new Dictionary<ExpressionKey, float>();
        private Vrm10Instance vrm;
        private Transform head, chest, leftArm, rightArm, leftForearm, rightForearm;
        private Transform hips, spine, leftThigh, rightThigh, leftShin, rightShin, leftFoot, rightFoot, leftHand, rightHand;
        private Vector3 restPosition;
        private Quaternion leftClapBasis = Quaternion.identity, rightClapBasis = Quaternion.identity;
        private float tapStarted = -10;
        private float tapUntil = -10;
        public const float TapReactionSeconds = 1.6f;
        public const float ApplauseClapsPerSecond = 2.5f;
        private const float ApplauseBlendSeconds = 0.22f;
        public float TapReactionRemaining => Mathf.Max(0, tapUntil - elapsed);
        private Vrm10Instance.UpdateTypes previousUpdateType;
        private float elapsed;
        private bool initialized;
        public string Mood { get; private set; } = "Neutral";
        public bool ShowControls = true;
        public bool IsReady => initialized;
        public bool Storytelling { get; private set; }
        public bool Speaking { get; private set; }
        public string AffectionGesture { get; private set; } = "idle";
        private float affectionWeight;
        public const float EmotionReactionSeconds = 6f;
        private float emotionStarted = -10;
        public string EmotionReaction { get; private set; } = "Neutral";
        public float EmotionReactionRemaining => Mathf.Max(0, EmotionReactionSeconds - (elapsed - emotionStarted));

        public bool PlayEmotionReaction(string mood)
        {
            if (!SetMood(mood) || mood == "Neutral") return false;
            EmotionReaction = mood;
            emotionStarted = elapsed;
            tapStarted = -10;
            tapUntil = -10;
            return true;
        }

        public void SetAffection(string gesture, float weight)
        {
            AffectionGesture = gesture == "kiss" || gesture == "hug" ? gesture : "idle";
            affectionWeight = Mathf.Clamp01(weight);
        }

        public void SetStorytelling(bool active, bool speaking)
        {
            if (active) tapUntil = -10;
            Storytelling = active;
            Speaking = active && speaking;
        }

        private void Start() { Initialize(); }

        private void OnEnable()
        {
            if (vrm != null) Initialize();
        }

        private void Initialize()
        {
            if (initialized) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            var hair = GetComponent<GenesisAndroidHair>();
            if (hair == null) hair = gameObject.AddComponent<GenesisAndroidHair>();
            hair.Apply();
#endif
            Animator animator = GetComponent<Animator>();
            vrm = GetComponent<Vrm10Instance>();
            if (animator.avatar == null || !animator.isHuman || animator.runtimeAnimatorController != null)
            {
                Debug.LogWarning("Zenda idle preview requires a Humanoid avatar without an animation controller.", this);
                enabled = false;
                return;
            }
            head = CaptureBone(animator, HumanBodyBones.Head);
            chest = CaptureBone(animator, HumanBodyBones.Chest);
            leftArm = CaptureBone(animator, HumanBodyBones.LeftUpperArm);
            rightArm = CaptureBone(animator, HumanBodyBones.RightUpperArm);
            leftForearm = CaptureBone(animator, HumanBodyBones.LeftLowerArm);
            rightForearm = CaptureBone(animator, HumanBodyBones.RightLowerArm);
            hips = CaptureBone(animator, HumanBodyBones.Hips);
            spine = CaptureBone(animator, HumanBodyBones.Spine);
            leftThigh = CaptureBone(animator, HumanBodyBones.LeftUpperLeg);
            rightThigh = CaptureBone(animator, HumanBodyBones.RightUpperLeg);
            leftShin = CaptureBone(animator, HumanBodyBones.LeftLowerLeg);
            rightShin = CaptureBone(animator, HumanBodyBones.RightLowerLeg);
            leftFoot = CaptureBone(animator, HumanBodyBones.LeftFoot);
            rightFoot = CaptureBone(animator, HumanBodyBones.RightFoot);
            leftHand = CaptureBone(animator, HumanBodyBones.LeftHand);
            rightHand = CaptureBone(animator, HumanBodyBones.RightHand);
            leftClapBasis = CaptureClapBasis(animator, leftHand, HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftLittleProximal);
            rightClapBasis = CaptureClapBasis(animator, rightHand, HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightIndexProximal, HumanBodyBones.RightLittleProximal);
            restPosition = transform.localPosition;
            tapStarted = -10;
            tapUntil = -10;
            previousWeights.Clear();
            foreach (ExpressionKey key in vrm.Runtime.Expression.ExpressionKeys)
                previousWeights[key] = vrm.Runtime.Expression.GetWeight(key);
            previousUpdateType = vrm.UpdateType;
            // Pose and expression input must precede UniVRM's constraints and spring bones.
            vrm.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate;
            elapsed = 0;
            Mood = "Neutral";
            initialized = true;
        }

        private Transform CaptureBone(Animator animator, HumanBodyBones bone)
        {
            Transform result = vrm.Runtime.ControlRig != null ? vrm.Runtime.ControlRig.GetBoneTransform(bone) : animator.GetBoneTransform(bone);
            if (result != null) rest[result] = result.localRotation;
            return result;
        }

        public bool PlayTapReaction()
        {
            if (!initialized || Storytelling) return false;
            emotionStarted = -10;
            // Extend one performance, never queue reactions or reset the clap phase.
            if (elapsed >= tapUntil) tapStarted = elapsed;
            tapUntil = elapsed + TapReactionSeconds;
            SetMood("happy");
            return true;
        }

        private Quaternion CaptureClapBasis(Animator animator, Transform hand, HumanBodyBones middle,
            HumanBodyBones index, HumanBodyBones little)
        {
            // Imported wrists need not share the normalized rig's axes. Recover
            // anatomical axes so fingers point up and thumb edges face the body.
            Transform middleBone = CaptureBone(animator, middle);
            Transform indexBone = CaptureBone(animator, index);
            Transform littleBone = CaptureBone(animator, little);
            if (hand == null || middleBone == null || indexBone == null || littleBone == null)
                return Quaternion.identity;
            Vector3 fingers = hand.InverseTransformDirection(middleBone.position - hand.position);
            Vector3 knuckles = hand.InverseTransformDirection(indexBone.position - littleBone.position);
            knuckles = Vector3.ProjectOnPlane(knuckles, fingers);
            if (fingers.sqrMagnitude < 0.000001f || knuckles.sqrMagnitude < 0.000001f)
                return Quaternion.identity;
            return Quaternion.Inverse(Quaternion.LookRotation(fingers, knuckles));
        }

        public bool SetMood(string mood)
        {
            if (!initialized) return false;
            if (mood == "Neutral") { Mood = mood; return true; }
            foreach (ExpressionPreset preset in Moods)
            {
                if (preset.ToString() != mood) continue;
                if (!previousWeights.ContainsKey(ExpressionKey.CreateFromPreset(preset))) return false;
                Mood = mood;
                return true;
            }
            return false;
        }

        private void LateUpdate()
        {
            elapsed += Time.unscaledDeltaTime;
            Sample(elapsed);
        }

        // Absolute-time sampling makes preview motion deterministic and prevents rotation drift.
        public void Sample(float seconds)
        {
            if (!initialized || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            // Every sample starts from the captured rig, including legs and wrists.
            // This makes interrupted reactions and out-of-order preview samples drift-free.
            foreach (KeyValuePair<Transform, Quaternion> pose in rest)
                if (pose.Key != null) pose.Key.localRotation = pose.Value;
            float phase = Mathf.Repeat(Mathf.Max(0, seconds), 120f * Mathf.PI);
            float gesture = Storytelling ? Mathf.Sin(phase * 1.8f) : 0;
            transform.localPosition = restPosition + Vector3.up * (Mathf.Sin(phase * 1.6f) * 0.012f);
            if (head != null)
                head.localRotation = rest[head] * Quaternion.Euler(Mathf.Sin(phase * 0.8f) * 3f + gesture * 2, Mathf.Sin(phase * 0.5f) * (Storytelling ? 2 : 6), 0);
            if (chest != null)
                chest.localRotation = rest[chest] * Quaternion.Euler(Mathf.Sin(phase * 1.5f) * 2f, 0, Mathf.Sin(phase * 0.7f) * 2f);
            PoseArm(leftArm, new Vector3(0, 0, 65 + Mathf.Sin(phase * 1.1f) * 3));
            PoseArm(rightArm, new Vector3(0, 0, -65 - Mathf.Sin(phase * 1.1f + 0.6f) * 3));
            PoseArm(leftForearm, new Vector3(0, -12, 0));
            PoseArm(rightForearm, new Vector3(0, 12 + gesture * 8, Storytelling ? -12 : 0));
            float emotionTime = seconds - emotionStarted;
            if (!Storytelling && emotionTime >= 0 && emotionTime < EmotionReactionSeconds)
            {
                // Smooth envelope returns to the existing idle without accumulated bone drift.
                float weight = Mathf.SmoothStep(0, 1, emotionTime / 0.4f)
                    * Mathf.SmoothStep(0, 1, (EmotionReactionSeconds - emotionTime) / 0.7f);
                Vector3 headPose = Vector3.zero, chestPose = Vector3.zero;
                Vector3 leftPose = new Vector3(0, 0, 65), rightPose = new Vector3(0, 0, -65);
                Vector3 leftLower = new Vector3(0, -12, 0), rightLower = new Vector3(0, 12, 0);
                switch (EmotionReaction)
                {
                    case "happy":
                        headPose = new Vector3(-6, 0, Mathf.Sin(emotionTime * 7) * 7);
                        rightPose = new Vector3(-25, -15, -22);
                        rightLower = new Vector3(0, 75, Mathf.Sin(emotionTime * 10) * 20);
                        chestPose = new Vector3(-4, Mathf.Sin(emotionTime * 5) * 5, 0);
                        break;
                    case "relaxed":
                        headPose = new Vector3(-7, 0, -6);
                        chestPose = new Vector3(-5, 0, 3);
                        leftPose = new Vector3(-8, 0, 58);
                        rightPose = new Vector3(-8, 0, -58);
                        break;
                    case "sad":
                        headPose = new Vector3(18, -8, -6);
                        chestPose = new Vector3(9, 0, -3);
                        leftPose = new Vector3(8, 0, 72);
                        rightPose = new Vector3(8, 0, -72);
                        break;
                    case "angry":
                        headPose = new Vector3(5, Mathf.Sin(emotionTime * 6) * 10, 0);
                        chestPose = new Vector3(-4, 0, 0);
                        leftPose = new Vector3(-12, 18, 45);
                        rightPose = new Vector3(-12, -18, -45);
                        leftLower = new Vector3(-10, -55, 0);
                        rightLower = new Vector3(-10, 55, 0);
                        break;
                    case "surprised":
                        headPose = new Vector3(-14, 0, 3);
                        chestPose = new Vector3(-8, 0, 0);
                        leftPose = new Vector3(-28, 12, 35);
                        rightPose = new Vector3(-28, -12, -35);
                        leftLower = new Vector3(-10, -65, 0);
                        rightLower = new Vector3(-10, 65, 0);
                        break;
                }
                if (head != null) head.localRotation *= Quaternion.Euler(headPose * weight);
                if (chest != null) chest.localRotation *= Quaternion.Euler(chestPose * weight);
                PoseArm(leftArm, Vector3.Lerp(new Vector3(0, 0, 65), leftPose, weight));
                PoseArm(rightArm, Vector3.Lerp(new Vector3(0, 0, -65), rightPose, weight));
                PoseArm(leftForearm, Vector3.Lerp(new Vector3(0, -12, 0), leftLower, weight));
                PoseArm(rightForearm, Vector3.Lerp(new Vector3(0, 12, 0), rightLower, weight));
                SampleFullBodyReaction(emotionTime, weight);
            }
            // Procedural affectionate poses on the existing control rig; never modify the model asset.
            if (!Storytelling && affectionWeight > 0)
            {
                float reach = affectionWeight * (0.65f + Mathf.Sin(phase * 3) * 0.15f);
                if (AffectionGesture == "hug")
                {
                    PoseArm(leftArm, new Vector3(-35 * reach, 25 * reach, 65 - 38 * reach));
                    PoseArm(rightArm, new Vector3(-35 * reach, -25 * reach, -65 + 38 * reach));
                    PoseArm(leftForearm, new Vector3(-15 * reach, -65 * reach, 0));
                    PoseArm(rightForearm, new Vector3(-15 * reach, 65 * reach, 0));
                }
                else if (AffectionGesture == "kiss")
                {
                    PoseArm(rightArm, new Vector3(-35 * reach, -20 * reach, -65 + 42 * reach));
                    PoseArm(rightForearm, new Vector3(-20 * reach, 95 * reach, -12 * reach));
                }
            }
            SampleApplause(seconds);
            foreach (ExpressionPreset preset in Moods)
                vrm.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), Mood == preset.ToString() ? FacialMoodWeight(Mood) : 0);
            float blink = BlinkWeight(seconds);
            vrm.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), blink);
            vrm.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blinkLeft), 0);
            vrm.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blinkRight), 0);
            // Silent dialogue performance, not audio-driven lip sync. Skip absent mouth presets.
            ExpressionKey mouth = ExpressionKey.CreateFromPreset(ExpressionPreset.aa);
            if (previousWeights.ContainsKey(mouth))
                vrm.Runtime.Expression.SetWeight(mouth, Speaking ? 0.08f + Mathf.Abs(Mathf.Sin(phase * 9)) * 0.18f : previousWeights[mouth]);
        }

        private void SampleApplause(float seconds)
        {
            float age = seconds - tapStarted;
            if (Storytelling || age < 0 || seconds >= tapUntil) return;
            float weight = Mathf.SmoothStep(0, 1, age / ApplauseBlendSeconds)
                * Mathf.SmoothStep(0, 1, (tapUntil - seconds) / ApplauseBlendSeconds);
            float beat = age * ApplauseClapsPerSecond * Mathf.PI * 2;
            // Hands meet in front of the chest, not inside the torso. Fixed visual
            // cadence is independent of taps/sec, frame rate and gameplay time scale.
            if (leftArm == null || rightArm == null || leftForearm == null || rightForearm == null
                || leftHand == null || rightHand == null) return;
            float reach = (Vector3.Distance(leftArm.position, leftForearm.position)
                + Vector3.Distance(leftForearm.position, leftHand.position)) / 0.36f;
            float gap = Mathf.Lerp(0.045f, 0.15f, (1 + Mathf.Cos(beat)) * 0.5f) * reach;
            Vector3 shoulders = (leftArm.position + rightArm.position) * 0.5f;
            Vector3 center = shoulders + (transform.up * -0.045f + transform.forward * 0.24f) * reach;
            PoseApplauseHand(leftArm, leftForearm, leftHand, center - transform.right * gap, -1, weight);
            PoseApplauseHand(rightArm, rightForearm, rightHand, center + transform.right * gap, 1, weight);
            BlendBody(head, new Vector3(4 + Mathf.Sin(beat) * 2, 0, 0), weight);
        }

        private void PoseApplauseHand(Transform upper, Transform lower, Transform hand, Vector3 target, float side, float weight)
        {
            if (upper == null || lower == null || hand == null) return;
            Quaternion upperIdle = upper.localRotation, lowerIdle = lower.localRotation, handIdle = hand.localRotation;
            float a = Vector3.Distance(upper.position, lower.position);
            float b = Vector3.Distance(lower.position, hand.position);
            if (a < 0.0001f || b < 0.0001f) return;
            Vector3 delta = target - upper.position;
            float distance = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + 0.0001f, a + b - 0.0001f);
            Vector3 direction = delta.normalized;
            float along = (a * a - b * b + distance * distance) / (2 * distance);
            Vector3 pole = Vector3.ProjectOnPlane(transform.right * side - transform.up, direction).normalized;
            Vector3 elbow = upper.position + direction * along + pole * Mathf.Sqrt(Mathf.Max(0, a * a - along * along));
            upper.rotation = Quaternion.FromToRotation(lower.position - upper.position, elbow - upper.position) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(hand.position - lower.position, target - lower.position) * lower.rotation;
            hand.rotation = Quaternion.LookRotation(transform.up, -transform.forward)
                * (side < 0 ? leftClapBasis : rightClapBasis);
            Quaternion upperPose = upper.localRotation, lowerPose = lower.localRotation, handPose = hand.localRotation;
            upper.localRotation = Quaternion.Slerp(upperIdle, upperPose, weight);
            lower.localRotation = Quaternion.Slerp(lowerIdle, lowerPose, weight);
            hand.localRotation = Quaternion.Slerp(handIdle, handPose, weight);
        }

        public static float FacialMoodWeight(string mood)
        {
            // VRoid happy/relaxed include closed-eye shapes: keep their resting
            // intensity low instead of holding a sleepy squint between blinks.
            return mood == "relaxed" ? 0.08f : mood == "happy" ? 0.28f : 0.65f;
        }

        public static float BlinkWeight(float seconds)
        {
            float blinkPhase = Mathf.Repeat(Mathf.Max(0, seconds) + 1.3f, 4.7f);
            return blinkPhase < 0.16f ? Mathf.Sin(blinkPhase / 0.16f * Mathf.PI) : 0;
        }

        private void BlendBody(Transform bone, Vector3 angles, float weight)
        {
            if (bone != null) bone.localRotation = Quaternion.Slerp(bone.localRotation, rest[bone] * Quaternion.Euler(angles), weight);
        }

        private void SampleFullBodyReaction(float time, float weight)
        {
            // In-place choreography on the existing humanoid rig. Root yaw is
            // untouched so drag rotation and the story-facing pose remain valid.
            float beat = time * Mathf.PI * 2;
            float sway = Mathf.Sin(beat);
            float leftStep = Mathf.Pow(Mathf.Max(0, sway), 2);
            float rightStep = Mathf.Pow(Mathf.Max(0, -sway), 2);
            float bounce = Mathf.Sin(beat * 2);
            Vector3 pelvis = Vector3.zero, torso = Vector3.zero;
            Vector3 leftLeg = Vector3.zero, rightLeg = Vector3.zero;
            Vector3 leftKnee = Vector3.zero, rightKnee = Vector3.zero;
            Vector3 leftAnkle = Vector3.zero, rightAnkle = Vector3.zero;
            Vector3 offset = Vector3.zero;
            float wrist = 0;
            switch (EmotionReaction)
            {
                case "happy":
                    // Step-touch with alternating heel lifts, counter-rotating
                    // shoulders/hips, arm sweeps, wrist flourishes and a soft bounce.
                    pelvis = new Vector3(-3 + bounce * 2, sway * 12, sway * 5);
                    torso = new Vector3(2, -sway * 10, -sway * 5);
                    leftLeg = new Vector3(-leftStep * 18, leftStep * 5, -4 - leftStep * 6);
                    rightLeg = new Vector3(-rightStep * 18, -rightStep * 5, 4 + rightStep * 6);
                    leftKnee.x = 6 + leftStep * 30;
                    rightKnee.x = 6 + rightStep * 30;
                    leftAnkle = new Vector3(-6 - leftStep * 10, 0, leftStep * 4);
                    rightAnkle = new Vector3(-6 - rightStep * 10, 0, -rightStep * 4);
                    BlendBody(leftArm, new Vector3(-18 + sway * 22, 12, 36 + sway * 18), weight);
                    BlendBody(rightArm, new Vector3(-18 - sway * 22, -12, -36 + sway * 18), weight);
                    BlendBody(leftForearm, new Vector3(-10, -45 - sway * 24, -10), weight);
                    BlendBody(rightForearm, new Vector3(-10, 45 - sway * 24, 10), weight);
                    offset = new Vector3(sway * 0.035f, -0.018f + bounce * 0.012f, 0);
                    wrist = sway * 16;
                    break;
                case "relaxed":
                    float slow = Mathf.Sin(time * Mathf.PI);
                    pelvis = new Vector3(0, slow * 7, slow * 4);
                    torso = new Vector3(-3, -slow * 6, -slow * 3);
                    leftLeg = new Vector3(-3, 0, -2 - slow * 2);
                    rightLeg = new Vector3(-3, 0, 2 - slow * 2);
                    leftKnee.x = 5 + Mathf.Max(0, slow) * 7;
                    rightKnee.x = 5 + Mathf.Max(0, -slow) * 7;
                    leftAnkle.x = rightAnkle.x = -4;
                    BlendBody(leftArm, new Vector3(-8, 8, 48 + slow * 12), weight);
                    BlendBody(rightArm, new Vector3(-8, -8, -48 + slow * 12), weight);
                    offset = new Vector3(slow * 0.018f, -0.008f, 0);
                    wrist = slow * 10;
                    break;
                case "sad":
                    float breath = Mathf.Sin(time * 2.4f);
                    pelvis = new Vector3(5, -3, -2);
                    torso = new Vector3(12 + breath * 2, 0, -3);
                    leftLeg.x = -8; rightLeg.x = -6;
                    leftKnee.x = 16 + breath * 2; rightKnee.x = 12;
                    leftAnkle.x = -8; rightAnkle.x = -6;
                    BlendBody(leftArm, new Vector3(-18, 16, 52), weight);
                    BlendBody(rightArm, new Vector3(-18, -16, -52), weight);
                    BlendBody(leftForearm, new Vector3(-8, -44, 0), weight);
                    BlendBody(rightForearm, new Vector3(-8, 44, 0), weight);
                    offset = new Vector3(0, -0.025f + breath * 0.003f, 0);
                    wrist = breath * 4;
                    break;
                case "angry":
                    pelvis = new Vector3(-3, sway * 8, sway * 3);
                    torso = new Vector3(-5, -sway * 10, -sway * 3);
                    leftLeg = new Vector3(-leftStep * 12, 0, -5);
                    rightLeg = new Vector3(-rightStep * 12, 0, 5);
                    leftKnee.x = 5 + leftStep * 22; rightKnee.x = 5 + rightStep * 22;
                    leftAnkle.x = -5 - leftStep * 8; rightAnkle.x = -5 - rightStep * 8;
                    offset = new Vector3(sway * 0.018f, -0.012f + bounce * 0.008f, 0);
                    wrist = sway * 6;
                    break;
                case "surprised":
                    float recoil = Mathf.Exp(-time * 0.8f);
                    pelvis = new Vector3(-8 + bounce * recoil * 3, 0, sway * 2);
                    torso = new Vector3(-10 + recoil * 4, sway * 4, 0);
                    leftLeg = new Vector3(-12, -4, -5);
                    rightLeg = new Vector3(-8, 4, 5);
                    leftKnee.x = 24 + bounce * recoil * 4; rightKnee.x = 18;
                    leftAnkle.x = -12; rightAnkle.x = -10;
                    offset = new Vector3(0, -0.035f + recoil * 0.018f, -recoil * 0.015f);
                    wrist = sway * 10;
                    break;
            }
            BlendBody(hips, pelvis, weight);
            BlendBody(spine, torso, weight);
            BlendBody(chest, torso * 0.6f, weight * 0.7f);
            BlendBody(leftThigh, leftLeg, weight); BlendBody(rightThigh, rightLeg, weight);
            BlendBody(leftShin, leftKnee, weight); BlendBody(rightShin, rightKnee, weight);
            BlendBody(leftFoot, leftAnkle, weight); BlendBody(rightFoot, rightAnkle, weight);
            BlendBody(leftHand, new Vector3(wrist * 0.3f, 0, wrist), weight);
            BlendBody(rightHand, new Vector3(-wrist * 0.3f, 0, -wrist), weight);
            transform.localPosition += offset * weight;
        }

        private void PoseArm(Transform bone, Vector3 angles)
        {
            if (bone != null) bone.localRotation = rest[bone] * Quaternion.Euler(angles);
        }

        private void OnDisable()
        {
            if (!initialized) return;
            transform.localPosition = restPosition;
            foreach (KeyValuePair<Transform, Quaternion> pose in rest)
                if (pose.Key != null) pose.Key.localRotation = pose.Value;
            foreach (KeyValuePair<ExpressionKey, float> weight in previousWeights)
                vrm.Runtime.Expression.SetWeight(weight.Key, weight.Value);
            vrm.UpdateType = previousUpdateType;
            rest.Clear();
            previousWeights.Clear();
            initialized = false;
            tapStarted = tapUntil = -10;
            Mood = "Neutral";
            Storytelling = Speaking = false;
            SetAffection("idle", 0);
            emotionStarted = -10;
            EmotionReaction = "Neutral";
        }

        private void OnGUI()
        {
            if (!ShowControls || !initialized) return;
            GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(240, Screen.width - 24), 300), GUI.skin.box);
            GUILayout.Label("Zenda / local animation preview");
            GUILayout.Label("Procedural idle + blink (not retargeted)");
            if (GUILayout.Button("Neutral")) SetMood("Neutral");
            foreach (ExpressionPreset preset in Moods)
                if (GUILayout.Button(preset.ToString())) SetMood(preset.ToString());
            GUILayout.Label("Mood: " + Mood);
            GUILayout.EndArea();
        }
    }
}