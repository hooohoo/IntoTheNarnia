using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ProfessorHouseConnectedPreview
{
    private static readonly string LogTag = "[ProfessorHouseConnectedPreview]";
    private static readonly string[] Names = {"01_Hall_Unity", "02_ChildrenRoom_Unity", "03_SecondaryStair_Unity", "04_Wardrobe_Unity"};
    private static readonly Vector3[] Positions = {new Vector3(16.4f,1.05f,10.8f), new Vector3(34.25f,5.37f,28.8f), new Vector3(26.175f,5.37f,27), new Vector3(4,10.05f,22.5f)};
    private static readonly Vector3[] Targets = {new Vector3(17,3.5f,20.9f), new Vector3(34.25f,5.6f,33.4f), new Vector3(26.175f,7,31.7f), new Vector3(4,10.9f,32)};

    [MenuItem("Narnia/Professor House/Connected Layout/Capture Unity Previews")]
    public static void Capture()
    {
        Directory.CreateDirectory(ProfessorHouseConnectedBuilder.ReportFolder);
        var obj = new GameObject("Connected_Preview_Camera");
        var camera = obj.AddComponent<Camera>();
        camera.enabled = false;
        camera.nearClipPlane = .05f;
        camera.farClipPlane = 100;
        camera.fieldOfView = 75;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.76f,.81f,.81f);
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
        try
        {
            for (int i = 0; i < Names.Length; i++)
            {
                camera.transform.position = Positions[i];
                camera.transform.LookAt(Targets[i],Vector3.up);
                CaptureCamera(camera,ProfessorHouseConnectedBuilder.ReportFolder + "/" + Names[i] + ".png");
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(obj); }
        LogUtil.Log(LogTag + " CONNECTED_UNITY_PREVIEWS_COMPLETE");
    }

    private static void CaptureCamera(Camera camera,string path)
    {
        var target = new RenderTexture(1536,1024,24,RenderTextureFormat.ARGB32);
        target.antiAliasing = 4;
        var texture = new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
        var previous = RenderTexture.active;
        try
        {
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest {destination=target});
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);
            texture.Apply();
            bool varied = false;
            var first = texture.GetPixel(0,0);
            for (int y = 0; y < target.height && !varied; y += 64)
                for (int x = 0; x < target.width; x += 64)
                    if (texture.GetPixel(x,y) != first) { varied=true; break; }
            if (!varied) throw new InvalidOperationException("Empty capture: use an editor with graphics, without -batchmode or -nographics.");
            File.WriteAllBytes(path,texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    public static void CaptureBatch()
    {
        EditorSceneManager.OpenScene(ProfessorHouseWalkthroughRoot.ScenePath);
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.LookAt(new Vector3(17,3,19),Quaternion.Euler(8,0,0),12,false,true);
        int frames = 0;
        EditorApplication.CallbackFunction capture = null;
        capture = () =>
        {
            if (++frames < 45) return;
            EditorApplication.update -= capture;
            try { Capture(); EditorApplication.Exit(0); }
            catch (Exception exception) { LogUtil.LogException(exception); EditorApplication.Exit(1); }
        };
        EditorApplication.update += capture;
    }
}
