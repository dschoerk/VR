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
using System.Collections;

/// <summary>
/// Class to select and manipulate scene objects with gogo interaction technique (IT). 
/// 
/// GoGo is a 1st person view IT
/// </summary>
public class GoGoInteraction : ObjectSelectionBase
{
	/* ------------------ VRUE Tasks START -------------------
	* 	Implement GoGo interaction technique
	----------------------------------------------------------------- */
	
	private float D = 3;
	private float k = 0.7f;
	
	GameObject tracker = null;
	GameObject virutalCamera = null;
	
	/// <summary>
	/// </summary>
	public void Start()
	{
		tracker = GameObject.Find("TrackerObject");
		virutalCamera = GameObject.Find("VirtualCamera");
		Debug.Log("Start VirtualHand");
	}
	
	private GameObject InteractionOrigin = null;
	private Transform TorsoTransform
	{
		get
		{
			if (this.InteractionOrigin == null)
				this.InteractionOrigin = GameObject.Find("InteractionOrigin");
			
			if (this.InteractionOrigin != null)
				return this.InteractionOrigin.transform;
			
			return null;
		}
	}
	
	private GameObject VirtualHand = null;
	private Transform VirtualHandTransform
	{
		get
		{
			if (this.VirtualHand == null)
				this.VirtualHand = GameObject.Find("VirtualHand(Clone)");
			
			if (this.VirtualHand != null)
				return this.VirtualHand.transform;
			
			return null;
		}
	}
	
	private GameObject PhysicalHand = null;
	private Transform PhysicalHandTransform
	{
		get
		{
			if (this.PhysicalHand == null)
				this.PhysicalHand = GameObject.Find("TrackerObject");
			
			if (this.PhysicalHand != null)
				return this.PhysicalHand.transform;
			
			return null;
		}
	}
	
	/// <summary>
	/// Implementation of concrete IT selection behaviour. 
	/// </summary>
	protected override void UpdateSelect()
	{
		if(tracker)
		{
			// INTERACTION TECHNIQUE THINGS ------------------------------------------------
			if (tracker.transform.parent.GetComponent<TrackBase>().isTracked())
			{
				// show virtual hand -> physical hand is autmatically rendert due to tracking state
				tracker.transform.parent.GetComponent<TrackBase>().setVisability(gameObject, true);
				
				//Update transform of the selector object (virtual hand)
				GogoTransformation(this.TorsoTransform, this.PhysicalHandTransform, D, k, this.VirtualHandTransform);
				
				// Transform (translate and rotate) selected object depending on of virtual hand's transformation
				if (selected)
				{
					this.transformInter(this.transform.position, this.transform.rotation);
				}
			}else 
			{
				// make virtual hand invisible -> physical hand is autmatically rendert due to tracking state
				tracker.transform.parent.GetComponent<TrackBase>().setVisability(gameObject, false);
			}
		}
		else
		{
			Debug.Log("No GameObject with name - TrackerObject - found in scene");
		}
	}
	
	
	protected void GogoTransformation(Transform trans_torso, Transform trans_hand, float D, float k, Transform trans_hand_gogo)
	{
		// calculate parameters between hand and torso
		// especially distance if of interest
		
		// polar coordinates: R, Phi, Theta? Actually not necessary, as only length of direction vector is scaled
		
		Vector3 vecRealHand_delta = trans_hand.position - trans_torso.position;
		
		// scale vector accordingly
		Vector3 vecVirtHand_NonLinDelta = Vector3.zero;
		if (vecRealHand_delta.magnitude >= D) 
			vecVirtHand_NonLinDelta = vecRealHand_delta*k*Mathf.Pow(vecRealHand_delta.magnitude - D,2);
		
		// assemble new transformation for Gogo hand
		trans_hand_gogo.position = trans_hand.position + vecVirtHand_NonLinDelta;
		trans_hand_gogo.rotation = trans_hand.rotation;
		
	}
	
	// ------------------ VRUE Tasks END ----------------------------
	
	/// <summary>
	/// Callback
	/// If our selector-Object collides with anotherObject we store the other object 
	/// 
	/// For usability purpose change color of collided object
	/// </summary>
	/// <param name="other">GameObject giben by the callback</param>
	public void OnTriggerEnter(Collider other)
	{		
		if (isOwnerCallback())
		{
			GameObject collidee = other.gameObject;
			
			if (hasObjectController(collidee))
			{
				
				collidees.Add(collidee.GetInstanceID(), collidee);
				//Debug.Log(collidee.GetInstanceID());
				
				// change color so user knows of intersection
				collidee.renderer.material.SetColor("_Color", Color.blue);
			}
		}
	}
	
	/// <summary>
	/// Callback
	/// If our selector-Object moves out of anotherObject we remove the other object from our list
	/// 
	/// For usability purpose change color of collided object
	/// </summary>
	/// <param name="other"></param>
	public void OnTriggerExit(Collider other)
	{
		if (isOwnerCallback())
		{
			GameObject collidee = other.gameObject;
			
			if (hasObjectController(collidee))
			{
				collidees.Remove(collidee.GetInstanceID());
				
				// change color so user knows of intersection end
				collidee.renderer.material.SetColor("_Color", Color.white);
			}
		}
	}
}