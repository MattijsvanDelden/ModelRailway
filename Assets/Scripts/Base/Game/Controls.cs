using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace Game
{

public class Controls
{
	public delegate void FireCallback();
	public delegate void HeldCallback();



	public Controls(TextAsset controlSettings)
	{
		var reader = new StringReader(controlSettings.text);
		for (;;)
		{
			string line = reader.ReadLine();
			if (line == null)
			{
				break;
			}
			string[] words = line.Split(',');
			string name = words[0].Trim();
			string[] controls = words[1].Split('+');

			var controlAssignment = new ControlAssignment()
			{
				Name = name
			};
			foreach (string control in controls)
			{ 
				string word = control.ToLower().Trim();
				switch (word)
				{
					case "ctrl":
					case "control":
						controlAssignment.WantsControlPressed = true;
						break;

					case "shft":
					case "shift":	
						controlAssignment.WantsShiftPressed = true;
						break;

					case "alt":
						controlAssignment.WantsAltPressed = true;
						break;

					default:
						try
						{
							controlAssignment.KeyCode = (KeyCode) Enum.Parse(typeof(KeyCode), word.ToUpper());
						}
						catch
						{
							Debug.LogWarning("Unknown control '" + word + "' ignored");
						}
						break;
				}
			}
			m_controlAssignments.Add(controlAssignment);
		}
	}



	public bool SetFireCallback(string controlName, FireCallback callback)
	{
		foreach(ControlAssignment control in m_controlAssignments)
		{
			if (control.Name == controlName)
			{
				control.FireCallback = callback;
				return true;
			}
		}
		return false;
	}



	public bool SetHeldCallback(string controlName, HeldCallback callback)
	{
		foreach (ControlAssignment control in m_controlAssignments)
		{
			if (control.Name == controlName)
			{
				control.HeldCallback = callback;
				return true;
			}
		}
		return false;
	}



	public void Update()
	{
		foreach (ControlAssignment control in m_controlAssignments)
		{
			if (control.WantedMetasPressed)
			{
				if (control.FireCallback != null && Input.GetKeyDown(control.KeyCode))
				{
					control.FireCallback();
				}
				if (control.HeldCallback != null && Input.GetKey(control.KeyCode))
				{
					control.HeldCallback();
				}
			}
		}
	}



	//- PRIVATE ------------------------------------------------------------------------



	private class ControlAssignment
	{
		public string		Name;
		public FireCallback	FireCallback;
		public HeldCallback HeldCallback;
		public KeyCode		KeyCode;
		public bool			WantsControlPressed;
		public bool			WantsShiftPressed;
		public bool			WantsAltPressed;



		public bool WantedMetasPressed
		{
			get 
			{
				bool pressed = true;
				if (WantsControlPressed && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
				{
					pressed = false;
				}
				if (WantsShiftPressed && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
				{
					pressed = false;
				}
				if (WantsAltPressed && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
				{
					pressed = false;
				}
				return pressed;
			}
		}
	}



	private readonly List<ControlAssignment>	m_controlAssignments = new List<ControlAssignment>();
}

}
