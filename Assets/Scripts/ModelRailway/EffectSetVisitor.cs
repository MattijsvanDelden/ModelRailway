// Model railway in C# by Mattijs

/*
using System.Globalization;



namespace ModelRailway
{

public class EffectSetVisitor : NodeVisitor
{
	private readonly EffectManager	m_effectManager;
	private readonly TextureManager m_textureManager;
	private readonly MyEffect[]		m_effects;



	public EffectSetVisitor(EffectManager effectManager, TextureManager textureManager, MyEffect[] effects) : base(false)
	{
		m_textureManager = textureManager;
		m_effectManager = effectManager;
		m_effects = effects;
	}



	public override bool Visit(Node node)
	{
		if (node is GeometryNode)
		{
			var geometryNode = node as GeometryNode;

			if (geometryNode.Name.StartsWith("meta_", true, CultureInfo.CurrentCulture))
			{
				geometryNode.Enabled = false;
				return true;
			}

			if (geometryNode.GeometrySet.Geometries[0].Tangents == null ||
				geometryNode.GeometrySet.Geometries[0].Binormals == null)
				geometryNode.GeometrySet.Geometries[0].CreateTangentsAndBinormals(1);

			if (geometryNode.MaterialTable != null)
			{
				foreach (Material material in geometryNode.MaterialTable.Materials)
				{
					material.Effect = m_effects[material.Stages.Count];
//						material.Specular = Colour.White;

					MaterialStage shadowMapStage = new MaterialStage
					{
						Texture = m_shadowMap,
						TextureName = "Shadow Map"
					};
					material.Stages.Add(shadowMapStage);
					if (material.Effect == null)
					{
						if (material.EffectName != null)
						{
							material.Effect = m_effectManager.GetEffect(material.EffectName);
						}
						else
						{
							// Assign default effect
							string effectName;
							switch (material.Stages.Count)
							{
								case 0:
									effectName = "PhongLighting_NoTexture";
									break;
								case 1:
									effectName = "PhongLighting_1Texture";
									break;
								default:
									effectName = "PhongLighting_2textures";
									break;
							}
							material.Effect = m_effectManager.GetEffect(effectName);
						}
					}
					foreach (MaterialStage stage in material.Stages)
					{
						if (stage.Texture == null && stage.TextureName != null)
						{
							stage.Texture = m_textureManager.GetTexture(stage.TextureName);
						}
					}
				}
			}
		}
		return true;
	}
}
}
*/