#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CreateSmoothOctagon : EditorWindow
{
    // ---------- Geometry ----------
    [Header("Shape")]
    float radius = 5f;                 // center -> vertex (XZ)
    bool usePrefabThickness = true;    // read Y size from Floor_G
    float thickness = 0.25f;           // if not using prefab
    bool placeBottomOnY0 = true;       // put the bottom face at Y=0

    // ---------- Top/Bottom UVs ----------
    [Header("Top/Bottom UVs")]
    bool autoUseFloorGTileSize = true; // auto-measure tile width from Floor_G
    float topTileWorldSize = 1f;       // world units per UV repeat on the top/bottom
    Vector2 topUvOffset = Vector2.zero;

    // ---------- Side UVs ----------
    public enum SideScaleMode { MatchTop, EvenPerimeter, FixedPerEdge }
    [Header("Side UVs")]
    SideScaleMode sideMode = SideScaleMode.MatchTop;
    float sideVTileWorld = 1f;         // repeats across thickness (V)
    int tilesPerEdge = 3;              // for FixedPerEdge
    bool snapRadiusToTiles = true;     // make each edge physically = tilesPerEdge * tileWidth

    // ---------- Extras ----------
    [Header("Extras")]
    bool addCollider = true;
    GameObject floorG;                  // copy materials, thickness, and tile size from here
    string savePath = "Assets/User-Created Prefabs/Octagon_Floor_Smooth.prefab";

    float measuredTileSize = 1f;        // read-only display

    [MenuItem("Tools/Floors/Create Smooth Octagon")]
    public static void ShowWindow() => GetWindow<CreateSmoothOctagon>("Smooth Octagon");

    void OnGUI()
    {
        GUILayout.Label("Octagon Prism", EditorStyles.boldLabel);

        // Geometry
        radius = EditorGUILayout.FloatField("Radius (to vertex)", Mathf.Max(0.05f, radius));
        usePrefabThickness = EditorGUILayout.Toggle("Use Floor_G Thickness", usePrefabThickness);
        EditorGUI.BeginDisabledGroup(usePrefabThickness);
        thickness = EditorGUILayout.FloatField("Custom Thickness", Mathf.Max(0.001f, thickness));
        EditorGUI.EndDisabledGroup();
        placeBottomOnY0 = EditorGUILayout.Toggle("Place Bottom at Y=0", placeBottomOnY0);

        EditorGUILayout.Space();
        GUILayout.Label("Top/Bottom UVs", EditorStyles.boldLabel);
        floorG = (GameObject)EditorGUILayout.ObjectField("Floor_G Prefab", floorG, typeof(GameObject), false);

        autoUseFloorGTileSize = EditorGUILayout.Toggle("Use Floor_G Tile Width", autoUseFloorGTileSize);
        if (autoUseFloorGTileSize)
        {
            if (floorG != null) measuredTileSize = Mathf.Max(0.001f, MeasureTileWorldSize(floorG));
            EditorGUILayout.LabelField("Measured Tile Width (world units)", measuredTileSize.ToString("0.###"));
            topTileWorldSize = measuredTileSize; // drive from measurement
        }
        else
        {
            topTileWorldSize = EditorGUILayout.FloatField("Top Tile World Size", Mathf.Max(0.001f, topTileWorldSize));
            measuredTileSize = topTileWorldSize;
        }
        topUvOffset = EditorGUILayout.Vector2Field("Top UV Offset (tiles)", topUvOffset);

        EditorGUILayout.Space();
        GUILayout.Label("Side UVs", EditorStyles.boldLabel);
        sideMode = (SideScaleMode)EditorGUILayout.EnumPopup("Side Scale Mode", sideMode);
        if (sideMode == SideScaleMode.FixedPerEdge)
        {
            tilesPerEdge = Mathf.Max(1, EditorGUILayout.IntField("Tiles Per Edge", tilesPerEdge));
            snapRadiusToTiles = EditorGUILayout.Toggle("Snap Radius To Tiles", snapRadiusToTiles);
        }
        sideVTileWorld = EditorGUILayout.FloatField("Vertical Tile (thickness) World Size", Mathf.Max(0.001f, sideVTileWorld));

        EditorGUILayout.Space();
        addCollider = EditorGUILayout.Toggle("Add MeshCollider", addCollider);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Create in Scene + Save Prefab"))
            CreateAndSave();
    }

    void CreateAndSave()
    {
        // Use Floor_G thickness if requested
        float h = thickness;
        if (usePrefabThickness && floorG != null)
        {
            var r = floorG.GetComponentInChildren<Renderer>();
            if (r != null) h = Mathf.Max(0.001f, r.bounds.size.y);
        }

        // Make sure we have the actual tile width
        float tileSize = Mathf.Max(0.001f, autoUseFloorGTileSize && floorG != null ? MeasureTileWorldSize(floorG) : topTileWorldSize);
        topTileWorldSize = tileSize; // keep top/side in sync

        // Optionally snap radius so each edge physically equals tilesPerEdge * tileSize
        if (sideMode == SideScaleMode.FixedPerEdge && snapRadiusToTiles)
        {
            // For a regular 8-gon: edgeLen = 2 * R * sin(π/8)
            float targetEdge = tilesPerEdge * tileSize;
            radius = targetEdge / (2f * Mathf.Sin(Mathf.PI / 8f));
        }

        float yB = placeBottomOnY0 ? 0f : -h * 0.5f;
        float yT = yB + h;

        // Build octagon rings
        const int n = 8;
        float a0 = Mathf.PI / 8f; // 22.5°, flat top
        var ringT = new Vector3[n];
        var ringB = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            float a = a0 + i * (2f * Mathf.PI / n);
            float x = radius * Mathf.Cos(a);
            float z = radius * Mathf.Sin(a);
            ringT[i] = new Vector3(x, yT, z);
            ringB[i] = new Vector3(x, yB, z);
        }

        // Edge lengths + perimeter
        float[] edgeLen = new float[n];
        float perimeter = 0f;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            float len = Vector3.Distance(ringT[i], ringT[j]);
            edgeLen[i] = len;
            perimeter += len;
        }

        // Top/bottom UVs: world-space (tileSize units per UV)
        Vector2 UVTop(Vector3 p) => new Vector2(p.x / tileSize, p.z / tileSize) + topUvOffset;

        // Mesh buffers
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs   = new List<Vector2>();
        var tris  = new List<int>();

        // Top center + ring
        int topCenter = verts.Count; verts.Add(new Vector3(0f, yT, 0f)); norms.Add(Vector3.up); uvs.Add(UVTop(new Vector3(0f, yT, 0f)));
        int topStart  = verts.Count;
        for (int i = 0; i < n; i++) { verts.Add(ringT[i]); norms.Add(Vector3.up); uvs.Add(UVTop(ringT[i])); }

        // Bottom center + ring
        int botCenter = verts.Count; verts.Add(new Vector3(0f, yB, 0f)); norms.Add(Vector3.down); uvs.Add(UVTop(new Vector3(0f, yB, 0f)));
        int botStart  = verts.Count;
        for (int i = 0; i < n; i++) { verts.Add(ringB[i]); norms.Add(Vector3.down); uvs.Add(UVTop(ringB[i])); }

        // Top fan
        for (int i = 0; i < n; i++)
        {
            int i1 = topStart + i;
            int i2 = topStart + ((i + 1) % n);
            tris.Add(topCenter); tris.Add(i2); tris.Add(i1);
        }

        // Bottom fan
        for (int i = 0; i < n; i++)
        {
            int i1 = botStart + i;
            int i2 = botStart + ((i + 1) % n);
            tris.Add(botCenter); tris.Add(i1); tris.Add(i2);
        }

        // --- Side walls ---
        float v0 = 0f;
        float v1 = (yT - yB) / Mathf.Max(0.001f, sideVTileWorld);
        float uAccumWorld = 0f; // for EvenPerimeter

        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            Vector3 t0 = ringT[i], t1 = ringT[next];
            Vector3 b0 = ringB[i], b1 = ringB[next];

            // U range per edge
            float u0, u1;
            if (sideMode == SideScaleMode.MatchTop)
            {
                // 1 UV repeat per tileSize world units
                u0 = 0f;
                u1 = edgeLen[i] / tileSize;
            }
            else if (sideMode == SideScaleMode.EvenPerimeter)
            {
                u0 = (uAccumWorld) / tileSize;
                u1 = (uAccumWorld + edgeLen[i]) / tileSize;
                uAccumWorld += edgeLen[i];
            }
            else // FixedPerEdge
            {
                u0 = 0f;
                u1 = tilesPerEdge; // exactly N repeats per side
            }

            Vector3 outward = Vector3.Cross(t1 - t0, b0 - t0).normalized;

            int v00 = verts.Count; verts.Add(b0); norms.Add(outward); uvs.Add(new Vector2(u0, v0));
            int v01 = verts.Count; verts.Add(t0); norms.Add(outward); uvs.Add(new Vector2(u0, v1));
            int v11 = verts.Count; verts.Add(t1); norms.Add(outward); uvs.Add(new Vector2(u1, v1));
            int v10 = verts.Count; verts.Add(b1); norms.Add(outward); uvs.Add(new Vector2(u1, v0));

            tris.Add(v00); tris.Add(v01); tris.Add(v11);
            tris.Add(v00); tris.Add(v11); tris.Add(v10);
        }

        // Build mesh
        var mesh = new Mesh { name = "Octagon_Mesh" };
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        // Scene object
        var go = new GameObject("Octagon_Floor_Smooth");
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;

        if (floorG != null)
        {
            var src = floorG.GetComponentInChildren<Renderer>();
            if (src != null) mr.sharedMaterials = src.sharedMaterials;
        }

        if (addCollider)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        // Save prefab
        var folder = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);

        Selection.activeObject = prefab != null ? prefab : go;
        EditorGUIUtility.PingObject(Selection.activeObject);

        Debug.Log($"✅ Octagon created. tileSize={tileSize:0.###}, sideMode={sideMode}, radius={radius:0.###}");
    }

    // Measure Floor_G tile width in world units looking at the top footprint
    float MeasureTileWorldSize(GameObject floorPrefab)
    {
        // Instantiate temporarily to read bounds at scale 1
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
        float size = 1f;
        try
        {
            var r = temp.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                // Choose the larger horizontal dimension as "tile width"
                size = Mathf.Max(r.bounds.size.x, r.bounds.size.z);
            }
        }
        finally
        {
            if (temp != null) Object.DestroyImmediate(temp);
        }
        return size;
    }
}
#endif
