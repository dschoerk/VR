/* =====================================================================================
 * ARTiFICe - Augmented Reality Framework for Distributed Collaboration
 * ====================================================================================
 * Copyright (c) 2010-2012 
 * 
 * Annette Mossel, Christian Schönauer, Georg Gerstweiler, Hannes Kaufmann
 * mossel | schoenauer | gerstweiler | kaufmann @ims.tuwien.ac.at
 * Interactive Media Systems Group, Vienna University of Technology, Austria
 * www.ims.tuwien.ac.at
 * 
 * ====================================================================================
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *  
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *  
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * =====================================================================================
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Class to show a GUI to select a IT by ARToolkitMarker during runtime. 
/// </summary>
public class ITSelectionGUI : MonoBehaviour
{
	public GUIStyle vrue11Style; 
	protected string _vh; 
	protected string _triggerMarker;

	/* ------------------ VRUE Tasks START --------------------------
	* Place required member variables here
	----------------------------------------------------------------- */
	
	private IList<ObjectSelectionBase> InteractionMethods = null;
	
	// ------------------ VRUE Tasks END ----------------------------
	
	/// <summary>
	/// Set StartUp Data. Method is called by OnEnable Unity Callback
	/// Must be overwritten in deriving class
	/// </summary>
	protected virtual void StartUpData()
	{	
		// name of interaction object in Unity Hierarchy
		_vh = "VirtualHand";
		
		// name of trigger marker
		_triggerMarker = "Marker2";
	}
	
	/// <summary>
	/// </summary>
	void OnEnable()
	{	
		// set init data
		StartUpData();
		Debug.Log("IT Selection GUI enabled");
		
		/* ------------------ VRUE Tasks START --------------------------
		* find ITs (components) attached to interaction game object
		* if none is attached, manually attach 3 ITs to interaction game object
		* initially det default IT
		----------------------------------------------------------------- */
		
		GameObject virtualHand = GameObject.Find(_vh);
		
		if (virtualHand != null) 
		{
			this.InteractionMethods = new List<ObjectSelectionBase> (virtualHand.GetComponents<ObjectSelectionBase> ());
			
			if (this.VirtualHandInteraction == null) 
			{
				// attatch VirtualHandInteraction manually to virtualHand
				virtualHand.AddComponent ("VirtualHandInteraction");
			}
			if (this.GoGoInteraction == null) 
			{
				// attatch GoGoInteraction manually to virtualHand
				virtualHand.AddComponent ("GoGoInteraction");
			}
			if (this.HomerInteraction == null) 
			{
				// attatch HomerInteraction manually to virtualHand
				virtualHand.AddComponent ("HomerInteraction");
			}
			
			if (this.InteractionMethods.Count() < 3)
				this.InteractionMethods = new List<ObjectSelectionBase> (virtualHand.GetComponents<ObjectSelectionBase> ());
		}
		
		this.ActivateInteractionMethod (this.InteractionMethods, "VirtualHandInteraction");
		this.SelectedInteractionMethod = this.ActiveInteractionMethod;
		
		// ------------------ VRUE Tasks END ----------------------------
	}
	
	private ObjectSelectionBase VirtualHandInteraction
	{
		get
		{
			return (from interactionMethod in this.InteractionMethods where interactionMethod is VirtualHandInteraction select interactionMethod).FirstOrDefault();
		}
	}
	
	private ObjectSelectionBase GoGoInteraction
	{
		get
		{
			return (from interactionMethod in this.InteractionMethods where interactionMethod is GoGoInteraction select interactionMethod).FirstOrDefault();
		}
	}
	
	private ObjectSelectionBase HomerInteraction
	{
		get
		{
			return (from interactionMethod in this.InteractionMethods where interactionMethod is HomerInteraction select interactionMethod).FirstOrDefault();
		}
	}
	
	private void ActivateInteractionMethod(IList<ObjectSelectionBase> interactionMethods,string interactionMethodStr)
	{
		foreach(ObjectSelectionBase interactionMethod in interactionMethods)
		{
			if (interactionMethod.GetType().ToString().Contains(interactionMethodStr))
				interactionMethod.enabled = true;
			else
				interactionMethod.enabled = false;
		}
	}
	
	private ObjectSelectionBase SelectedInteractionMethod = null;
	
	public ObjectSelectionBase ActiveInteractionMethod
	{
		get
		{
			return (from interactionMethod in this.InteractionMethods where interactionMethod.enabled == true select interactionMethod).FirstOrDefault();
		}
	}
	
	private bool IsTriggerMarkerVisible
	{
		get
		{
			return this.MultiMarkerSwitch.IsFaceFront(this._triggerMarker);
		}
	}
	
	private MultiMarkerSwitch _MultiMarkerSwitch = null;
	private MultiMarkerSwitch MultiMarkerSwitch
	{
		get
		{
			if (this._MultiMarkerSwitch == null)
				this._MultiMarkerSwitch = this.GetComponent<MultiMarkerSwitch>();
			
			return this._MultiMarkerSwitch;
		}
	}
	
	private bool _MenuIsActive = false;
	private DateTime MenuDeactivationTime = DateTime.MaxValue;
	private TimeSpan MenuDeactivationTimeout = new TimeSpan(0,0,4);
	private bool MenuIsActive
	{
		get
		{
			if (DateTime.Now > this.MenuDeactivationTime)
				this._MenuIsActive = false;
			
			return this._MenuIsActive;
		}
		set
		{
			if (value)
				this.MenuDeactivationTime = DateTime.Now + this.MenuDeactivationTimeout;
			
			this._MenuIsActive = value;
		}
	}
	
	/// <summary>
	/// Unity Callback
	/// OnGUI is called every frame for rendering and handling GUI events.
	/// </summary>
	void OnGUI () {
		
		/* ------------------ VRUE Tasks START --------------------------
		* check if ITs are available
		* if trigger marker is visible and no objects are currently selected by interaction game object show GUI
		* depending on visible marker switch through availabe ITs
		* implement user confirmation and set selected IT only if user has confirmed it
		* disable the GUI if virtual hand has selected objects and if user has confirmend an IT
		----------------------------------------------------------------- */
		
		// if trigger marker is visible and no objects are currently selected 
		if (IsTriggerMarkerVisible && !this.InteractionMethods.All (interactionMethod => interactionMethod.getSelectionState ())) 
			this.MenuIsActive = true;
		
		if (this.InteractionMethods.Any (interactionMethod => interactionMethod.getSelectionState ()))
			this.MenuIsActive = false;
		
		this.RotationalMenu();
		
		// ------------------ VRUE Tasks END ----------------------------
	}
	
	private readonly Dictionary<string,string> InteractionTypeMarkerNameMapping = new Dictionary<string, string>{
		{"VirtualHandInteraction", "Marker5"},
		{"GoGoInteraction", "Marker4"},
		{"HomerInteraction", "Marker0"},
	};
	
	private void RotationalMenu()
	{
		if (!this.MenuIsActive)
			return;
		
		string faceFrontName = this.MultiMarkerSwitch.GetFaceFront();
		
		// get index of active interaction
		int startIndex = this.InteractionMethods.IndexOf (this.SelectedInteractionMethod);
		
		// loop from startIndex till startIndex-1 in a cycle
		for (int index=0; index<this.InteractionMethods.Count(); index++) 
		{
			int interactionIndex = (index+startIndex) % this.InteractionMethods.Count();
			
			// Make the second button.
			string currentInteractionMethodTypeStr = this.InteractionMethods[interactionIndex].GetType().ToString();
			string currentInteractionMarkerName = InteractionTypeMarkerNameMapping[currentInteractionMethodTypeStr];

			float left = Screen.width / 2 - 100;
			float top = Screen.height / 2 + (index-this.InteractionMethods.Count())*30;
			float width = 200;
			float height = 30;
			
			//if ((GUI.Button (new Rect (300, 150+index*30, 200, 30), currentInteractionMethodTypeStr) || 
			if ((GUI.Button (new Rect (left,top, width, height), currentInteractionMethodTypeStr) || 
			     faceFrontName == currentInteractionMarkerName)) 
			{
				this.SelectedInteractionMethod = this.InteractionMethods[interactionIndex];
				this.MenuIsActive = true;
				if (Input.GetButton("Fire2"))
				{
					this.ActivateInteractionMethod(this.InteractionMethods, currentInteractionMethodTypeStr);
					this.MenuIsActive = false;
				}
			}
			
		}
		
	}
	
	
	/* ------------------ VRUE Tasks START -------------------
	----------------------------------------------------------------- */
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	// ------------------ VRUE Tasks END ----------------------------
	
}
