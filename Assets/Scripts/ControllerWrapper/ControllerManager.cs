﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerID {None, One, Two, Three, Four};

public class ControllerManager  {

	public enum ControlType { None, Xbox, PS4, Keyboard };
	public enum OperatingSystem { Win, OSX, Linux };

	public OperatingSystem currentOS;
	public Dictionary<PlayerID, ControllerInputWrapper> playerControls;

	public static ControllerManager instance;

	public const float CUSTOM_DEADZONE = 0.15f;

	public ControllerManager()
	{
		setUpPlatform();
		playerControls = new Dictionary<PlayerID, ControllerInputWrapper>();
		if(instance != this) {
			instance = this;
		}
	}

	public void ClearPlayers()
	{
		playerControls = new Dictionary<PlayerID, ControllerInputWrapper>();
	}

	public int NumPlayers {
		get {
			return playerControls.Count;
		}
	}

	public ControlType PlayerControlType(PlayerID id) {
		if(!playerControls.ContainsKey(id)) return ControlType.None;
		if(playerControls[id].GetType().Equals(typeof(Xbox360ControllerWrapper))
			|| playerControls[id].GetType().Equals(typeof(XboxOneControllerWrapper))) {
			return ControlType.Xbox;
		} else if(playerControls[id].GetType().Equals(typeof(PS4ControllerWrapper))
			|| playerControls[id].GetType().Equals(typeof(PS3ControllerWrapper))) {
			return ControlType.PS4;
		}  else if(playerControls[id].GetType().Equals(typeof(KeyboardWrapper))) {
			return ControlType.Keyboard;
		} else {
			return ControlType.None;
		}
	}

	public void ResetInputs() {
		playerControls = new Dictionary<PlayerID, ControllerInputWrapper>();
	}

	public bool AddPlayer(ControllerInputWrapper.Buttons connectCode) {
		KeyboardWrapper kw = new KeyboardWrapper(-1);
		if(!playerControls.ContainsValue(kw) && kw.GetButton(connectCode)) {
			for(int j = 1; j < 5; j++) {
				if(!playerControls.ContainsKey((PlayerID)j)) {
					playerControls.Add((PlayerID)(j), kw);
					Debug.Log((PlayerID)(j) + ": " + kw + " added");
					return true;
				}
			}
		}
		if(playerControls.Count < 4) {
			string[] controllerNames = Input.GetJoystickNames();
			for (int i = 0; i < controllerNames.Length; i++) {
				ControllerInputWrapper ciw = getControllerType(i);
				if(ciw != null && !playerControls.ContainsValue(ciw) && ciw.GetButton(connectCode)) {
					for(int j = 1; j < 5; j++) {
						if(!playerControls.ContainsKey((PlayerID)j)) {
							playerControls.Add((PlayerID)(j), ciw);
							Debug.Log((PlayerID)(j) + ": " + ciw + " added");
							return true;
						}
					}
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Adds an AI controller to the game.
	/// </summary>
	/// <returns>Whether the AI controller was successfully added.</returns>
	public bool AddAI(ControllerInputWrapper.Buttons connectCode) {
		if (playerControls.Count < 4) {
			string[] controllerNames = Input.GetJoystickNames();
			ControllerInputWrapper[] controllers = new ControllerInputWrapper[controllerNames.Length + 1];
			controllers[0] = new KeyboardWrapper(-1);
			for (int i = 0; i < controllerNames.Length; i++) {
				controllers[i + 1] = getControllerType(i);
			}
			foreach (ControllerInputWrapper ciw in controllers) {
				if (ciw != null && ciw.GetButton(connectCode)) {
					for (int j = 1; j < 5; j++) {
						if (!playerControls.ContainsKey((PlayerID)j)) {
							AIWrapper aiw = new AIWrapper(-2);
							playerControls.Add((PlayerID)(j), aiw);
							Debug.Log((PlayerID)(j) + ": " + aiw + " added");
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public void AllowPlayerRemoval(ControllerInputWrapper.Buttons removalButton) {
		PlayerID playerToRemove = PlayerID.None;
		foreach(KeyValuePair<PlayerID, ControllerInputWrapper> kvp in ControllerManager.instance.playerControls) {
			if(kvp.Value.GetButton(removalButton)) {
				playerToRemove = kvp.Key;
				break;
			}
		}
		if(playerToRemove != PlayerID.None) {
			playerControls.Remove(playerToRemove);
		}
	}

	public ControllerInputWrapper getControllerType(int joyNum)
	{
		string[] controllerNames = Input.GetJoystickNames();
		if (joyNum < 0 || joyNum > controllerNames.Length)
		{
			return null;
		}
		//        joyNum--;
		string name = controllerNames[joyNum];

		if (name.Contains("Wireless"))
		{
			return new PS4ControllerWrapper(joyNum);
		}
		else if (name.Contains("Logitech"))
		{
			return new LogitechControllerWrapper(joyNum);
		}
		else if (name.Contains("360"))
		{
			return new Xbox360ControllerWrapper(joyNum);
		}
		else
		{
			return new XboxOneControllerWrapper(joyNum);
		}


	}

	private void setUpPlatform()
	{
		//Debug.Log ("platform: " + Application.platform);
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXWebPlayer 
			|| Application.platform == RuntimePlatform.OSXEditor)
		{
			currentOS = OperatingSystem.OSX;
		}
		else
		{
			currentOS = OperatingSystem.Win;
		}
	}

	/// <summary>
	/// Checks if the controller with a certain ID is an AI controller.
	/// </summary>
	/// <returns>Whether the controller with the given ID is an AI controller.</returns>
	/// <param name="id">The ID of the controller to check.</param>
	public bool IsAIController(PlayerID id) {
		ControllerInputWrapper controller;
		if (playerControls.TryGetValue(id, out controller)) {
			return controller is AIWrapper;
		}
		return false;
	}

	public float GetAxis(ControllerInputWrapper.Axis axis, PlayerID id, bool isRaw = false)
	{
		if(!playerControls.ContainsKey(id)) return 0;
		if (playerControls[id] == null)
		{
			return 0;
		}
		return playerControls[id].GetAxis(axis, isRaw);
	}

	public float GetTrigger(ControllerInputWrapper.Triggers trigger, PlayerID id, bool isRaw = false)
	{
		if(!playerControls.ContainsKey(id)) return 0;
		if (playerControls[id] == null)
		{
			return 0;
		}
		return playerControls[id].GetTrigger(trigger, isRaw);
	}

	public bool GetButton(ControllerInputWrapper.Buttons button, PlayerID id = PlayerID.One)
	{
		if(!playerControls.ContainsKey(id)) return false;
		if (playerControls[id] == null)
		{
			return false;
		}
		return playerControls[id].GetButton(button);
	}

	public bool GetButtonDown(ControllerInputWrapper.Buttons button, PlayerID id = PlayerID.One) {
		if(!playerControls.ContainsKey(id)) return false;
		if (playerControls[id] == null)
		{
			return false;
		}
		return playerControls[id].GetButtonDown(button);
	}

	public bool GetButtonUp(ControllerInputWrapper.Buttons button, int joyNum)
	{
		//        joyNum--;
		//        if (joyNum < 0 || playerControls[joyNum] == null)
		//        {
		//            return false;
		//        }
		//        return playerControls[joyNum].GetButtonUp(button, joyNum + 1);
		return false;
	}

	public bool GetButtonAll(ControllerInputWrapper.Buttons button, bool isDown)
	{
		//This definitely needs an update........... Like seriously..
		//        int i = 0;
		//        foreach (ControllerInputWrapper cW in playerControls)
		//        {
		//            if (!cW.GetButton(button, i, isDown)){
		//                return false;
		//            }
		//        }
		//        return true;
		return false;
	}

	public override string ToString ()
	{
		return currentOS.ToString(); 
	}
}
