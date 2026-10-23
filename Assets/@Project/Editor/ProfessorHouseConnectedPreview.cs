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
    private static readonly string[] Names = {"01_Professor_Study", "02_Study_Ceiling", "03_Artifact_Gallery", "04_Armor_Window", "05_Gallery_Hall_Exit", "06_Gallery_Collections", "07_Escape_Corridor", "08_Family_Corridor", "09_East_Corridor", "10_Children_Corridor", "11_Front_Gallery"};
    private static readonly Vector3[] Positions = {new Vector3(7f,5.9f,18.5f), new Vector3(6.8f,5.8f,13.1f), new Vector3(8.7f,1.5f,12f), new Vector3(5.1f,1.4f,16f), new Vector3(2.2f,1.5f,20.1f), new Vector3(5.1f,1.4f,17.7f), new Vector3(27f,1.35f,11.5f), new Vector3(11.6f,5.7f,8f), new Vector3(47f,5.7f,10.9f), new Vector3(48.5f,5.65f,27f), new Vector3(35f,1.4f,8f)};
    private static readonly Vector3[] Targets = {new Vector3(2.7f,5.65f,17f), new Vector3(3.3f,7.1f,17.5f), new Vector3(3.8f,1.7f,18.8f), new Vector3(2.2f,1.4f,18f), new Vector3(9.6f,1.85f,14f), new Vector3(5.1f,1.8f,21.8f), new Vector3(27f,1.8f,25.5f), new Vector3(32.5f,6.0f,8f), new Vector3(47f,6.05f,25f), new Vector3(31f,5.8f,27f), new Vector3(49f,1.7f,8f)};

    [MenuItem("Narnia/Professor House/Connected Layout/Capture Unity Previews")]
    public static void Capture()
    {
        Directory.CreateDirectory(ProfessorHouseConnectedBuilder.ReportFolder);
        var obj = new GameObject("Connected_Preview_Camera");
        var camera = obj.AddComponent<Camera>();
        camera.enabled = false;
        camera.nearClipPlane = .05f;
        camera.farClipPlane = 100;
        camera.fieldOfView = 70;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.76f,.81f,.81f);
        ProfessorHouseHallFinishBuilder.ConfigureCamera(camera);
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
