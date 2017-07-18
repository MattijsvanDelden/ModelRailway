using System;
using System.IO;
using UnityEngine;
using Debug=System.Diagnostics.Debug;



namespace ModelRailway
{

public class WagonEngineDescriptionLoader
{
	public bool Load(string fileName, out WagonDescription wagonDescription, out EngineDescription engineDescription)
	{
		wagonDescription = null;
		engineDescription = null;
		if (!File.Exists(fileName))
		{
			return false;
		}
		m_reader = new StreamReader(fileName);
		m_reader.ReadLine();

		m_engineDescription = null;
		m_wagonDescription = null;
		m_inEngineDescription = false;
		m_inWagonDescription = false;
		
		ReadSubChunks(0);
		
		m_reader.Close();

		engineDescription = m_engineDescription;
		wagonDescription = m_wagonDescription;

		return true;
	}



	private void ReadSubChunks(int level)
	{
		while (!m_reader.EndOfStream)
		{
			if (!ReadChunk(level))
			{
				break;
			}
		}
	}



	private bool ReadChunk(int level)
	{
		string chunkName = m_reader.ReadWord();
		if (m_reader.EndOfStream)
		{
			return false;
		}
		if (chunkName == ")")
		{
			return false;
		}

		string separator;
		if (chunkName == "(")
		{
			chunkName = "<Unnamed>";
		}
		else
		{
			separator = m_reader.ReadWord();
			Debug.Assert(separator == "(");
		}
		string descriptionName = null;
		if (chunkName == "Wagon" || chunkName == "Engine")
		{
			descriptionName = m_reader.ReadWord();
		}

		bool skipRestOfChunk = false;
		bool skipEndbracket = false;
		switch (chunkName)
		{
			case "Wagon":
				if(level == 0)
				{
					m_wagonDescription = new WagonDescription { Name = descriptionName };
					m_inEngineDescription = false;
					m_inWagonDescription = true;
					ReadSubChunks(level + 1);
					skipEndbracket = true;
				}
				break;

			case "Engine":
				Debug.Assert(level == 0);
				{
					m_engineDescription = new EngineDescription { Name = descriptionName };
					m_inEngineDescription = true;
					m_inWagonDescription = false;
				}
				ReadSubChunks(level + 1);
				skipEndbracket = true;
				break;

			case "Type":
				if (m_inWagonDescription)
				{
					m_wagonDescription.Type = (WagonType) Enum.Parse(typeof (WagonType), m_reader.ReadWord());
				}
				else
				{
					skipRestOfChunk = true;
				}
				break;

			case "WagonShape":
				m_wagonDescription.ShapeFileName = m_reader.ReadWord();
				break;

			case "FreightAnim":
				m_wagonDescription.FreightAnimShapeFileName = m_reader.ReadWord();
				skipRestOfChunk = true;
				break;

			case "Size":
				float z = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				float y = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				float x = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				m_wagonDescription.Size = new Vector3(x, y, z);
				break;

			case "WheelRadius":
				string radiusString = m_reader.ReadWord();
				float radius = MSTSHelpers.ParseMeasurement(radiusString);
				if (m_inEngineDescription)
				{
					m_engineDescription.WheelRadius = radius;	
				}
				else if (m_inWagonDescription)
				{
					m_wagonDescription.WheelRadius = radius;
				}
				break;


			// Non-MSTS stuff (added by me!)

			case "FrontCouplerOffset":
				m_wagonDescription.FrontCouplerOffset = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				break;

			case "RearCouplerOffset":
				m_wagonDescription.RearCouplerOffset = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				break;

			case "WheelRotationAngle":
				m_wagonDescription.InitialWheelRotationAngle = MSTSHelpers.ParseMeasurement(m_reader.ReadWord());
				break;

			default:
				skipRestOfChunk = true;
				break;
		}

		if (skipRestOfChunk)
		{
			MSTSHelpers.SkipChunkContents(m_reader);
		}

		if (!skipEndbracket)
		{
			separator = m_reader.ReadWord();
			Debug.Assert(separator == ")");
		}

		return true;
	}

	private StreamReader m_reader;
	private bool m_inWagonDescription;
	private bool m_inEngineDescription;
	private EngineDescription m_engineDescription;
	private WagonDescription m_wagonDescription;
}

}
