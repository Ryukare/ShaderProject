using System;
using UnityEngine;

public class LightCaster : MonoBehaviour
{
	[SerializeField] private LayerMask layerMaskToIgnore;
	[SerializeField] private LayerMask wallMask;
	[SerializeField] private GameObject lightRays;
	[SerializeField] private float radius;
	[SerializeField] private float offset;
	private Collider[] _walls;
	private Mesh _lightMesh;
	private Vector3 _sourcePosition;
	
	//used for updating the vertices and UVs of the light mesh. The angle variable is for properly sorting the ray hit points.
    private struct AngledVertices{ 
        public Vector3 Vertices;
        public float Angle;
        public Vector2 Uv;
    }
    
	private void Start() {
		_lightMesh = lightRays.GetComponent<MeshFilter>().mesh;
		_sourcePosition = transform.position;
	}
	
	// Adds three ints to the end of an int array.
	private static int[] AddItemsToArray (int[] original, int itemToAdd1, int itemToAdd2, int itemToAdd3) {
      int[] finalArray = new int[original.Length + 3];
      for(var i = 0; i < original.Length; i ++) {
           finalArray[i] = original[i];
      }
      finalArray[original.Length] = itemToAdd1;
      finalArray[original.Length + 1] = itemToAdd2;
      finalArray[original.Length + 2] = itemToAdd3;
      return finalArray;
 	}

	// Adds two arrays together, making a third array.
    private static Vector3[] MergeArrays(Vector3[] firstArray, Vector3[] secondArray){
        Vector3[] newArray = new Vector3[firstArray.Length + secondArray.Length];

        Array.Copy(firstArray, newArray, firstArray.Length);
        Array.Copy(secondArray, 0, newArray, firstArray.Length, secondArray.Length);

        return newArray;
     }
    
	private void Update()
    {
	    GetWalls();
	    _lightMesh.Clear();
	    _sourcePosition = transform.position;
	    UpdateLightMesh();
    }

	//Updates light mesh to represent current field view
	private void UpdateLightMesh()
	{
		Vector3[] allWallVertices = _walls[0].GetComponent<MeshFilter>().mesh.vertices;
		
        for (var i = 1; i < _walls.Length; i++)
        {
	        allWallVertices = MergeArrays(allWallVertices, _walls[i].GetComponent<MeshFilter>().mesh.vertices);
        }
        
        AngledVertices[] allAngledVertices = new AngledVertices[allWallVertices.Length * 2];
		Vector3[] vertices = new Vector3[allWallVertices.Length * 2 + 1];
        Vector2[] uvs = new Vector2[allWallVertices.Length * 2 + 1];

        var lightRaysMatrix = lightRays.transform.worldToLocalMatrix;
        
        vertices[0] = lightRaysMatrix.MultiplyPoint3x4(_sourcePosition);
		uvs[0] = new Vector2(lightRaysMatrix.MultiplyPoint3x4(_sourcePosition).x, 
			lightRaysMatrix.MultiplyPoint3x4(_sourcePosition).y);
		
        var allAngledVerticesIndex = 0; 
        foreach (var wall in _walls)
        {
	        foreach (var vertex in wall.GetComponent<MeshFilter>().mesh.vertices)
	        {
		        var wallVertexPosition = wall.transform.localToWorldMatrix.MultiplyPoint3x4(allWallVertices[allAngledVerticesIndex]); 
                
		        var wallVertexDistanceFromSourceX = wallVertexPosition.x - _sourcePosition.x;
		        var wallVertexDistanceFromSourceY = wallVertexPosition.y - _sourcePosition.y;
                
                var angle1 = Mathf.Atan2(wallVertexDistanceFromSourceY - offset,wallVertexDistanceFromSourceX - offset);
                var angle2 = Mathf.Atan2(wallVertexDistanceFromSourceY + offset,wallVertexDistanceFromSourceX + offset);
                
                RaycastHit hit; 
                Physics.Raycast(_sourcePosition, new Vector2(wallVertexDistanceFromSourceX - offset, 
	                wallVertexDistanceFromSourceY - offset), out hit, 100, ~layerMaskToIgnore);
                RaycastHit hit2;
                Physics.Raycast(_sourcePosition, new Vector2(wallVertexDistanceFromSourceX + offset, 
	                wallVertexDistanceFromSourceY + offset), out hit2, 100, ~layerMaskToIgnore);
                
                allAngledVertices[allAngledVerticesIndex * 2].Vertices = lightRaysMatrix.MultiplyPoint3x4(hit.point);
                allAngledVertices[allAngledVerticesIndex * 2].Angle = angle1;
                allAngledVertices[allAngledVerticesIndex * 2].Uv = 
	                new Vector2(allAngledVertices[allAngledVerticesIndex * 2].Vertices.x, 
		                allAngledVertices[allAngledVerticesIndex * 2].Vertices.y);

                allAngledVertices[allAngledVerticesIndex * 2 + 1].Vertices = lightRaysMatrix.MultiplyPoint3x4(hit2.point);
                allAngledVertices[allAngledVerticesIndex * 2 + 1].Angle = angle2;
                allAngledVertices[allAngledVerticesIndex * 2 + 1].Uv = 
	                new Vector2(allAngledVertices[allAngledVerticesIndex * 2 + 1].Vertices.x, 
		                allAngledVertices[allAngledVerticesIndex * 2 + 1].Vertices.y);
                
                allAngledVerticesIndex++;
	        }
        }
        
        Array.Sort(allAngledVertices, (one, two) => one.Angle.CompareTo(two.Angle));
        
        for (var i = 0; i < allAngledVertices.Length; i++) 
        {                                       
	        vertices[i+1] = allAngledVertices[i].Vertices;
            uvs[i+1] = allAngledVertices[i].Uv;
        }
        
        _lightMesh.vertices = vertices; 
        
        for (var i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2 (uvs[i].x + .5f, uvs[i].y + .5f);
        }
        
        _lightMesh.uv = uvs; 
        
		int[] triangles = {0,1,vertices.Length-1}; 
		
		for (var i = vertices.Length-1; i > 0; i--) 
		{
			triangles = AddItemsToArray(triangles, 0, i, i-1);
		}
		
		_lightMesh.triangles = triangles; 
	}

	private void GetWalls()
	{
		_walls = Physics.OverlapSphere(_sourcePosition, radius, wallMask);
	}
}