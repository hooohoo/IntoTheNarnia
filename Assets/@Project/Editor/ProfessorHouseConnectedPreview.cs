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
    private static readonly string[] Names = {"01_Hall_Front", "02_Hall_UnderStair", "03_Hall_PortraitClearance", "04_Professor_Study", "05_Study_Desk", "06_Private_Library", "07_Professor_Bedroom", "08_Girls_Bedroom", "09_Boys_Bedroom", "10_Girls_Reverse", "11_Hall_EastUnderStair"};
    private static readonly Vector3[] Positions = {new Vector3(17f,1.3f,10.55f), new Vector3(13.5f,1.3f,17.2f), new Vector3(15.6f,4f,19.5f), new Vector3(7f,5.9f,18.5f), new Vector3(4.9f,5.6f,14.5f), new Vector3(6.8f,5.7f,23.1f), new Vector3(8.5f,5.7f,2f), new Vector3(34.25f,5.7f,28.55f), new Vector3(42.75f,5.7f,28.55f), new Vector3(34.25f,5.7f,33.1f), new Vector3(22f,1.25f,18.1f)};
    private static readonly Vector3[] Targets = {new Vector3(17f,3.4f,20.9f), new Vector3(12.7f,1.8f,21f), new Vector3(13.45f,5.6f,21.7f), new Vector3(2.7f,5.65f,17f), new Vector3(3.8f,5.5f,18.1f), new Vector3(3.7f,5.8f,26.4f), new Vector3(4.5f,5.5f,7.5f), new Vector3(34.25f,5.7f,33f), new Vector3(42.75f,5.7f,33f), new Vector3(34.25f,5.7f,28.6f), new Vector3(20.5f,1.9f,21.6f)};

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
