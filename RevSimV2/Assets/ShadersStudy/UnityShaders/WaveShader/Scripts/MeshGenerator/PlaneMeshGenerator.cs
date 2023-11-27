using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlaneMeshGenerator 
{

    // Start is called before the first frame update
    public static Mesh CreateMesh(int _sizeInUnits, int _resolutionPerUnit, float _UVScale = 2, string meshName = "GeneratedPlaneMesh")
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName;

        int points = _sizeInUnits * _resolutionPerUnit;

        mesh.vertices = GenerateVerts(points, _resolutionPerUnit);
        int verticesAmount = mesh.vertices.Length;
        mesh.triangles = GenerateTries(points, verticesAmount);
        mesh.uv = GenerateUVs(points, _UVScale, verticesAmount);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Vector3[] GenerateVerts(int sizeInUnits, int resolution)
    {
        var verts = new Vector3[(sizeInUnits + 1) * (sizeInUnits + 1)];

        //equaly distributed verts
        for(int x = 0; x <= sizeInUnits; x++)
            for(int z = 0; z <= sizeInUnits  ; z++)
                verts[x * (sizeInUnits + 1) + z] = new Vector3(x - sizeInUnits/2, 0, z- sizeInUnits/2) / resolution;

        return verts;
    }

    private static int[] GenerateTries(int sizeInUnits, int verticesAmount)
    {
        var tries = new int[verticesAmount * 6];

        //two triangles are one tile
        for(int x = 0; x < sizeInUnits  ; x++)
        {
            for(int z = 0; z < sizeInUnits  ; z++)
            {
                tries[index(x, z, sizeInUnits) * 6 + 0] = index(x, z, sizeInUnits);
                tries[index(x, z, sizeInUnits) * 6 + 1] = index(x + 1, z + 1, sizeInUnits);
                tries[index(x, z, sizeInUnits) * 6 + 2] = index(x + 1, z, sizeInUnits);
                tries[index(x, z, sizeInUnits) * 6 + 3] = index(x, z, sizeInUnits);
                tries[index(x, z, sizeInUnits) * 6 + 4] = index(x, z + 1, sizeInUnits);
                tries[index(x, z, sizeInUnits) * 6 + 5] = index(x + 1, z + 1, sizeInUnits);
            }
        }

        return tries;
    }

    private static Vector2[] GenerateUVs(int sizeInUnits, float UVScale, int verticesAmount)
    {
        var uvs = new Vector2[verticesAmount];

        //always set one uv over n tiles than flip the uv and set it again
        for (int x = 0; x <= sizeInUnits  ; x++)
        {
            for (int z = 0; z <= sizeInUnits; z++)
            {
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z, sizeInUnits)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }

    private static int index(int x, int z, int sizeInUnits)
    {
        return x * (sizeInUnits + 1) + z;
    }

}
