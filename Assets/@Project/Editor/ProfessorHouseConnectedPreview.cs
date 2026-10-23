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
    private static readonly string[] Names = {"01_Arrival_Foyer", "02_Foyer_Welcome", "03_Reception", "04_Reception_Fireplace", "05_Garden_Gallery_Route", "06_Dining_Room", "07_Dining_Table", "08_Public_Library", "09_Library_Window_Nook", "10_Library_Connections", "11_Gallery_Reception"};
    private static readonly Vector3[] Positions = {new Vector3(17f,1.55f,1.35f), new Vector3(17f,1.5f,8.7f), new Vector3(8.3f,1.5f,7.9f), new Vector3(4.1f,1.4f,4.5f), new Vector3(1.2f,1.45f,7.5f), new Vector3(25.7f,1.5f,8.7f), new Vector3(32.8f,2.05f,1.25f), new Vector3(8.2f,1.5f,23.4f), new Vector3(6.7f,1.6f,29.3f), new Vector3(2.6f,1.5f,29.1f), new Vector3(5f,1.5f,12.5f)};
    private static readonly Vector3[] Targets = {new Vector3(17f,1.8f,10.5f), new Vector3(20f,1.65f,3f), new Vector3(5.1f,1.6f,2.9f), new Vector3(9.2f,1.7f,2.8f), new Vector3(6.1f,1.7f,9.6f), new Vector3(29.6f,1.5f,4.8f), new Vector3(29.5f,1.2f,4.8f), new Vector3(4.4f,1.65f,29.8f), new Vector3(3.2f,1.3f,32f), new Vector3(8.4f,1.6f,27.5f), new Vector3(5f,1.6f,6f)};

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
