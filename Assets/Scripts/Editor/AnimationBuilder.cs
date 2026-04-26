using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class AnimationBuilder
{
    public static AnimatorController GeneratePlayerAnimator(string spriteSheetPath)
    {
        string spriteName = System.IO.Path.GetFileNameWithoutExtension(spriteSheetPath);
        string controllerPath = "Assets/Controllers/" + spriteName + "_Controller.controller";
        
        // Create Controller
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // Parameters
        controller.AddParameter("moveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("moveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);

        // Load Sprites
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
        List<Sprite> sprites = assets.OfType<Sprite>().OrderBy(s => {
            string[] parts = s.name.Split('_');
            int idx = int.Parse(parts[parts.Length - 1]);
            return idx;
        }).ToList();

        // CORRECT MAPPING (D-L-R-U)
        // Rows 0-3: Idle
        AnimationClip idleDown = CreateClip(spriteName + "_Idle_Down", sprites.GetRange(0, 4));
        AnimationClip idleLeft = CreateClip(spriteName + "_Idle_Left", sprites.GetRange(4, 4));
        AnimationClip idleRight = CreateClip(spriteName + "_Idle_Right", sprites.GetRange(8, 4));
        AnimationClip idleUp = CreateClip(spriteName + "_Idle_Up", sprites.GetRange(12, 4));
        
        // Rows 4-7: Walk
        AnimationClip walkDown = CreateClip(spriteName + "_Walk_Down", sprites.GetRange(16, 4));
        AnimationClip walkLeft = CreateClip(spriteName + "_Walk_Left", sprites.GetRange(20, 4));
        AnimationClip walkRight = CreateClip(spriteName + "_Walk_Right", sprites.GetRange(24, 4));
        AnimationClip walkUp = CreateClip(spriteName + "_Walk_Up", sprites.GetRange(28, 4));

        // State Machine
        var rootStateMachine = controller.layers[0].stateMachine;

        // IDLE BLEND TREE
        BlendTree idleTree;
        var idleState = controller.CreateBlendTreeInController("IdleTree", out idleTree);
        idleTree.blendType = BlendTreeType.SimpleDirectional2D;
        idleTree.blendParameter = "moveX";
        idleTree.blendParameterY = "moveY";
        
        // Adding children with correct vector mapping
        idleTree.AddChild(idleDown, new Vector2(0, -1));
        idleTree.AddChild(idleUp, new Vector2(0, 1));
        idleTree.AddChild(idleLeft, new Vector2(-1, 0));
        idleTree.AddChild(idleRight, new Vector2(1, 0));

        // WALK BLEND TREE
        BlendTree walkTree;
        var walkState = controller.CreateBlendTreeInController("WalkTree", out walkTree);
        walkTree.blendType = BlendTreeType.SimpleDirectional2D;
        walkTree.blendParameter = "moveX";
        walkTree.blendParameterY = "moveY";
        
        walkTree.AddChild(walkDown, new Vector2(0, -1));
        walkTree.AddChild(walkUp, new Vector2(0, 1));
        walkTree.AddChild(walkLeft, new Vector2(-1, 0));
        walkTree.AddChild(walkRight, new Vector2(1, 0));

        // Transitions
        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        toWalk.duration = 0; // Instant transition
        
        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        toIdle.duration = 0;

        return controller;
    }

    private static AnimationClip CreateClip(string name, List<Sprite> frames)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = name;
        
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Count];
        float frameTime = 1f / 8f; 
        for (int i = 0; i < frames.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i * frameTime;
            keyframes[i].value = frames[i];
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
        
        string clipPath = "Assets/Animations/" + name + ".anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
        {
            AssetDatabase.DeleteAsset(clipPath);
        }
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }
}
