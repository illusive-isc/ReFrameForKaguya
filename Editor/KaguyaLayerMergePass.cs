using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(jp.illusive_isc.ReFrame.IKUSIA.Editor.KaguyaLayerMergeDefinition))]

namespace jp.illusive_isc.ReFrame.IKUSIA.Editor
{
    public class KaguyaLayerMergeDefinition : Plugin<KaguyaLayerMergeDefinition>
    {
        public override string QualifiedName => "IllusoryOverride.ReFrameForIKUSIA.KaguyaLayerMerge";
        public override string DisplayName => "ReFrame for IKUSIA / kaguya layer merge";

        protected override void Configure()
        {

            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("IllusoryOverride.ReFrameCore.Delete")
                .Run(KaguyaLayerMergePass.Instance);
        }
    }

    /// <summary>kaguya (paryi_FX) 固有のレイヤー統合。</summary>
    [DependsOnContext(typeof(AnimatorServicesContext))]
    public class KaguyaLayerMergePass : Pass<KaguyaLayerMergePass>
    {
        const string AfkLayerName = "PenCtrl_R";
        const string NadeLayerName = "HeartGun";
        const string InitLayerName = "ColliderCtrl";
        const string MergedLayerName = "ReFrame Init";

        const string AfkIdleStateName = "off";
        const string NadeStateName = "IKUSIA NadeAction on";

        static readonly string[] AfkStateNames =
        {
            "off",
            "kaguyaafk",
            "kaguyaafk end",
            "kaguyaafk off",
        };

        /// <summary>なでカメラ側で「何もしない待機ステート」として捨ててよい名前。</summary>
        static readonly string[] NadeIdleStateNames = { "New State", "afk" };

        sealed class InitParts
        {
            public VirtualLayer Layer;
            public VirtualState Entry;
            public VirtualState Driver;
        }

        sealed class AfkParts
        {
            public VirtualLayer Layer;
            public VirtualState Idle;
            public List<VirtualState> Chain;
        }

        sealed class NadeParts
        {
            public VirtualLayer Layer;
            public VirtualState State;
            public List<VirtualStateTransition> Entries;
        }

        protected override void Execute(BuildContext context)
        {
            var asc = context.Extension<AnimatorServicesContext>();

            foreach (
                var controller in asc
                    .ControllerContext.Controllers.Values.Where(c => c != null)
                    .Distinct()
            )
            {
                MergeController(controller);
            }
        }

        const string NadeStateDesktop = "IKUSIA NadeAction on 1";
        const string NadeStateVr = "IKUSIA NadeAction on 2";

        /// <summary>なでカメラの on 1 (デスクトップ) と on 2 (VR) を 1 ステートに畳む。</summary>
        static bool TryFoldNadeCameraByVrMode(VirtualAnimatorController controller)
        {
            var layer = FindLayer(controller, NadeLayerName);
            if (layer?.StateMachine == null)
                return false;
            var sm = layer.StateMachine;

            VirtualState desktop = null;
            VirtualState vr = null;
            foreach (var child in sm.States)
            {
                if (child.State == null)
                    continue;
                if (child.State.Name == NadeStateDesktop)
                    desktop = child.State;
                else if (child.State.Name == NadeStateVr)
                    vr = child.State;
            }
            if (desktop == null || vr == null)
                return false;
            if (desktop.Motion == null || vr.Motion == null)
                return false;

            VirtualStateTransition toDesktop = null;
            VirtualStateTransition toVr = null;
            foreach (var transition in sm.AnyStateTransitions)
            {
                if (transition.DestinationState == desktop)
                    toDesktop = transition;
                else if (transition.DestinationState == vr)
                    toVr = transition;
            }
            if (toDesktop == null || toVr == null)
                return false;
            if (!SameExceptVrMode(toDesktop.Conditions, toVr.Conditions))
                return false;

            var tree = VirtualBlendTree.Create("NadeCamera VRMode");
            tree.BlendType = BlendTreeType.Simple1D;
            tree.BlendParameter = "VRMode";
            tree.UseAutomaticThresholds = false;
            tree.Children = ImmutableList.Create(
                new VirtualBlendTree.VirtualChildMotion
                {
                    Motion = desktop.Motion,
                    Threshold = 0f,
                    TimeScale = 1f,
                },
                new VirtualBlendTree.VirtualChildMotion
                {
                    Motion = vr.Motion,
                    Threshold = 1f,
                    TimeScale = 1f,
                }
            );

            desktop.Motion = tree;
            desktop.Name = NadeStateName;
            toDesktop.Conditions = toDesktop
                .Conditions.Where(c => c.parameter != "VRMode")
                .ToImmutableList();
            toDesktop.CanTransitionToSelf = false;

            sm.AnyStateTransitions = sm.AnyStateTransitions.Remove(toVr);
            sm.States = sm.States.Where(c => c.State != vr).ToImmutableList();

            if (
                controller.Parameters.TryGetValue("VRMode", out var vrMode)
                && vrMode.type != AnimatorControllerParameterType.Float
            )
            {
                controller.Parameters = controller.Parameters.SetItem(
                    "VRMode",
                    new AnimatorControllerParameter
                    {
                        name = "VRMode",
                        type = AnimatorControllerParameterType.Float,
                        defaultFloat = 0f,
                    }
                );
            }

            return true;
        }

        /// <summary>2 つの条件列が、VRMode の条件を除いて一致するか。</summary>
        static bool SameExceptVrMode(
            IEnumerable<AnimatorCondition> a,
            IEnumerable<AnimatorCondition> b
        )
        {
            var left = a.Where(c => c.parameter != "VRMode").ToList();
            var right = b.Where(c => c.parameter != "VRMode").ToList();
            if (left.Count != right.Count)
                return false;
            for (var i = 0; i < left.Count; i++)
            {
                if (
                    left[i].parameter != right[i].parameter
                    || left[i].mode != right[i].mode
                    || !Mathf.Approximately(left[i].threshold, right[i].threshold)
                )
                    return false;
            }
            return true;
        }

        static void MergeController(VirtualAnimatorController controller)
        {

            TryFoldNadeCameraByVrMode(controller);

            var init = ResolveInitParts(controller);
            var afk = ResolveAfkParts(controller);

            var nade = afk == null ? null : ResolveNadeParts(controller);

            if (init == null && nade == null)
                return;

            var consumed = new List<VirtualLayer>();
            if (init != null)
                consumed.Add(init.Layer);
            if (afk != null)
                consumed.Add(afk.Layer);
            if (nade != null)
                consumed.Add(nade.Layer);

            var anchor = afk?.Layer ?? init.Layer;
            var anchorIndex = ResolveSurvivingIndex(controller, anchor, consumed);

            var positions = CollectPositions(consumed);

            var merged = controller.AddLayer(LayerPriority.Default, MergedLayerName);
            var sm = merged.StateMachine;
            if (sm == null)
            {
                controller.RemoveLayer(merged);
                return;
            }
            merged.DefaultWeight = 1f;
            merged.BlendingMode = AnimatorLayerBlendingMode.Override;

            var states = new List<VirtualState>();
            if (init != null)
                states.Add(init.Entry);

            VirtualState idle;
            if (afk != null)
            {
                idle = afk.Idle;
                states.Add(idle);
                states.AddRange(afk.Chain);
                if (nade != null)
                    states.Add(nade.State);
            }
            else
            {
                idle = init.Driver;
                states.Add(idle);
            }

            if (afk != null && init != null)
            {

                idle.Behaviours = idle.Behaviours.AddRange(init.Driver.Behaviours);
                init.Entry.Transitions[0].SetDestination(idle);
            }

            if (nade != null)
            {

                var transitions = idle.Transitions;
                foreach (var source in nade.Entries)
                {
                    var moved = VirtualStateTransition.Create();
                    moved.SetDestination(nade.State);
                    moved.Conditions = source.Conditions;
                    moved.Duration = source.Duration;
                    moved.HasFixedDuration = source.HasFixedDuration;
                    moved.ExitTime = null;
                    transitions = transitions.Add(moved);
                }
                idle.Transitions = transitions;
            }

            if (init != null)
                RedirectExitsTo(states, idle);

            sm.States = LayOut(states, positions);
            sm.DefaultState = init != null ? init.Entry : idle;

            var discarded = new HashSet<VirtualLayer>(consumed);
            controller.RemoveLayers(l => discarded.Contains(l));

            MoveLayerTo(controller, merged, anchorIndex);

            Debug.Log(
                $"[ReFrameForIKUSIA] kaguya layer merge: "
                    + $"{string.Join(" / ", consumed.Select(l => l.Name))} → '{MergedLayerName}' "
                    + $"(index {anchorIndex}, {states.Count} states)"
            );
        }

        /// <summary>anchor が、consumed を取り除いた後の並びで 何番目に相当するかを返す。</summary>
        static int ResolveSurvivingIndex(
            VirtualAnimatorController controller,
            VirtualLayer anchor,
            IEnumerable<VirtualLayer> consumed
        )
        {
            var removed = new HashSet<VirtualLayer>(consumed);
            var index = 0;
            foreach (var layer in controller.Layers)
            {
                if (layer == anchor)
                    return index;
                if (!removed.Contains(layer))
                    index++;
            }
            return index;
        }

        /// <summary>Animator ウィンドウ上のステートノードのおおよその大きさ。</summary>
        const float NodeWidth = 220f;
        const float NodeHeight = 60f;

        /// <summary>元のレイヤーでの座標をそのまま複製してステートを並べる。</summary>
        static ImmutableList<VirtualStateMachine.VirtualChildState> LayOut(
            IEnumerable<VirtualState> states,
            IReadOnlyDictionary<VirtualState, Vector3> positions
        )
        {
            var placed = new List<Vector3>();
            var result = ImmutableList.CreateBuilder<VirtualStateMachine.VirtualChildState>();

            foreach (var state in states)
            {
                var position = positions.TryGetValue(state, out var original)
                    ? original
                    : Vector3.zero;

                while (
                    placed.Any(p =>
                        Mathf.Abs(p.x - position.x) < NodeWidth
                        && Mathf.Abs(p.y - position.y) < NodeHeight
                    )
                )
                {
                    position.y += NodeHeight;
                }

                placed.Add(position);
                result.Add(
                    new VirtualStateMachine.VirtualChildState
                    {
                        State = state,
                        Position = position,
                    }
                );
            }

            return result.ToImmutable();
        }

        /// <summary>畳む対象のレイヤーから、各ステートのグラフ上の座標を控える。</summary>
        static Dictionary<VirtualState, Vector3> CollectPositions(IEnumerable<VirtualLayer> layers)
        {
            var positions = new Dictionary<VirtualState, Vector3>();
            foreach (var layer in layers)
            {
                var sm = layer.StateMachine;
                if (sm == null)
                    continue;
                foreach (var child in sm.States)
                {
                    if (child.State != null)
                        positions[child.State] = child.Position;
                }
            }
            return positions;
        }

        /// <summary>レイヤーを指定位置へ差し込み直す。</summary>
        static void MoveLayerTo(VirtualAnimatorController controller, VirtualLayer layer, int index)
        {
            var layers = controller.Layers.ToList();
            if (!layers.Remove(layer))
                return;
            layers.Insert(Mathf.Clamp(index, 0, layers.Count), layer);
            controller.Layers = layers;
        }

        /// <summary>Exit 行きの遷移をすべて destination 行きに張り替える。</summary>
        static void RedirectExitsTo(IEnumerable<VirtualState> states, VirtualState destination)
        {
            foreach (var state in states)
            foreach (var transition in state.Transitions)
            {
                if (transition.IsExit)
                    transition.SetDestination(destination);
            }
        }

        static VirtualLayer FindLayer(VirtualAnimatorController controller, string name) =>
            controller.Layers.FirstOrDefault(l => l != null && l.Name == name);

        /// <summary>重み・ブレンド・マスクが既定のままで、他レイヤーと単純に混ぜられるか。</summary>
        static bool IsPlainOverrideLayer(VirtualLayer layer) =>
            layer.AvatarMask == null
            && layer.BlendingMode == AnimatorLayerBlendingMode.Override
            && Mathf.Approximately(layer.DefaultWeight, 1f)
            && layer.SyncedLayerIndex < 0;

        /// <summary>ColliderCtrl が「起動時に 1 回だけ遷移して Driver を撃つ」だけに痩せているか。</summary>
        static InitParts ResolveInitParts(VirtualAnimatorController controller)
        {
            var layer = FindLayer(controller, InitLayerName);
            if (layer == null || !IsPlainOverrideLayer(layer))
                return null;

            var sm = layer.StateMachine;
            if (sm == null || sm.StateMachines.Count > 0)
                return null;
            if (sm.AnyStateTransitions.Count > 0 || sm.EntryTransitions.Count > 0)
                return null;
            if (sm.States.Count != 2)
                return null;

            VirtualState driver = null;
            VirtualState entry = null;
            foreach (var child in sm.States)
            {
                var state = child.State;

                if (state == null || state.Motion != null)
                    return null;

                if (state.Behaviours.Count > 0 && state.Transitions.Count == 0)
                    driver = state;
                else if (state.Behaviours.Count == 0 && state.Transitions.Count == 1)
                    entry = state;
                else
                    return null;
            }
            if (driver == null || entry == null || sm.DefaultState != entry)
                return null;
            if (entry.Transitions[0].DestinationState != driver)
                return null;

            if (driver.Behaviours.Any(b => !(b is VRCAvatarParameterDriver)))
                return null;

            return new InitParts
            {
                Layer = layer,
                Entry = entry,
                Driver = driver,
            };
        }

        /// <summary>PenCtrl_R が「AFK 演出だけ」に痩せているか。</summary>
        static AfkParts ResolveAfkParts(VirtualAnimatorController controller)
        {
            var layer = FindLayer(controller, AfkLayerName);
            if (layer == null || !IsPlainOverrideLayer(layer))
                return null;

            var sm = layer.StateMachine;
            if (sm == null || sm.StateMachines.Count > 0)
                return null;

            if (sm.AnyStateTransitions.Count > 0 || sm.EntryTransitions.Count > 0)
                return null;
            if (sm.States.Count != AfkStateNames.Length)
                return null;

            var states = sm.States.Select(c => c.State).ToList();
            if (states.Any(s => s == null))
                return null;
            var names = states.Select(s => s.Name).ToList();
            if (AfkStateNames.Any(expected => !names.Contains(expected)))
                return null;

            var idle = states.First(s => s.Name == AfkIdleStateName);
            if (sm.DefaultState != idle)
                return null;

            return new AfkParts
            {
                Layer = layer,
                Idle = idle,
                Chain = states.Where(s => s != idle).ToList(),
            };
        }

        /// <summary>HeartGun が「なでカメラだけ」に痩せているか。</summary>
        static NadeParts ResolveNadeParts(VirtualAnimatorController controller)
        {
            var layer = FindLayer(controller, NadeLayerName);
            if (layer == null || !IsPlainOverrideLayer(layer))
                return null;

            var sm = layer.StateMachine;
            if (sm == null || sm.StateMachines.Count > 0 || sm.EntryTransitions.Count > 0)
                return null;

            VirtualState nade = null;
            foreach (var child in sm.States)
            {
                var state = child.State;
                if (state == null)
                    return null;
                if (state.Name == NadeStateName)
                {
                    nade = state;
                    continue;
                }

                if (!NadeIdleStateNames.Contains(state.Name))
                    return null;
                if (state.Motion != null || state.Behaviours.Count > 0)
                    return null;
            }
            if (nade == null)
                return null;

            var entries = new List<VirtualStateTransition>();
            foreach (var transition in sm.AnyStateTransitions)
            {
                var destination = transition.DestinationState;
                if (destination == nade)
                {
                    entries.Add(transition);
                    continue;
                }
                if (destination == null || !NadeIdleStateNames.Contains(destination.Name))
                    return null;
            }
            if (entries.Count == 0)
                return null;

            return new NadeParts
            {
                Layer = layer,
                State = nade,
                Entries = entries,
            };
        }
    }
}
