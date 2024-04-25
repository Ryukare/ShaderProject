using System;
using UnityEngine;

public class LightCaster : MonoBehaviour
{
	[SerializeField] private LayerMask layerMaskToIgnore;
	[SerializeField] private LayerMask wallMask;
	[SerializeField] private GameObject lightRays;
	[SerializeField] private float radius;
	[SerializeField] private float offset;
	private Collider[] _sceneObjects;
	private Mesh _lightMesh;
	
	//used for updating the vertices and UVs of the light mesh. The angle variable is for properly sorting the ray hit points.
    private struct AngledVertices{ 
        public Vector3 Vert;
        public float Angle;
        public Vector2 Uv;
    }
    
	private void Start() {
		_lightMesh = lightRays.GetComponent<MeshFilter>().mesh; 
	}
	
    /// <summary>
    /// Adds three ints to the end of an int array.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="itemToAdd1"></param>
    /// <param name="itemToAdd2"></param>
    /// <param name="itemToAdd3"></param>
    /// <returns></returns>
	private static int[] AddItemsToArray (int[] original, int itemToAdd1, int itemToAdd2, int itemToAdd3) {
      int[] finalArray = new int[ original.Length + 3];
      for(var i = 0; i < original.Length; i ++) {
           finalArray[i] = original[i];
      }
      finalArray[original.Length] = itemToAdd1;
      finalArray[original.Length + 1] = itemToAdd2;
      finalArray[original.Length + 2] = itemToAdd3;
      return finalArray;
 	}

    /// <summary>
    /// Adds two arrays together, making a third array.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static Vector3[] ConcatArrays(Vector3[] first, Vector3[] second){
        Vector3[] concatted = new Vector3[first.Length + second.Length];

        Array.Copy(first, concatted, first.Length);
        Array.Copy(second, 0, concatted, first.Length, second.Length);

        return concatted;
     }
    
	private void Update()
    {
	    GetWalls();
	    _lightMesh.Clear(); 
	    
		Vector3[] objectVertices = _sceneObjects[0].GetComponent<MeshFilter>().mesh.vertices;
        for (var i = 1; i < _sceneObjects.Length; i++)
        {
	        objectVertices = ConcatArrays(objectVertices, _sceneObjects[i].GetComponent<MeshFilter>().mesh.vertices);
        }
        
        //these lines (1) an array of structs which will be used to populate the light mesh and (2)
        //the vertices and UVs to ultimately populate the mesh.
        // (the "*2" is because there are twice as many rays casted as vertices, and the "+1" because
        // the first point in the mesh should be the center of the light source)
        AngledVertices[] angleds = new AngledVertices[(objectVertices.Length*2)];
		Vector3[] verts = new Vector3[(objectVertices.Length*2)+1];
        Vector2[] uvs = new Vector2[(objectVertices.Length*2)+1];


        //Store the vertex location and UV of the center of the light source in the first locations of verts and uvs.
		verts[0] = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
		uvs[0] = new Vector2(lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).x, 
			lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).y);

        var h = 0; //a constantly increasing int to use to calculate the current location in the angleds struct array.

        //cycle through all scene objects.
        for (var j = 0; j < _sceneObjects.Length; j++) 
        {
	        //cycle through all vertices in the current scene object.
            for (var i = 0; i < _sceneObjects[j].GetComponent<MeshFilter>().mesh.vertices.Length; i++) 
		    {
			    // just to make the current position shorter to reference.
                Vector3 me = transform.position;
                //get the vertex location in world space coordinates.
                Vector3 other = _sceneObjects[j].transform.localToWorldMatrix.MultiplyPoint3x4(objectVertices[h]); 

                // calculate the angle of the two offsets, to be stored in the structs.
                var angle1 = Mathf.Atan2(((other.y-me.y)-offset),((other.x-me.x)-offset));
                var angle3 = Mathf.Atan2(((other.y-me.y)+offset),((other.x-me.x)+offset));
                //create and fire the two rays from the center of the light source in the direction of the vertex, with offsets.
                RaycastHit hit; 
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)-offset, 
	                (other.y-me.y)-offset), out hit, 100, ~layerMaskToIgnore);
                RaycastHit hit2;
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)+offset, 
	                (other.y-me.y)+offset), out hit2, 100, ~layerMaskToIgnore);
                //store the hit locations as vertices in the struct, in model coordinates, as well as the angle of the ray cast and the UV at the vertex.
                angleds[(h*2)].Vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                angleds[(h*2)].Angle = angle1;
                angleds[(h*2)].Uv = new Vector2(angleds[(h*2)].Vert.x, angleds[(h*2)].Vert.y);

			    angleds[(h*2)+1].Vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit2.point);
                angleds[(h*2)+1].Angle = angle3;
                angleds[(h*2)+1].Uv = new Vector2(angleds[(h*2)+1].Vert.x, angleds[(h*2)+1].Vert.y);

                //increment h.
                h++;
		    }
        }
        
        //sort the struct array of vertices from smallest angle to greatest.
        Array.Sort(angleds, delegate(AngledVertices one, AngledVertices two) 
        {
	        return one.Angle.CompareTo(two.Angle);
        });
        
		//store the values in the struct array in verts and uvs.
        for (var i = 0; i < angleds.Length; i++) 
        {                                       
	        //(offsetting one because index 0 is the center of the light source and triangle fan)
            verts[i+1] = angleds[i].Vert;
            uvs[i+1] = angleds[i].Uv;
        }

        //update the actual mesh with the new vertices.
        _lightMesh.vertices = verts; 

        //offset all the UVs by .5 on both s and t to make the texture center be at the object center.
        for (var i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2 (uvs[i].x + .5f, uvs[i].y + .5f);
        }

        //update the actual mesh with the new UVs.
        _lightMesh.uv = uvs; 
        
        //init the triangles array, starting with the last triangle to orient normals properly.
		int[] triangles = {0,1,verts.Length-1}; 

		//add all triangles to the triangle array, determined by three verts in the vertex array.
		for (var i = verts.Length-1; i > 0; i--) 
		{
			triangles = AddItemsToArray(triangles, 0, i, i-1);
		}

		//update the actual mesh with the new triangles.
		_lightMesh.triangles = triangles; 
  	}

	private void GetWalls()
	{
		_sceneObjects = Physics.OverlapSphere(transform.position, radius, wallMask);
	}
}