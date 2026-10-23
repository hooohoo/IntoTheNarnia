using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class ProfessorHouseConnectedValidation
{
    private static readonly string LogTag = "[ProfessorHouseConnectedValidation]";

    [MenuItem("Narnia/Professor House/Connected Layout/Validate Walkthrough")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().path != ProfessorHouseWalkthroughRoot.ScenePath)
            EditorSceneManager.OpenScene(ProfessorHouseWalkthroughRoot.ScenePath);
        var data = ProfessorHouseConnectedBuilder.ReadManifest();
        Physics.SyncTransforms();
        var report = new List<string>();
        var failures = new List<string>();
        int segments = 0;
        int capsuleSamples = 0;
        for (int r = 0; r < data.Routes.Length; r++)
        {
            var route = data.Routes[r];
            int before = failures.Count;
            for (int i = 1; i < route.Points.Length; i++)
            {
                segments++;
                string label = route.Key + " segment " + i;
                var path = CompletePath(route.Points[i-1], route.Points[i], label, failures);
                if (path == null) continue;
                for (int c = 1; c < path.corners.Length; c++)
                {
                    var a = path.corners[c-1];
                    var b = path.corners[c];
                    int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a,b)/.28f));
                    for (int s = 0; s <= steps; s++)
                    {
                        var foot = Vector3.Lerp(a,b,(float)s/steps);
                        float radius = ProfessorHouseWalkthroughRoot.CharacterRadius;
                        var bottom = foot + Vector3.up * (radius+.065f);
                        var top = foot + Vector3.up * (ProfessorHouseWalkthroughRoot.CharacterHeight-radius);
                        capsuleSamples++;
                        if (Physics.CheckCapsule(bottom,top,radius,~0,QueryTriggerInteraction.Ignore))
                            failures.Add(label + " capsule obstruction at " + foot.ToString("F3"));
                    }
                }
            }
            report.Add((before == failures.Count ? "PASS " : "FAIL ") + route.Key + " ordered path and character clearance");
        }

        int openDoors = 0;
        int closedDoors = 0;
        for (int i = 0; i < data.Doors.Length; i++)
        {
            var door = data.Doors[i];
            float y = door.Floor == "ground" ? 0 : door.Floor == "upper" ? 4.32f : 9;
            var center = new Vector3(door.Point[0],y,door.Point[1]);
            var offset = door.Axis == "h" ? Vector3.forward*.65f : Vector3.right*.65f;
            string label = door.Floor + " " + door.A + " / " + door.B;
            int before = failures.Count;
            bool restricted = door.Restriction == "professor";
            if (!restricted)
            {
                CompletePath(center-offset,center+offset,label,failures);
                openDoors++;
            }
            else
            {
                closedDoors++;
                if (!NavMesh.SamplePosition(center-offset,out var a,.3f,NavMesh.AllAreas) || !NavMesh.SamplePosition(center+offset,out var b,.3f,NavMesh.AllAreas))
                    failures.Add(label + " missing approach surface");
                else
                {
                    var path = new NavMeshPath();
                    if (NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,path) && path.status == NavMeshPathStatus.PathComplete)
                        failures.Add(label + " must remain closed");
                }
            }
            report.Add((before == failures.Count ? "PASS " : "FAIL ") + label + (restricted ? " closed boundary" : " open doorway"));
        }

        var emptyUpperPoints = new[] {new Vector3(16,4.32f,28),new Vector3(36,4.32f,18),new Vector3(16,9,28),new Vector3(36,9,18)};
        for (int i = 0; i < emptyUpperPoints.Length; i++)
        {
            var p = emptyUpperPoints[i];
            if (NavMesh.SamplePosition(p,out var hit,.35f,NavMesh.AllAreas))
                failures.Add("Garden void was covered at " + p);
        }
        var k0Start = new Vector3(29.1f,4.32f,27);
        var k0End = new Vector3(30.8f,4.32f,27);
        if (!NavMesh.SamplePosition(k0Start,out var s2,.35f,NavMesh.AllAreas) || !NavMesh.SamplePosition(k0End,out var k0,.35f,NavMesh.AllAreas) || !NavMesh.Raycast(s2.position,k0.position,out var blocked,NavMesh.AllAreas))
            failures.Add("S2 to K0 must not have a direct shortcut through the wall.");

        Directory.CreateDirectory(ProfessorHouseConnectedBuilder.ReportFolder);
        report.Insert(0,"Connected walkthrough: " + data.RoomCount + " spaces, " + data.DoorCount + " doorways.");
        report.Add("Ordered route segments: " + segments + "; capsule samples: " + capsuleSamples + "; open doors: " + openDoors + "; closed boundaries: " + closedDoors);
        report.Add("Agent: height=1.20 m, radius=0.22 m, climb=0.22 m, slope=40 degrees. Comparison profile, not final Lucy proportions.");
        report.Add("Source layout SHA-256: " + data.SourceLayoutSha256);
        report.Add("Failures: " + failures.Count);
        report.AddRange(failures);
        File.WriteAllText(ProfessorHouseConnectedBuilder.ReportFolder + "/Navigation.txt",string.Join("\n",report)+"\n");
        if (failures.Count > 0)
            throw new InvalidOperationException("Connected navigation validation failed: " + failures.Count + "; first=" + failures[0]);
        LogUtil.Log(LogTag + " CONNECTED_NAVIGATION_VALIDATED segments=" + segments + " capsuleSamples=" + capsuleSamples);
    }

    private static NavMeshPath CompletePath(Vector3 start, Vector3 end, string label, List<string> failures)
    {
        if (!NavMesh.SamplePosition(start,out var a,.35f,NavMesh.AllAreas) || !NavMesh.SamplePosition(end,out var b,.35f,NavMesh.AllAreas))
        {
            failures.Add(label + " endpoint off navigation surface: " + start + " -> " + end);
            return null;
        }
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,path) || path.status != NavMeshPathStatus.PathComplete)
        {
            failures.Add(label + " incomplete path: " + a.position + " -> " + b.position);
            return null;
        }
        float length = 0;
        for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i-1],path.corners[i]);
        if (length > Vector3.Distance(a.position,b.position)*1.8f+1)
            failures.Add(label + " unexpected detour: " + length.ToString("F2") + " metres");
        return path;
    }
}
