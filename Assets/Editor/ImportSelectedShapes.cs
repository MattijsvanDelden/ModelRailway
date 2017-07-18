using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;


/*
public class Test : ScriptableObject
{
	[MenuItem("Custom/Test Mesh Generation")]
	public static void TestMeshGeneration()
	{
		// All generated models will be structured after this template
		const string ModelTemplate = "Assets/ModelTemplate.prefab";
		const string PrefabPath = "Assets/TestMeshGeneration.prefab";
		 
		// clone the model template
		Object templatePrefab = AssetDatabase.LoadAssetAtPath(ModelTemplate, typeof(GameObject));
		var template = (GameObject)EditorUtility.InstantiatePrefab(templatePrefab);
	 
		// this way links will persist when we regenerate the mesh
		Object prefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(GameObject));
		if (!prefab) 
		{
			prefab = EditorUtility.CreateEmptyPrefab( PrefabPath );
		}
	 
		// sort of the same...
		var mesh = (Mesh) AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Mesh));
		if (!mesh) 
		{
			mesh = new Mesh { name = "GeneratedMesh" };
			AssetDatabase.AddObjectToAsset (mesh, PrefabPath);
		} else 
		{
			mesh.Clear();
		}
		// generate your mesh in place

		mesh.vertices = new [] 
		{  
			new Vector3(0, 0, 0), 
			new Vector3(1, 0, 0), 
			new Vector3(1, 1, 0), 
			new Vector3(0, 1, 0), 
		};
		mesh.triangles = new []
		{
			0, 1, 2,
			0, 2, 3,
			0, 2, 1,
			0, 3, 2
		};

		// assume that MeshFilter is already there. could check and AddComponent
		template.GetComponent<MeshFilter>().sharedMesh = mesh;
	 
		// make sure 
		EditorUtility.ReplacePrefab(template, prefab, ReplacePrefabOptions.ReplaceNameBased);
		// get rid of the temporary object (otherwise it stays over in scene)
		DestroyImmediate(template);
	}
}
*/



public class ImportSelectedShapes : ScriptableObject 
{
	private const string VisualPathBase = "Assets/TrainCarVisuals";
	private const string DefinitionPathBase	= "Assets/TrainCarDefinitions";

	[MenuItem("Custom/Import Selected Shapes")]
	public static void ImportSelected()
	{
		foreach (Object o in Selection.objects)
		{
			string srcAssetPath = AssetDatabase.GetAssetPath(o);
			if (srcAssetPath.EndsWith(".s"))
			{
				var shapeLoader = new ShapeFileLoader();
				List<ShapeAnimation> animations;
				GameObject shapeObject = shapeLoader.Load(srcAssetPath, "Shape Textures", out animations);

				string visualDestPath = VisualPathBase + "/" + o.name + ".prefab";
				GameObject visualPrefab = PrefabUtility.CreatePrefab(visualDestPath, shapeObject);
				var materials = new List<Material>();
				AddObjectsToAsset(shapeObject, visualPrefab, materials);
				PrefabUtility.ReplacePrefab(shapeObject, visualPrefab, ReplacePrefabOptions.ReplaceNameBased);

				var definitionGameObject = new GameObject(o.name);
				var definition = definitionGameObject.AddComponent<TrainCarDefinition>();
				definition.Animations = animations.ToArray();
				definition.Visual = visualPrefab;
				string definitionDestPath = DefinitionPathBase + "/" + o.name + ".prefab";
				GameObject definitionPrefab = PrefabUtility.CreatePrefab(definitionDestPath, definitionGameObject);
				PrefabUtility.ReplacePrefab(definitionGameObject, definitionPrefab, ReplacePrefabOptions.ReplaceNameBased);
				//AssetDatabase.CreateAsset(prefab, destAssetPath);
				
				DestroyImmediate(shapeObject, true);
				DestroyImmediate(definitionGameObject, true);

				Debug.Log("Imported " + srcAssetPath);
			}
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}



	private static void AddObjectsToAsset(GameObject go, Object asset, List<Material> materials)
	{
		var meshFilter = go.GetComponent<MeshFilter>();
		if (meshFilter != null)
		{
			AssetDatabase.AddObjectToAsset(meshFilter.sharedMesh, asset);
		}
		var renderer = go.GetComponent<MeshRenderer>();
		if (renderer != null)
		{
			if (!materials.Contains(renderer.sharedMaterial))
			{
				AssetDatabase.AddObjectToAsset(renderer.sharedMaterial, asset);
				materials.Add(renderer.sharedMaterial);
			}
		}
		foreach (Transform child in go.transform)
		{
			AddObjectsToAsset(child.gameObject, asset, materials);
		}
	}
}
