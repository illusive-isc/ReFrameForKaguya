using jp.illusive_isc.ReFrame.Core;
using UnityEngine;

namespace jp.illusive_isc.ReFrame.IKUSIA.Kaguya
{
    [AddComponentMenu("ILLUSORY OVERRIDE/ReFrame/KaguyaReFrame")]
    [ReFrameAvatarSignature(
        "kaguya",
        FbxGuids = new[] { "38ad9ff0c7a686442b157fb93f1a6a20" },
        AvatarNames = new[] { "kaguyaAvatar" }
    )]
    [ReFrameTheme("Packages/jp.illusive-isc.reframe-kaguya/Editor/UI/KaguyaTheme.uss")]
    [ReFrameLayerRename("ear kaguya", "tail Left Hand", "ear Left Hand")]
    [ReFrameLayerRename("ear kaguya", "tail Right Hand", "ear Right Hand")]
    [ReFrameLayerRename("ear kaguya", "contact", "ear contact")]
    [ReFrameCutCovered(
        "Body_b",
        Covers = new[] { "sailor", "outer", "stocking", "loafer", "beret", "bag" },
        MaskAsset = "Packages/jp.illusive-isc.reframe-kaguya/Runtime/CoveredMasks/Body_b.txt"
    )]
    [ReFramePhysBoneGroup("尻尾", "tail")]
    [ReFramePhysBoneGroup("後ろ髪", "Backhair", "Side_back")]
    [ReFramePhysBoneGroup("前髪", "FrontHair", "int")]
    [ReFramePhysBoneGroup("横髪", "Sidehair")]
    [ReFramePhysBoneGroup("アホ毛", "Ahoge", "ahoge2")]
    [ReFramePhysBoneGroup("スカート", "skirt_root")]
    [ReFramePhysBoneGroup("袖", "sode_hand")]
    [ReFramePhysBoneGroup("耳", "ear_pb")]
    [ReFramePhysBoneGroup("胸", "Breast")]
    [ReFramePhysBoneGroup("もちまるを掴む", "Grab bone")]
    [ReFrameGroupOrder(

        "closet", "Gimmick", "IKUSIA_emote",

        "もちまる", "FireGun", "Particle", "ペン", "コライダー", "Face", "Gesture",

        "tail", "ear"
    )]
    [ReFrameGroupLabel("Gesture", "ジェスチャー")]
    [ReFrameGroupLabel("FireGun", "ファイアガン")]
    [ReFrameGroupLabel("Hand Animaton", "ハンドアニメーション")]
    [ReFrameGroupLabel("closet", "衣装・姿")]
    [ReFrameGroupLabel("tail", "尻尾")]
    [ReFrameGroupLabel("ear", "耳")]
    [ReFrameQuestBake(
        Brightness = 0.83f,
        ShadowFromNormalMap = true,
        MaxTextureSize = 1024

    )]
    [ReFrameQuestBrightness("Body", Brightness = 1f)]
    [ReFrameQuestBlendBackdrop(
        "kaguya_cloth/stocking",
        Opacity = 0.7f,
        Backdrop = "#FFEAEB"
    )]
    [ReFrameQuestCutByBlendShape("Body", "照れ", "Cheek2", "Cheek3", "Cheek4", "ga-n")]
    [ReFrameQuestTransparent("kaguya_cloth/outer", 1, Label = "油揚げの袋", Beyond = "#B0A8A7", Rim = 0.2f, Gloss = 0.35f, Size = 1024)]
    [ReFrameQuestTransparent("kaguya_cloth/outer_breast_big_open", 1, Label = "油揚げの袋", Beyond = "#B0A8A7", Rim = 0.2f, Gloss = 0.35f, Size = 1024)]
    [ReFrameQuestTransparent("kaguya_cloth/aburaage_open", 0, Label = "油揚げの袋 (口元)", Beyond = "#B0A8A7", Rim = 0.2f, Gloss = 0.35f, AllSides = true, Size = 1024)]

    public class KaguyaReFrame : IKUSIACommonReFrame
    {
        [ReFrameDelete("kaguya beret", ReFrameParameterType.Bool)]
        [ReFrameLabel("ベレー帽")]
        public ReFrameDeleteEntry beret = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya outer", ReFrameParameterType.Bool)]
        [ReFrameLabel("アウター")]
        public ReFrameDeleteEntry outer = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya sailor", ReFrameParameterType.Bool)]
        [ReFrameLabel("セーラー服")]
        public ReFrameDeleteEntry sailor = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya stocking", ReFrameParameterType.Bool)]
        [ReFrameLabel("ストッキング")]
        public ReFrameDeleteEntry stocking = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya loafer", ReFrameParameterType.Bool)]
        [ReFrameLabel("ローファー")]
        public ReFrameDeleteEntry loafer = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya bag", ReFrameParameterType.Bool)]
        [ReFrameBlendTreeOverride("kaguya bag", "kaguya sailor", 1f, Always = true)]
        [ReFrameLabel("バッグ")]
        public ReFrameDeleteEntry bag = new() { Enabled = false, Value = 1f };

        [ReFrameDelete("kaguya aburaage", ReFrameParameterType.Bool)]
        [ReFrameLabel("油揚げ")]
        public ReFrameDeleteEntry aburaage = new() { Enabled = false, Value = 0f };

        [ReFrameDelete("kaguya outer_breast_big_open", ReFrameParameterType.Bool)]
        [ReFrameLabel("胸元の開いた衣装")]
        public ReFrameDeleteEntry outerBreastBigOpen = new() { Enabled = true, Value = 0f };

        [ReFrameMenuGroup("closet")]
        [ReFrameDeleteObject("Hair")]
        [ReFrameDeleteObject("Advanced/Hair rotation")]
        [ReFrameDeleteRelatedBlendTree("Hair rotation")]
        [ReFrameValueLocked(0f)]
        [ReFrameLabel("髪 (髪型を差し替えるとき)")]
        public ReFrameDeleteEntry hair = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet")]
        [ReFrameDeleteObject("Bra")]
        [ReFrameDeleteObject("Pants")]
        [ReFrameValueLocked(0f)]
        [ReFrameLabel("下着 (ブラ・パンツ)")]
        public ReFrameDeleteEntry braAndPants = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("IKUSIA_emote", "姿勢変更", "VRCEmote")]
        [ReFrameBundleMember("VRCEmote")]
        [ReFrameDeleteRelatedBlendTree("VRMode0", Always = true, Bake = true)]
        [ReFrameDeleteState("HeartGun", "IKUSIA NadeAction on 1")]
        [ReFrameDeleteState("HeartGun", "IKUSIA NadeAction on 2")]
        [ReFrameValueLocked(0f)]
        [ReFrameDelete("IKUSIA NadeAction", ReFrameParameterType.Float)]
        [ReFrameDeleteObject("Advanced/NadeCamera")]
        [ReFrameLabel("なで (軸・カメラ)")]
        public ReFrameDeleteEntry nadeCamera = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "Gesture")]
        [ReFrameDelete("GestureVariation")]
        [ReFrameCutUndrivenTransitions("phone on", EvaluateAsFixed = true)]
        [ReFrameCutUndrivenTransitions("doughnut", EvaluateAsFixed = true)]
        [ReFrameCutUndrivenTransitions("beer", EvaluateAsFixed = true)]
        [ReFrameLabel("ジェスチャー差分")]
        public ReFrameDeleteEntry gestureVariation = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick")]
        [ReFrameApplyToAvatar]
        [ReFrameLabel("足: ヒールオフ")]
        [ReFrameBlendShape("Body_b", "Foot_heel_OFF_____足_ヒールオフ")]
        [ReFrameBlendShape("kaguya_cloth/stocking", "Foot_heel_OFF_____足_ヒールオフ")]
        public ReFrameDeleteEntry heelOff = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick")]
        [ReFrameApplyToAvatar]
        [ReFrameLabel("足: ハイヒール")]
        [ReFrameBlendShape("Body_b", "Foot_Hiheel_____足_ハイヒール")]
        [ReFrameBlendShape("kaguya_cloth/stocking", "Foot_Hiheel_____足_ハイヒール")]
        public ReFrameDeleteEntry highHeel = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "FireGun")]
        [ReFrameDelete("kaguya fire Gun", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet point L", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet point R", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet gun L", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet gun R", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet reset L", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet reset R", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet set", ReFrameParameterType.Bool)]
        [ReFrameDelete("kaguya pet shot", ReFrameParameterType.Bool)]
        [ReFrameValueLocked]
        [ReFrameDeleteLayer("fish pet", 0f)]
        [ReFrameDeleteObject("Advanced/fire pet", 0f)]
        [ReFrameDeleteObject("Advanced/Fire pet ray L", 0f)]
        [ReFrameDeleteObject("Advanced/Fire pet ray R", 0f)]
        [ReFrameDeleteRelatedBlendTree("pet anime")]
        [ReFrameLabel("ファイアガン")]
        public ReFrameDeleteEntry fireGun = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "FireGun")]
        [ReFrameDelete("kaguya pet light level", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya fire Gun")]
        [ReFrameLabel("ファイアガンの光量")]
        public ReFrameDeleteEntry lightPower = new() { Enabled = false, Value = 0.1f };

        [ReFrameMenuGroup("Gimmick", "もちまる")]
        [ReFrameDelete("mochimaru ON", ReFrameParameterType.Bool)]
        [ReFrameDelete("mochimaru position X", ReFrameParameterType.Float)]
        [ReFrameDelete("mochimaru position Y", ReFrameParameterType.Float)]
        [ReFrameDelete("mochimaru random animation", ReFrameParameterType.Float)]
        [ReFrameDelete("mochimaru random move", ReFrameParameterType.Float)]
        [ReFrameDelete("mochimaru FLY", ReFrameParameterType.Float)]
        [ReFrameDelete("mochimaru Position Head", ReFrameParameterType.Bool)]
        [ReFrameValueLocked]
        [ReFrameDeleteLayer("mochimaru gimmick", 0f)]
        [ReFrameDeleteLayer("mochimaru animation", 0f)]
        [ReFrameDeleteObject("Advanced/mochimaru", 0f)]
        [ReFrameLabel("もちまる")]
        public ReFrameDeleteEntry mochimaruOn = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail Toggle", ReFrameParameterType.Bool)]
        [ReFrameReverse]
        [ReFrameLabel("尻尾")]
        public ReFrameDeleteEntry tailToggle = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail variation", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLabel("尻尾の見た目")]
        public ReFrameDeleteEntry tailVariation = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail PB variation", ReFrameParameterType.Bool)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLabel("尻尾の揺れ方")]
        public ReFrameDeleteEntry tailPbVariation = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail Count", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLabel("尻尾の本数")]
        public ReFrameDeleteEntry tailCount = new() { Enabled = false, Value = 1f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail scale", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLabel("尻尾の大きさ")]
        public ReFrameDeleteEntry tailScale = new() { Enabled = false, Value = 0.5f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail Horizontal", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLinkedWith("kaguya tail Vertical")]
        [ReFrameLabel("尻尾の向き (左右)")]
        public ReFrameDeleteEntry tailHorizontal = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("closet", "tail")]
        [ReFrameDelete("kaguya tail Vertical", ReFrameParameterType.Float)]
        [ReFrameBundleMember("kaguya tail Toggle")]
        [ReFrameLinkedWith("kaguya tail Horizontal")]
        [ReFrameLabel("尻尾の向き (上下)")]
        public ReFrameDeleteEntry tailVertical = new() { Enabled = false, Value = -1f };

        [ReFrameMenuGroup("closet", "ear")]
        [ReFrameDelete("kaguya ear", ReFrameParameterType.Bool)]
        [ReFrameLabel("耳")]
        public ReFrameDeleteEntry ear = new() { Enabled = false, Value = 1f };

        [ReFrameMenuGroup("Gimmick", "Particle")]
        [ReFrameDelete("Particle1", ReFrameParameterType.Bool)]
        [ReFrameDeleteRelatedBlendTree("VoiceParticle1", 0f)]
        [ReFrameDeleteObject("Advanced/Particle/1", 0f)]
        [ReFrameQuestParticleMaterial("Advanced/Particle/1/breath")]
        [ReFrameLabel("白い息")]
        public ReFrameDeleteEntry whiteBreath = new() { Enabled = false, Value = 0f };

        [ReFrameDelete("Pen1", ReFrameParameterType.Bool)]
        [ReFrameDelete("Pen1Grab", ReFrameParameterType.Bool)]
        [ReFrameValueLocked(0f)]
        [ReFrameQuestParticleMaterial("Advanced/Particle/7/2/pen1_R/PenParticle")]
        [ReFrameQuestParticleMaterial("Advanced/Particle/7/2/pen1_R/PenParticle/SubEmitter0")]
        [ReFrameDeleteObject("Advanced/Particle/7/Pen_Eraser_R")]
        [ReFrameDeleteObject(
            "Advanced/Constraint/Hand_L_Constraint0",
            RequiresAll = new[] { "Pen2", "HeartGun" }
        )]
        [ReFrameDeleteObject(
            "Advanced/Constraint/Hand_R_Constraint0",
            RequiresAll = new[] { "Pen2", "HeartGun" }
        )]
        [ReFrameMenuGroup("Gimmick", "ペン")]
        [ReFrameLabel("ペン 1 (掴みを含む)")]
        public ReFrameDeleteEntry pen1 = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "ペン")]
        [ReFrameDelete("Pen2", ReFrameParameterType.Bool)]
        [ReFrameDelete("Pen2Grab", ReFrameParameterType.Bool)]
        [ReFrameValueLocked(0f)]
        [ReFrameQuestParticleMaterial("Advanced/Particle/7/4/pen1_L/PenParticle")]
        [ReFrameQuestParticleMaterial("Advanced/Particle/7/4/pen1_L/PenParticle/SubEmitter0")]
        [ReFrameDeleteObject("Advanced/Particle/7/Pen_Eraser_L")]
        [ReFrameLabel("ペン 2 (掴みを含む)")]
        public ReFrameDeleteEntry pen2 = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "Particle")]
        [ReFrameDelete("8bit", ReFrameParameterType.Bool)]
        [ReFrameDeleteObject("Advanced/Gimmick1/8", Always = true)]
        [ReFrameDeleteObject("Advanced/Ground", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Index_R_Constraint", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Index_L_Constraint", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Hand_L_Constraint", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Hand_R_Constraint", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Middle_L_Constraint0", Always = true)]
        [ReFrameDeleteObject("Advanced/Constraint/Middle_R_Constraint0", Always = true)]
        [ReFrameSetMaxParticles("Advanced/Particle/5/8bitheart", 16, Always = true)]
        [ReFrameSetMaxParticles("Advanced/Particle/5/8bitheart/8bitheart flare", 32, Always = true)]
        [ReFrameQuestParticleMaterial("Advanced/Particle/5/8bitheart")]
        [ReFrameQuestParticleMaterial("Advanced/Particle/5/8bitheart/8bitheart flare")]
        [ReFrameLabel("8bit")]
        public ReFrameDeleteEntry eightBit = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "Particle")]
        [ReFrameDelete("HeartGun", ReFrameParameterType.Bool)]
        [ReFrameDelete("HeartGunCollider R", ReFrameParameterType.Float)]
        [ReFrameDelete("HeartGunCollider L", ReFrameParameterType.Float)]
        [ReFrameValueLocked(0f)]
        [ReFrameDeleteObject("Advanced/HeartGunR")]
        [ReFrameDeleteObject("Advanced/HeartGunL")]
        [ReFrameDeleteObject("Advanced/HeartGunR2")]
        [ReFrameDeleteObject("Advanced/HeartGunL2")]
        [ReFrameLabel("ハートガン")]
        public ReFrameDeleteEntry heartGun = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "コライダー")]
        [ReFrameDelete("JumpCollider", ReFrameParameterType.Bool)]
        [ReFrameDelete("Paryi_KeyJump", ReFrameParameterType.Bool)]
        [ReFrameDelete("SpeedCollider", ReFrameParameterType.Bool)]
        [ReFrameDelete("Paryi_KeySpeed", ReFrameParameterType.Bool)]
        [ReFrameDelete("ColliderON", ReFrameParameterType.Bool)]
        [ReFrameCutUndrivenTransitions("ColliderON")]
        [ReFrameCutUndrivenTransitions("Paryi_KeyJump", EvaluateAsFixed = true)]
        [ReFrameCutUndrivenTransitions("Paryi_KeySpeed", EvaluateAsFixed = true)]
        [ReFrameDeleteState("ColliderCtrl", "Instation", 0f)]
        [ReFrameUnsyncParameter("JumpCollider")]
        [ReFrameUnsyncParameter("SpeedCollider")]
        [ReFrameValueLocked]
        [ReFrameDeleteObject("Advanced/Gimmick1/JUMP")]
        [ReFrameDeleteObject("Advanced/Gimmick1/SPEED")]
        [ReFrameLabel("ジャンプ・ダッシュ (コライダー)")]
        public ReFrameDeleteEntry jumpDashCollider = new() { Enabled = false, Value = 0f };

        [ReFrameMenuGroup("Gimmick", "Particle")]
        [ReFrameValueLocked(0f)]
        [ReFrameDeleteObject("Advanced/AFK/afkFire/Particle System (2)")]
        [ReFrameDeleteObject("Advanced/AFK/afkFire/fire 2 (1)")]
        [ReFrameDeleteObject("Advanced/AFK/afkFire/fire 2 (2)")]
        [ReFrameDeleteObject("Advanced/AFK/afkstart (1)")]
        [ReFrameSetMaxParticles("Advanced/AFK/afkFire/fire 2", 32, Always = true)]
        [ReFrameSetMaxParticles("Advanced/AFK/afkFire/fire 2 (1)", 32, Always = true)]
        [ReFrameSetMaxParticles("Advanced/AFK/afkFire/fire 2 (2)", 32, Always = true)]
        [ReFrameSetMaxParticles("Advanced/AFK/afkFire/Particle System (2)", 64, Always = true)]
        [ReFrameSetMaxParticles("Advanced/AFK/afkstart (1)", 16, Always = true)]
        [ReFrameQuestParticleMaterial("Advanced/AFK/afkFire/fire 2")]
        [ReFrameQuestParticleMaterial("Advanced/AFK/afkFire/fire 2 (1)")]
        [ReFrameQuestParticleMaterial("Advanced/AFK/afkFire/fire 2 (2)")]
        [ReFrameQuestParticleMaterial("Advanced/AFK/afkFire/Particle System (2)")]
        [ReFrameQuestParticleMaterial("Advanced/AFK/afkstart (1)")]
        [ReFrameLabel("AFK の炎")]
        public ReFrameDeleteEntry afkFire = new() { Enabled = false, Value = 0f };

        [ReFrameLabel("ハンドアニメーション")]
        [ReFrameMenuGroup("IKUSIA_emote", "Hand Animaton")]
        [ReFrameValueLocked(0f)]
        [ReFrameDeleteLayer("hand_animation", 0f)]
        [ReFrameDelete("hand_animation")]
        [ReFrameDelete("HandAnimationMirror", ReFrameParameterType.Bool)]
        public ReFrameDeleteEntry handAnimations = new() { Enabled = false, Value = 0f };
    }
}
