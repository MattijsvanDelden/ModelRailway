using System.Diagnostics;
using System.IO;
using System.Text;

internal class UniCodeShapeChunkReader : ShapeChunkReader
{
	private string ReadWord()
	{
		byte[] buffer = new byte[2];
		StringBuilder builder = new StringBuilder();
		bool readingLeadingWhiteSpace = true;
		for(;;)
		{
			if (m_stream.Read(buffer, 0, 2) < 2)
			{
				break;
			}
			char[] chars = Encoding.Unicode.GetChars(buffer);
			if (char.IsWhiteSpace(chars[0]))
			{
				if (!readingLeadingWhiteSpace)
					break;
			}
			else
			{
				readingLeadingWhiteSpace = false;
				builder.Append(chars[0]);
			}
		}
		return builder.ToString();
	}

	public UniCodeShapeChunkReader(Stream stream) : base(stream)
	{
	}

	public override int ReadInt()
	{
		return int.Parse(ReadWord());
	}

	public override float ReadFloat()
	{
		return float.Parse(ReadWord());
	}

	public override ShapeChunkType ReadChunkHeader(out string chunkName)
	{
		string chunkTypeString = ReadWord();
		if (chunkTypeString == "" || chunkTypeString == ")")
		{
			chunkName = "";
			return ShapeChunkType.EndOfChunk;
		}

		// Get optional name and skip '('
		chunkName = ReadWord();
		if (chunkName != "(")
		{
			string tmp = ReadWord();
			Debug.Assert(tmp == "(");
		}
		else
			chunkName = "";

		switch (chunkTypeString)
		{
			case "shape" : return ShapeChunkType.shape;
			case "points": return ShapeChunkType.points;
			case "point":  return ShapeChunkType.point; 
			case "uv_points":  return ShapeChunkType.uv_points; 
			case "uv_point":  return ShapeChunkType.uv_point; 
			case "normals":  return ShapeChunkType.normals; 
			case "vector":  return ShapeChunkType.vector; 
			case "matrices":  return ShapeChunkType.matrices; 
			case "matrix":  return ShapeChunkType.matrix; 
			case "lod_controls":  return ShapeChunkType.lod_controls; 
			case "lod_control":  return ShapeChunkType.lod_control; 
			case "distance_levels":  return ShapeChunkType.distance_levels; 
			case "distance_level":  return ShapeChunkType.distance_level;
			case "distance_level_header":  return ShapeChunkType.distance_level_header; 
			case "hierarchy":  return ShapeChunkType.hierarchy;
			case "sub_objects":  return ShapeChunkType.sub_objects; 
			case "sub_object":  return ShapeChunkType.sub_object; 
			case "vertices":  return ShapeChunkType.vertices; 
			case "vertex":  return ShapeChunkType.vertex; 
			case "vertex_uvs":  return ShapeChunkType.vertex_uvs; 
			case "primitives":  return ShapeChunkType.primitives; 
			case "indexed_trilist":  return ShapeChunkType.indexed_trilist; 
			case "vertex_idxs":  return ShapeChunkType.vertex_idxs; 
			case "images":  return ShapeChunkType.images; 
			case "image":  return ShapeChunkType.image; 
			case "shader_names":  return ShapeChunkType.shader_names; 
			case "vtx_states":  return ShapeChunkType.vtx_states; 
			case "vtx_state":  return ShapeChunkType.vtx_state; 
			case "prim_states":  return ShapeChunkType.prim_states; 
			case "prim_state":  return ShapeChunkType.prim_state; 
			case "prim_state_idx":  return ShapeChunkType.prim_state_idx; 
			case "tex_idxs" : return ShapeChunkType.tex_idxs;
			case "textures" : return ShapeChunkType.textures;
			case "texture" : return ShapeChunkType.texture;
			case "animations": return ShapeChunkType.animations;
			case "animation": return ShapeChunkType.animation;
			case "anim_nodes": return ShapeChunkType.anim_nodes;
			case "anim_node": return ShapeChunkType.anim_node;
			case "controllers": return ShapeChunkType.controllers;
			case "tcb_rot": return ShapeChunkType.tcb_rot;
			case "tcb_key": return ShapeChunkType.tcb_key;
			case "linear_pos": return ShapeChunkType.linear_pos;
			case "linear_key": return ShapeChunkType.linear_key;
			default:
				return ShapeChunkType.Unknown;
		}
	}

	public override void SkipRemainingChunk()
	{
		// Skip unknown chunk contents
		int subchunkDepth = 1 ;
		for (;;)
		{
			string word = ReadWord();
			if (word == "")
			{
				break;
			}
			if (word == "(")
			{
				subchunkDepth++ ;
			}
			else if (word == ")")
			{
				subchunkDepth-- ;
				if (subchunkDepth == 0)
					break ;
			}
		}
	}

	public override void EndChunk(bool readChunkEnd)
	{
		if (readChunkEnd)
		{
			string chunkEnd = ReadWord();
			Debug.Assert(chunkEnd == ")");
		}
	}

	public override string ReadString()
	{
		return ReadWord();
	}

	public override int ReadColour()
	{
		ReadWord();
		return 0;
	}
}
