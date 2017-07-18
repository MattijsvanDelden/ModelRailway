using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using UnityEngine;



internal enum ShapeChunkType
{
    error,
    comment,
    point,
    vector,
    quat,
    normals,
    normal_idxs,
    points,
    uv_point,
    uv_points,
    colour,
    colours,
    packed_colour,
    image,
    images,
    texture,
    textures,
    light_material,
    light_materials,
    linear_key,
    tcb_key,
    linear_pos,
    tcb_pos,
    slerp_rot,
    tcb_rot,
    controllers,
    anim_node,
    anim_nodes,
    animation,
    animations,
    anim,
    lod_controls,
    lod_control,
    distance_levels_header,
    distance_level_header,
    dlevel_selection,
    distance_levels,
    distance_level,
    sub_objects,
    sub_object,
    sub_object_header,
    geometry_info,
    geometry_nodes,
    geometry_node,
    geometry_node_map,
    cullable_prims,
    vtx_state,
    vtx_states,
    vertex,
    vertex_uvs,
    vertices,
    vertex_set,
    vertex_sets,
    primitives,
    prim_state,
    prim_states,
    prim_state_idx,
    indexed_point_list,
    point_list,
    indexed_line_list,
    indexed_trilist,
    tex_idxs,
    tri,
    vertex_idxs,
    flags,
    matrix,
    matrices,
    hierarchy,
    volumes,
    vol_sphere,
    shape_header,
    shape,
    shader_names,
    shader_name,
    texture_filter_names,
    texture_filter_name,
    sort_vectors,
    uvop_arg_sets,
    uvop_arg_set,
    light_model_cfgs,
    light_model_cfg,
    uv_ops,
    uvop_copy,
    uv_op_share,
    uv_op_copy,
    uv_op_uniformscale,
    uv_op_user_uninformscale,
    uv_op_nonuniformscale,
    uv_op_user_nonuninformscale,
    uv_op_transform,
    uv_op_user_transform,
    uv_op_reflectxy,
    uv_op_reflectmap,
    uv_op_reflectmapfull,
    uv_op_spheremap,
    uv_op_spheremapfull,
    uv_op_specularmap,
    uv_op_embossbump,
    user_uv_args,
    io_dev,
    io_map,
    sguid,
    dlev_cfg_table,
    dlev_cfg,
    subobject_shaders,
    subobject_light_cfgs,
    shape_named_data,
    shape_named_data_header,
    shape_named_geometry,
    shape_geom_ref,
    material_palette,
    blend_config,
    blend_config_header,
    filtermode_cfgs,
    filter_mode_cfg,
    blend_mode_cfgs,
    blend_mode_cfg,
    texture_stage_progs,
    texture_stage_prog,
    blend_mode_cfg_refs,
    shader_cfgs,
    shader_cfg,
    texture_slots,
    texture_slot,
    named_filter_modes,
    named_filter_mode,
    filtermode_cfg_refs,
    filtermode_cfg_ref,
    named_shaders,
    named_shader,
    shader_cfg_refs,
    shader_cfg_ref,

	EndOfChunk = 99999,
	Unknown
}

public class ShapeFileLoader
{
	private struct VertexState
	{
		public int Flags;
		public int MatrixIndex;
	}



	private class PrimitiveState
	{
		public int		Flags;
		public int		ShaderIndex;
		public int		TextureIndexCount;
		public int[]	TextureIndices;
		public float	Zbias;
		public int		VertexStateIndex;
		public int		AlphaTestMode;
		public int		LightConfigurationIndex;
		public int		ZBufferMode;
	} ;



	private void AddChildToGameObject(int parentIndex, Transform child)
	{
		Vector3 position = child.localPosition;
		child.parent = m_gameObjects[parentIndex].transform;
		child.localPosition = position;
	}



	private bool LoadShapeChunk()
	{
		LoadChildChunks();
		return false;
	}



	private bool LoadPointsChunk()
	{
		int pointCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < pointCount ; k++)
		{
			LoadChunk();
		}
		return true;
	}



	private bool LoadPointChunk()
	{
		float x = m_chunkReader.ReadFloat();
		float y = m_chunkReader.ReadFloat();
		float z = m_chunkReader.ReadFloat();

		m_points.Add(m_posZtoPosXRotation * new Vector3(x, y, z));

		return true;
	}



	private bool LoadUVPointsChunk()
	{
		int uvPointCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < uvPointCount ; k++)
		{
			LoadChunk();
		}
		return true;
	}



	private bool LoadUVPointChunk()
	{
		float u = m_chunkReader.ReadFloat();
		float v = m_chunkReader.ReadFloat();
		m_uvPoints.Add(new Vector2(u, v));

		return true;
	}



	private bool LoadNormalsChunk()
	{
		int normalCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < normalCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadVectorChunk()
	{
		float x = m_chunkReader.ReadFloat();
		float y = m_chunkReader.ReadFloat();
		float z = m_chunkReader.ReadFloat();

		m_normals.Add(m_posZtoPosXRotation * new Vector3(x, y, z));

		return true;
	}



	private bool LoadMatricesChunk()
	{
		int matrixCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < matrixCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadMatrixChunk(string name)
	{
		var m = new Matrix4x4
    	{
    		m00 = m_chunkReader.ReadFloat(),
    		m01 = m_chunkReader.ReadFloat(),
    		m02 = m_chunkReader.ReadFloat(),
    		m10 = m_chunkReader.ReadFloat(),
    		m11 = m_chunkReader.ReadFloat(),
    		m12 = m_chunkReader.ReadFloat(),
    		m20 = m_chunkReader.ReadFloat(),
    		m21 = m_chunkReader.ReadFloat(),
    		m22 = m_chunkReader.ReadFloat(),
    		m30 = m_chunkReader.ReadFloat(),
    		m31 = m_chunkReader.ReadFloat(),
    		m32 = m_chunkReader.ReadFloat(),
    		m33 = 1
    	};

		// TODO use all of the matrix, not just the translation component!

		var translation = m.GetRow(3);

		var gameObject = new GameObject(name);
		gameObject.transform.localPosition = m_posZtoPosXRotation * translation;
		m_gameObjects.Add(gameObject);

		return true;
	}



	private bool LoadLODControlsChunk()
	{
		int lodControlCount = m_chunkReader.ReadInt();
		Debug.Assert(lodControlCount == 1);
		for (int k = 0 ; k < lodControlCount ; k++)
			LoadChunk();

		return true;
	}



	private void LoadChildChunks()
	{
		for (;;)
		{
			if (!LoadChunk())
				break;
		}
	}



	private bool LoadLODControlChunk()
	{
		LoadChildChunks();
		return false;
	}



	private bool LoadDistanceLevelsChunk()
	{
		int distanceLevelsCount = m_chunkReader.ReadInt();
		m_currentDistanceLevelIndex = 0;
		for (int k = 0 ; k < distanceLevelsCount ; k++)
		{
			LoadChunk();
			m_currentDistanceLevelIndex++;
		}

		return true;
	}



	private bool LoadDistanceLevelChunk()
	{
		LoadChildChunks();
		return false;
	}



	private bool LoadSubObjectsChunk()
	{
		int subObjectCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < subObjectCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadSubObjectChunk()
	{
		m_geometryVertexPositions.Clear();
		m_geometryVertexNormals.Clear();
		m_geometryVertexUVs.Clear();
		LoadChildChunks();
		return false;
	}



	private bool LoadVerticesChunk()
	{
		int vertexCount = m_chunkReader.ReadInt();
		for (int k = 0 ; k < vertexCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadVertexChunk()
	{
		m_chunkReader.ReadInt(); // Flags
		int pointIndex = m_chunkReader.ReadInt();
		int normalIndex = m_chunkReader.ReadInt();
		m_chunkReader.ReadColour(); // Colour1
		m_chunkReader.ReadColour(); // Colour2

		m_geometryVertexPositions.Add(m_points[pointIndex]);
		m_geometryVertexNormals.Add(m_normals[normalIndex]);

		LoadChildChunks();

		return false;
	}



	private bool LoadVertexUVsChunk()
	{
		int uvCount = m_chunkReader.ReadInt();
		while (m_geometryVertexUVs.Count == 0 || m_geometryVertexUVs.Count < uvCount)
			m_geometryVertexUVs.Add(new List<Vector2>());

		if (uvCount == 0)
		{
			// No UV coords for this vertex. Add UVs anyway to
			// keep the number of vertex UVs equal to the number
			// of vertex positions and normals
			m_geometryVertexUVs[0].Add(new Vector2());
		}
		else if (uvCount == 1)
		{
			int uvPointIndex = m_chunkReader.ReadInt();
			m_geometryVertexUVs[0].Add(m_uvPoints[uvPointIndex]);
		}
		else
		{
			Debug.Assert(false);  // TODO handle this case
		}

		return true;
	}



	private bool LoadPrimitivesChunk()
	{
		int primitiveCount = m_chunkReader.ReadInt();

		m_currentPrimitiveStateIndex = 0 ;
		for (int k = 0 ; k < primitiveCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadIndexedTrilistChunk()
	{
		m_geometryVertexIndices.Clear();

		LoadChildChunks();

		if (m_currentDistanceLevelIndex == 0)
		{
			// Each indexed trilist becomes a geometry node

			// We only add the vertices that are actually used in this geometry node.
			// This also means that the indices must be renumbered.
			int uniqueIndexCount = 0;
			var indexRenumbering = new Dictionary<int, int>();
			var usedVertexIndices = new List<int>();
			for (int indexIndex = 0 ; indexIndex < m_geometryVertexIndices.Count ; indexIndex++)
			{
				int index = m_geometryVertexIndices[indexIndex];
				if (!indexRenumbering.ContainsKey(index))
				{
					indexRenumbering.Add(index, uniqueIndexCount);	// original index, renumbered index
					usedVertexIndices.Add(index);
					uniqueIndexCount++;
				}
			}

			Debug.Assert(m_geometryVertexUVs.Count <= 2);

			int vertexCount = uniqueIndexCount;
			var positions	= new Vector3[vertexCount];
			var normals		= new Vector3[vertexCount];
			var uv1			= m_geometryVertexUVs.Count >= 1 ? new Vector2[vertexCount] : null;
			var	uv2			= m_geometryVertexUVs.Count == 2 ? new Vector2[vertexCount] : null;

			for (int vertexIndex = 0 ; vertexIndex < vertexCount ; vertexIndex++)
			{
				int sourceVertexIndex = usedVertexIndices[vertexIndex];
				positions[vertexIndex] = m_geometryVertexPositions[sourceVertexIndex];
				normals[vertexIndex] = m_geometryVertexNormals[sourceVertexIndex];

				if (m_geometryVertexUVs.Count == 1)
				{
					 uv1[vertexIndex] = new Vector2(m_geometryVertexUVs[0][sourceVertexIndex].x, 1 - m_geometryVertexUVs[0][sourceVertexIndex].y);
				}
				else if (m_geometryVertexUVs.Count == 2)
				{
					 uv2[vertexIndex] = new Vector2(m_geometryVertexUVs[1][sourceVertexIndex].x, 1 - m_geometryVertexUVs[1][sourceVertexIndex].y);
				}
			}

			var indices = new int[m_geometryVertexIndices.Count];
			int triangleCount = m_geometryVertexIndices.Count / 3;
			for (int triangleIndex = 0; triangleIndex < triangleCount; triangleIndex++)
			{
				indices[3 * triangleIndex + 0] = indexRenumbering[m_geometryVertexIndices[3 * triangleIndex + 0]];
				indices[3 * triangleIndex + 1] = indexRenumbering[m_geometryVertexIndices[3 * triangleIndex + 1]];
				indices[3 * triangleIndex + 2] = indexRenumbering[m_geometryVertexIndices[3 * triangleIndex + 2]];
			}

			PrimitiveState primitiveState = m_primitiveStates[m_currentPrimitiveStateIndex];
			VertexState vertexState = m_vertexStates[primitiveState.VertexStateIndex];
			var gameObject = m_gameObjects[vertexState.MatrixIndex];

			var mesh = new Mesh 
			{
				name		= gameObject.name,
				vertices	= positions, 
				normals		= normals, 
				triangles	= indices,
				uv			= uv1, 
//				uv2			= uv2
			};

			if (gameObject.GetComponent<MeshFilter>() != null)
			{
				gameObject = new GameObject(gameObject.name);
				AddChildToGameObject(vertexState.MatrixIndex, gameObject.transform);
			}

			var meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;

			var meshRenderer = gameObject.AddComponent<MeshRenderer>();
			Debug.Assert(primitiveState.TextureIndexCount == 1);
			meshRenderer.sharedMaterial = m_materials[primitiveState.TextureIndices[0]];
		}
		return false;
	}



	private bool LoadVertexIndicesChunk()
	{
		int indexCount = m_chunkReader.ReadInt();
		for (int k = 0 ; k < indexCount ; k++)
		{
			m_geometryVertexIndices.Add(m_chunkReader.ReadInt());
		}

		return true;
	}



	private bool LoadImagesChunk()
	{
		int imageCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < imageCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadImageChunk()
	{
		string imageFileName = m_chunkReader.ReadString();
		string imageName = imageFileName.RemoveExtension();

		var material = new Material(Shader.Find("Diffuse"))
       	{
       		name		= imageName,
       		mainTexture = (Texture2D) Resources.Load(m_textureAssetFolder + "/" + imageName)
       	};
		m_materials.Add(material);

		return true;
	}



	private bool LoadTexturesChunk()
	{
		int textureCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < textureCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadTextureChunk()
	{
		m_chunkReader.ReadInt();		// Image index
		m_chunkReader.ReadInt();		// Filter mode
		m_chunkReader.ReadInt();		// Mipmap LOD bias
		m_chunkReader.ReadColour();		// Border colour

//		material.AlphaTestFunction = CompareFunction.Greater;
//		material.AlphaTestReference = 0.1f;
//		material.AlphaBlendArgumentSource = Blend.SourceAlpha;
//		material.AlphaBlendArgumentDestination = Blend.InverseSourceAlpha;
		return true;
	}



	private bool LoadShaderNamesChunk()
	{
		int shaderCount = m_chunkReader.ReadInt();
		for (int k = 0 ; k < shaderCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadVertexStatesChunk()
	{
		int vertexStateCount = m_chunkReader.ReadInt();
		for (int k = 0 ; k < vertexStateCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadVertexStateChunk()
	{
		var vertexState = new VertexState 
		{
			Flags		= m_chunkReader.ReadInt(), 
			MatrixIndex = m_chunkReader.ReadInt()
		};

		m_chunkReader.ReadInt();		// Skip LightMatIdx
		m_chunkReader.ReadInt();		// Skip LightCfgIdx
		m_chunkReader.ReadInt();		// Skip LightFlags

/*
		// There is an optional 'matrix2' element that must be skipped.
		// This means that we always must read the end-of-chunk marker and
		// LoadChunk should not read the marker
		string element = m_chunkReader.ReadWord();
		if (element != ")")
			m_chunkReader.ReadWord();
*/

		m_vertexStates.Add(vertexState);

		// Tell loadchunk not to read the end of the chunk
		return true;
	}



	private bool LoadPrimitiveStatesChunk()
	{
		int primitiveStateCount = m_chunkReader.ReadInt();
		for (int k = 0 ; k < primitiveStateCount ; k++)
		{
			LoadChunk();
		}

		return true;
	}



	private bool LoadPrimitiveStateChunk()
	{
		var primitiveState = new PrimitiveState();
		m_currentPrimitiveStateIndex = m_primitiveStates.Count;
		m_primitiveStates.Add(primitiveState);

		primitiveState.Flags = m_chunkReader.ReadInt();
		primitiveState.ShaderIndex = m_chunkReader.ReadInt();
//			Console.WriteLine("ShaderIndex = " + primitiveState.ShaderIndex);

		LoadChunk();

		primitiveState.Zbias = m_chunkReader.ReadInt();
		primitiveState.VertexStateIndex = m_chunkReader.ReadInt();
		primitiveState.AlphaTestMode = m_chunkReader.ReadInt();
		primitiveState.LightConfigurationIndex = m_chunkReader.ReadInt();
		primitiveState.ZBufferMode = m_chunkReader.ReadInt();

		// Tell loadchunk not to read the end of the chunk
		return true;
	}



	private bool LoadPrimitiveStateIndexChunk()
	{
		m_currentPrimitiveStateIndex = m_chunkReader.ReadInt();

		return true;
	}



	private bool LoadTextureIndicesChunk()
	{
		PrimitiveState primitiveState = m_primitiveStates[m_currentPrimitiveStateIndex];

		primitiveState.TextureIndexCount = m_chunkReader.ReadInt();
		primitiveState.TextureIndices = new int[primitiveState.TextureIndexCount];
		for (int k = 0 ; k < primitiveState.TextureIndexCount ; k++)
		{
			primitiveState.TextureIndices[k] = m_chunkReader.ReadInt();
		}
		return true;
	}



	private bool LoadDistanceLevelHeaderChunk()
	{
		LoadChildChunks();
		return false;
	}



	private bool LoadHierarchyChunk()
	{
		int indexCount = m_chunkReader.ReadInt();
		Debug.Assert(indexCount == m_gameObjects.Count);
		for (int k = 0 ; k < indexCount ; k++)
		{
			int parentIndex = m_chunkReader.ReadInt();
			if (parentIndex < 0)
			{
				Debug.Assert(m_topNode == null);
				m_topNode = m_gameObjects[k];
			}
			else
			{
				AddChildToGameObject(parentIndex, m_gameObjects[k].transform);
			}
		}

		return true;
	}



	private bool LoadTCBRotControllerChunk()
	{
		int keyCount = m_chunkReader.ReadInt();

		m_currentAnimationController = new ShapeAnimationController(keyCount, false, true);
		var controllers = m_currentAnimationNode.Controllers != null ? new List<ShapeAnimationController>(m_currentAnimationNode.Controllers) : new List<ShapeAnimationController>();
		controllers.Add(m_currentAnimationController);
		m_currentAnimationNode.Controllers = controllers.ToArray();
		m_keyFrameIndex = 0;
//			Console.WriteLine("Start of TCB controller");
		for (int k = 0 ; k < keyCount ; k++)
		{
			LoadChunk();
		}

		m_currentAnimationController = null;
		return true;
	}



	private bool LoadTCBKeyChunk()
	{
		int keyIndex = m_chunkReader.ReadInt();

		var quaternion = new Quaternion(
			-m_chunkReader.ReadFloat(),
			-m_chunkReader.ReadFloat(),
			-m_chunkReader.ReadFloat(),
			m_chunkReader.ReadFloat());
/*
		Matrix m = Matrix.CreateFromQuaternion(quaternion);
		Vector3 angles = 57.295f * m.GetEulerAngles();
		Console.WriteLine("Key = " + angles);
*/
		// TODO what is this stuff?
		m_chunkReader.ReadFloat();
		m_chunkReader.ReadFloat();
		m_chunkReader.ReadFloat();
		m_chunkReader.ReadFloat();
		m_chunkReader.ReadFloat();

		if (m_currentAnimationController.RotationKeyFrames != null)
		{
			m_currentAnimationController.AbsoluteRotation = true;
			m_currentAnimationController.RotationKeyFrames[m_keyFrameIndex] = m_posZtoPosXRotation * quaternion * m_posZtoPosXRotationInv;
			m_currentAnimationController.KeyIndexes[m_keyFrameIndex] = keyIndex;
			m_keyFrameIndex++;
		}

		return true;
	}


	
	private bool LoadSlerpKeyChunk()
	{
		int keyIndex = m_chunkReader.ReadInt();

		Debug.Assert(false, "Not yet implemented properly"); // TODO this quaternion is probably still wrong. Same as TCB?
		var quaternion = new Quaternion(
			m_chunkReader.ReadFloat(),
			m_chunkReader.ReadFloat(),
			m_chunkReader.ReadFloat(),
			m_chunkReader.ReadFloat());
/*
		Matrix m = Matrix.CreateFromQuaternion(quaternion);
		Vector3 angles = 57.295f * m.GetEulerAngles();
		Console.WriteLine("Key = " + angles);
*/
		if (m_currentAnimationController.RotationKeyFrames != null)
		{
			m_currentAnimationController.AbsoluteRotation = true;	// TODO should be relative, but animations look right when it's absolute (e.g. rg92.s)
			m_currentAnimationController.RotationKeyFrames[m_keyFrameIndex] = quaternion;
			m_currentAnimationController.KeyIndexes[m_keyFrameIndex] = keyIndex;
			m_keyFrameIndex++;
		}

		return true;
	}


	
	private bool LoadLinearPosControllerChunk()
	{
		int keyCount = m_chunkReader.ReadInt();

		m_currentAnimationController = new ShapeAnimationController(keyCount, true, false);
		var controllers = m_currentAnimationNode.Controllers != null ? new List<ShapeAnimationController>(m_currentAnimationNode.Controllers) : new List<ShapeAnimationController>();
		controllers.Add(m_currentAnimationController);
		m_currentAnimationNode.Controllers = controllers.ToArray();
		m_keyFrameIndex = 0;
		for (int k = 0 ; k < keyCount ; k++)
		{
			LoadChunk();
		}

		m_currentAnimationController = null;
		return true;
	}


	private bool LoadLinearKeyChunk()
	{
		int keyIndex = m_chunkReader.ReadInt();

		float z = -m_chunkReader.ReadFloat(); 
		float y = m_chunkReader.ReadFloat(); 
		float x = m_chunkReader.ReadFloat();

		var position = new Vector3(x, y, z);

		if (m_currentAnimationController.TranslationKeyFrames != null)
		{
			m_currentAnimationController.TranslationKeyFrames[m_keyFrameIndex] = position;
			m_currentAnimationController.KeyIndexes[m_keyFrameIndex] = keyIndex;
			m_keyFrameIndex++;
		}

		return true;
	}

	
	private bool LoadAnimationControllersChunk()
	{
		int controllerCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < controllerCount ; k++)
		{
			LoadChunk();
		}
		return true;
	}


	private bool LoadAnimationNodeChunk(string nodeName)
	{
		m_currentAnimationNode = m_currentAnimation.AddNode(nodeName);

		LoadChunk();

		m_currentAnimationNode = null;
		return true;
	}


	private bool LoadAnimationNodesChunk()
	{
		int nodeCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < nodeCount ; k++)
		{
			LoadChunk();
		}
		return true;
	}

	
	private bool LoadAnimationChunk()
	{
		m_currentAnimation = new ShapeAnimation(4);
		m_animations.Add(m_currentAnimation);

		m_chunkReader.ReadFloat();
		m_chunkReader.ReadFloat();
		LoadChunk();

		m_currentAnimation = null;
		return true;
	}


	private bool LoadAnimationsChunk()
	{
		int animationCount = m_chunkReader.ReadInt();

		for (int k = 0 ; k < animationCount ; k++)
		{
			LoadChunk();
		}
		return true;
	}


	private bool LoadChunk()
	{
		string chunkName;
		ShapeChunkType chunkType = m_chunkReader.ReadChunkHeader(out chunkName);
		if (chunkType == ShapeChunkType.EndOfChunk)
			return false;

		if (m_chunkReader.EndOfStream)
			return false;

		bool readChunkEnd;
		switch (chunkType)
		{
			case ShapeChunkType.shape:
				readChunkEnd = LoadShapeChunk(); 
				break;

			case ShapeChunkType.points: 
				readChunkEnd = LoadPointsChunk(); 
				break;

			case ShapeChunkType.point: 
				readChunkEnd = LoadPointChunk(); 
				break;

			case ShapeChunkType.uv_points: 
				readChunkEnd = LoadUVPointsChunk(); 
				break;

			case ShapeChunkType.uv_point: 
				readChunkEnd = LoadUVPointChunk(); 
				break;

			case ShapeChunkType.normals: 
				readChunkEnd = LoadNormalsChunk(); 
				break;

			case ShapeChunkType.vector: 
				readChunkEnd = LoadVectorChunk(); 
				break;

			case ShapeChunkType.matrices: 
				readChunkEnd = LoadMatricesChunk(); 
				break;

			case ShapeChunkType.matrix: 
				readChunkEnd = LoadMatrixChunk(chunkName); 
				break;

			case ShapeChunkType.lod_controls: 
				readChunkEnd = LoadLODControlsChunk(); 
				break;

			case ShapeChunkType.lod_control: 
				readChunkEnd = LoadLODControlChunk(); 
				break;

			case ShapeChunkType.distance_levels: 
				readChunkEnd = LoadDistanceLevelsChunk(); 
				break;

			case ShapeChunkType.distance_level:
				readChunkEnd = LoadDistanceLevelChunk(); 
				break;

			case ShapeChunkType.distance_level_header: 
				readChunkEnd = LoadDistanceLevelHeaderChunk(); 
				break;

			case ShapeChunkType.hierarchy:
				if (m_currentDistanceLevelIndex == 0)
				{
					readChunkEnd = LoadHierarchyChunk();
					break;
				}
				goto default;

			case ShapeChunkType.sub_objects: 
				readChunkEnd = LoadSubObjectsChunk(); 
				break;

			case ShapeChunkType.sub_object: 
				readChunkEnd = LoadSubObjectChunk();
				break;

			case ShapeChunkType.vertices: 
				readChunkEnd = LoadVerticesChunk();
				break;

			case ShapeChunkType.vertex: 
				readChunkEnd = LoadVertexChunk(); 
				break;

			case ShapeChunkType.vertex_uvs: 
				readChunkEnd = LoadVertexUVsChunk(); 
				break;

			case ShapeChunkType.primitives: 
				readChunkEnd = LoadPrimitivesChunk(); 
				break;

			case ShapeChunkType.indexed_trilist: 
				readChunkEnd = LoadIndexedTrilistChunk(); 
				break;

			case ShapeChunkType.vertex_idxs: 
				readChunkEnd = LoadVertexIndicesChunk(); 
				break;

			case ShapeChunkType.images: 
				readChunkEnd = LoadImagesChunk(); 
				break;

			case ShapeChunkType.image: 
				readChunkEnd = LoadImageChunk(); 
				break;

			case ShapeChunkType.textures:
				readChunkEnd = LoadTexturesChunk();
				break;

			case ShapeChunkType.texture:
				readChunkEnd = LoadTextureChunk();
				break;

			case ShapeChunkType.shader_names: 
				readChunkEnd = LoadShaderNamesChunk(); 
				break;

			case ShapeChunkType.vtx_states: 
				readChunkEnd = LoadVertexStatesChunk(); 
				break;

			case ShapeChunkType.vtx_state: 
				readChunkEnd = LoadVertexStateChunk(); 
				break;

			case ShapeChunkType.prim_states: 
				readChunkEnd = LoadPrimitiveStatesChunk(); 
				break;

			case ShapeChunkType.prim_state: 
				readChunkEnd = LoadPrimitiveStateChunk(); 
				break;

			case ShapeChunkType.prim_state_idx: 
				readChunkEnd = LoadPrimitiveStateIndexChunk(); 
				break;

			case ShapeChunkType.tex_idxs: 
				readChunkEnd = LoadTextureIndicesChunk(); 
				break;

			case ShapeChunkType.animations:
				readChunkEnd = LoadAnimationsChunk();
				break;

			case ShapeChunkType.animation:
				readChunkEnd = LoadAnimationChunk();
				break;

			case ShapeChunkType.anim_nodes:
				readChunkEnd = LoadAnimationNodesChunk();
				break;

			case ShapeChunkType.anim_node:
				readChunkEnd = LoadAnimationNodeChunk(chunkName);
				break;

			case ShapeChunkType.controllers:
				readChunkEnd = LoadAnimationControllersChunk();
				break;

			case ShapeChunkType.tcb_rot:
				readChunkEnd = LoadTCBRotControllerChunk();
				break;

			case ShapeChunkType.tcb_key:
				readChunkEnd = LoadTCBKeyChunk();
				break;

			case ShapeChunkType.slerp_rot:
				readChunkEnd = LoadSlerpKeyChunk();
				break;

			case ShapeChunkType.linear_pos:
				readChunkEnd = LoadLinearPosControllerChunk();
				break;

			case ShapeChunkType.linear_key:
				readChunkEnd = LoadLinearKeyChunk();
				break;
			default:
				m_chunkReader.SkipRemainingChunk();
				readChunkEnd = false;
				break;
		}

		m_chunkReader.EndChunk(readChunkEnd);

		return true ;
	}



	public GameObject Load(string fileName, string textureAssetFolder, out List<ShapeAnimation> animations)
	{
		m_textureAssetFolder = textureAssetFolder;

		animations = null;
		Stream stream = File.OpenRead(fileName);
        var buffer = new byte[256];

		// Check if file header is in unicode or ASCII format
		bool unicodeHeader = false;
		stream.Read(buffer, 0, 2);
		if (buffer[0] == 0xFF && buffer[1] == 0xFE)
		{
			unicodeHeader = true;
		}
		else
		{
			// No unicode indicator, so undo read because we
			// just read part of the header!
			stream.Seek(0, SeekOrigin.Begin);
		}
		
		// Read 16 character shape file header. Only care about first 8 characters
        string header;
        if (unicodeHeader) 
        {
            stream.Read(buffer, 0, 16 * 2);
            header = Encoding.Unicode.GetString(buffer, 0, 8 * 2);
			if (header[0] == '\r' && header[1] == '\n')
			{
				// At least one file (us1rd2l1000r10d.s) starts with a newline, so correct the header
				header = header.Substring(2);
	            header += Encoding.Unicode.GetString(buffer, 8 * 2, 2 * 2);
			}
        }
        else
        {
            stream.Read(buffer, 0, 16);
            header = Encoding.ASCII.GetString(buffer, 0, 8);
        }

		// Check if shape file is compressed
        if (header.StartsWith("SIMISA@F"))
        {
			// Rest of file is compressed: 'change' our stream to a decompressing stream
            stream = new InflaterInputStream(stream);
        }
        else if (header.StartsWith("SIMISA@@"))
        {
			// Rest of file is uncompressed
		}
        else
        {
            throw new Exception(String.Format("Unrecognized header '{0}' in {1}", header, fileName));
        }

		// Read 16 character subheader. Only care about first 8 characters
        string subHeader;
        if (unicodeHeader)
        {
            stream.Read(buffer, 0, 16 * 2);
            subHeader = Encoding.Unicode.GetString(buffer, 0, 8 * 2);
        }
        else
        {
            stream.Read(buffer, 0, 16);
            subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
        }

		// Check whether the (uncompressed) contents of the file is binary or text.
		// Note that the header and subheader can be ASCII or unicode, but the rest 
		// of a text (type 't') file is always unicode text
        if (subHeader[7] == 's' || subHeader[7] == 't')
        {
			m_chunkReader = new UniCodeShapeChunkReader(stream);
        }
        else if (subHeader[7] == 'b')
        {
            if(subHeader[5] == 'w')
			{
               throw new Exception(String.Format("Unsupported token offset in shape file {0}", fileName));
			}
			m_chunkReader = new BinaryShapeChunkReader(stream);
        }
        else
        {
           throw new Exception(String.Format("Unsupported subheader in shape file {0}", fileName));
        }

		LoadChunk();

		m_chunkReader.Close();

		foreach (ShapeAnimation animation in m_animations)
		{
			animation.SetTransforms(m_topNode.transform);
		}

		animations = m_animations;

		m_topNode.name = Path.GetFileName(fileName).RemoveExtension();

		return m_topNode;
	}



	private ShapeChunkReader m_chunkReader;
	private GameObject m_topNode;

	private readonly List<GameObject>		m_gameObjects					= new List<GameObject>();
	private readonly List<Vector3>			m_points						= new List<Vector3>();
	private readonly List<Vector2>			m_uvPoints						= new List<Vector2>();
	private readonly List<Vector3>			m_normals						= new List<Vector3>();
	private readonly List<VertexState>		m_vertexStates					= new List<VertexState>();
	private readonly List<PrimitiveState>	m_primitiveStates				= new List<PrimitiveState>();
	private readonly List<int>				m_geometryVertexIndices 		= new List<int>();
	private readonly Quaternion				m_posZtoPosXRotationInv			= Quaternion.Euler(0, -90, 0);
	private readonly Quaternion				m_posZtoPosXRotation			= Quaternion.Euler(0, 90, 0);
	private int 							m_currentDistanceLevelIndex;
	private int 							m_currentPrimitiveStateIndex;
	private readonly List<Vector3> 			m_geometryVertexPositions		= new List<Vector3>();
	private readonly List<Vector3> 			m_geometryVertexNormals			= new List<Vector3>();
	private readonly List<List<Vector2>>	m_geometryVertexUVs				= new List<List<Vector2> >();
	private readonly List<ShapeAnimation>	m_animations					= new List<ShapeAnimation>();
	private ShapeAnimation					m_currentAnimation;
	private ShapeAnimationNode				m_currentAnimationNode;
	private ShapeAnimationController		m_currentAnimationController;
	private int								m_keyFrameIndex;
	private readonly List<Material>			m_materials						= new List<Material>();
	private string							m_textureAssetFolder;

	

}
