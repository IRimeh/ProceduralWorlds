using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class World : MonoBehaviour
{
    public static World world;

    [Header("General")]
    public float WorldRadius = 25.0f;
    [Range(2, 100)]
    public int Points = 30;
    public float AtmosphereSize = 10.0f;

    [Header("Terrain")]
    [Range(0, 128)]
    public int seed = 0;
    public DeformationSettings DeformationSettings;
    public DeformationSettings RandomDeformationSettings;
    
    [Header("Colors")]
    public ColorSettings colorSettings;

    [Header("Generation")]
    public bool autoUpdate = false;
    public bool generate = false;
    public bool generateRandom = false;

    //Mesh data
    private List<Vector3> points = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();
    private List<Vector3> normals = new List<Vector3>();

    //Components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshFilter waterMeshFilter;
    private MeshRenderer waterMeshRenderer;
    private MeshFilter atmosphereMeshFilter;
    private MeshRenderer atmosphereMeshRenderer;


    private Deformer deformer;
    private ColorController colorController;

    private float delta;
    private float maxHeight = float.MinValue;
    private float minHeight = float.MaxValue;

    private void OnEnable()
    {
        world = this;
        deformer = new Deformer(DeformationSettings, seed);
        colorController = new ColorController(colorSettings, 64);
    }


    private void Start()
    {
        RandomDeformationSettings.Randomize();
        Generate(RandomDeformationSettings);
    }


    private void OnValidate()
    {
       
        if (autoUpdate)
        {
            Generate(DeformationSettings);
        }

        if(generate)
        {
            Generate(DeformationSettings);
            generate = false;
        }

        if(generateRandom)
        {
            

            generateRandom = false;
        }
    }


    public void Generate(DeformationSettings deformationSettings)
    {
        GenerateWorld(deformationSettings);
        GenerateColors();
    }

    public void GenerateRandom()
    {
        RandomDeformationSettings.Randomize();
        Generate(RandomDeformationSettings);
        GenerateColors();
    }


    public void GenerateColors()
    {
        if (colorController == null)
            colorController = new ColorController(colorSettings, 64);
        colorController.UpdateSettings(colorSettings);
        colorController.ApplyColorSettings(meshRenderer.sharedMaterial, minHeight, maxHeight);
    }


    public void GenerateWorld(DeformationSettings deformationSettings)
    {
        if (deformer == null)
            deformer = new Deformer(deformationSettings, seed);
        deformer.UpdateSettings(deformationSettings, seed);

        float delt1 = (WorldRadius * 0.5f) * (WorldRadius * 0.5f);
        float delt2 = delt1 + delt1;

        float delt = delt1 + delt2;
        delta = Mathf.Sqrt(delt);

        //World Generation
        ClearLists();
        GeneratePoints(100);
        CurvePointsToSphere();
        TerrainDeformation(deformer);
        GenerateTriangles();
        CalculateNormals();
        GenerateMesh();


        //Atmosphere Generation
        ClearLists();
        GeneratePoints(30);
        CurvePointsToSphere();
        GenerateTriangles();
        CalculateNormals();
        GenerateWaterMesh();

        //Atmosphere Generation
        ClearLists();
        GeneratePoints(30);
        CurvePointsToSphere(AtmosphereSize);
        GenerateTriangles();
        CalculateNormals();
        GenerateAtmosphereMesh();
    }


    private void GenerateMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (!meshFilter)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!meshRenderer)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();


        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }


    private void GenerateWaterMesh()
    {
        waterMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        waterMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        if (!waterMeshFilter)
            waterMeshFilter = transform.GetChild(0).gameObject.AddComponent<MeshFilter>();
        if (!waterMeshRenderer)
            waterMeshRenderer = transform.GetChild(0).gameObject.AddComponent<MeshRenderer>();


        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        waterMeshFilter.mesh = mesh;
    }


    private void GenerateAtmosphereMesh()
    {
        atmosphereMeshFilter = transform.GetChild(1).GetComponent<MeshFilter>();
        atmosphereMeshRenderer = transform.GetChild(1).GetComponent<MeshRenderer>();
        if (!atmosphereMeshFilter)
            atmosphereMeshFilter = transform.GetChild(1).gameObject.AddComponent<MeshFilter>();
        if (!atmosphereMeshRenderer)
            atmosphereMeshRenderer = transform.GetChild(1).gameObject.AddComponent<MeshRenderer>();


        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        atmosphereMeshFilter.mesh = mesh;
    }




    private void ClearLists()
    {
        points.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }




    private void GeneratePoints(float maxPoints)
    {
        //Top
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3((float)x / ((float)Points - 1.0f), 0, (float)y / ((float)Points - 1.0f));
                points.Add((pos * WorldRadius) + new Vector3(-WorldRadius * 0.5f, WorldRadius * 0.5f, -WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }

        //Bottom
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3((float)x / ((float)Points - 1.0f), 0, (float)y / ((float)Points - 1.0f));
                points.Add((pos * WorldRadius) + new Vector3(-WorldRadius * 0.5f, -WorldRadius * 0.5f, -WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }

        //Front
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3((float)x / ((float)Points - 1.0f), (float)y / ((float)Points - 1.0f), 0);
                points.Add((pos * WorldRadius) + new Vector3(-WorldRadius * 0.5f, -WorldRadius * 0.5f, -WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }

        //Back
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3((float)x / ((float)Points - 1.0f), (float)y / ((float)Points - 1.0f), 0);
                points.Add((pos * WorldRadius) + new Vector3(-WorldRadius * 0.5f, -WorldRadius * 0.5f, WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }

        //Right
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3(0, (float)y / ((float)Points - 1.0f), (float)x / ((float)Points - 1.0f));
                points.Add((pos * WorldRadius) + new Vector3(WorldRadius * 0.5f, -WorldRadius * 0.5f, -WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }

        //Left
        for (int x = 0; x < Points; x++)
        {
            for (int y = 0; y < Points; y++)
            {
                Vector3 pos = new Vector3(0, (float)y / ((float)Points - 1.0f), (float)x / ((float)Points - 1.0f));
                points.Add((pos * WorldRadius) + new Vector3(-WorldRadius * 0.5f, -WorldRadius * 0.5f, -WorldRadius * 0.5f));

                uvs.Add(new Vector2(0, 0));
                colors.Add(new Color(0, 0, 0, 1));
            }
        }
    }


    private void GenerateTriangles()
    {
        int faces = 6;
        for (int i = 0; i < faces; i++)
        {
            int faceOffset = i * (Points * Points);
            bool inverse = i % 2 != 0;

            for (int y = 0; y < Points; y++)
            {
                for (int x = 0; x < Points; x++)
                {
                    if (y + 1 < Points)
                    {
                        if (x - 1 >= 0)
                        {
                            if (inverse)
                            {
                                triangles.Add(x + (Points * y) + faceOffset);
                                triangles.Add((x - 1) + (Points * (y + 1)) + faceOffset);
                                triangles.Add(x + (Points * (y + 1)) + faceOffset);
                            }
                            else
                            {
                                triangles.Add(x + (Points * (y + 1)) + faceOffset);
                                triangles.Add((x - 1) + (Points * (y + 1)) + faceOffset);
                                triangles.Add(x + (Points * y) + faceOffset);
                            }
                        }

                        if (x + 1 < Points)
                        {
                            if (inverse)
                            {
                                triangles.Add(x + (Points * y) + faceOffset);
                                triangles.Add(x + (Points * (y + 1)) + faceOffset);
                                triangles.Add((x + 1) + (Points * y) + faceOffset);
                            }
                            else
                            {
                                triangles.Add((x + 1) + (Points * y) + faceOffset);
                                triangles.Add(x + (Points * (y + 1)) + faceOffset);
                                triangles.Add(x + (Points * y) + faceOffset);
                            }
                        }
                    }
                }
            }
        }
    }



    private void CurvePointsToSphere(float additionalOffset = 0)
    {
        for (int i = 0; i < points.Count; i++)
        {
            float perc = 1.0f - (points[i].magnitude / delta);
            float offset = perc * (WorldRadius * 0.85f);
            points[i] += points[i].normalized * (offset + additionalOffset);
        }
    }



    private void TerrainDeformation(Deformer deformer)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = deformer.GetDeformedPoint(points[i]);
            maxHeight = Mathf.Max(maxHeight, points[i].magnitude);
            minHeight = Mathf.Min(minHeight, points[i].magnitude);
        }
    }



    private void CalculateNormals()
    {
        for (int i = 0; i < points.Count; i++)
        {
            normals.Add(points[i].normalized);
        }
    }
}
