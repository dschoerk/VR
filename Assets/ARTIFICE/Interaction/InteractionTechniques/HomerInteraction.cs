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
/// Class to select and manipulate scene objects with HOMER interaction technique (IT). 
/// 
/// HOMER is a 1st person view IT
/// </summary>
public class HomerInteraction : ObjectSelectionBase
{
	/* ------------------ VRUE Tasks START -------------------
	* 	Implement Homer interaction technique
	----------------------------------------------------------------- */
	
	GameObject tracker = null;
	GameObject virutalCamera = null;
	private GameObject virtualHand = null;
	private LineRenderer lineRenderer = null;
	private float rayLength = 1000;
	
	/// <summary>
	/// </summary>
	public void Start()
	{
	}
	
	public void OnEnable()
	{
		tracker = GameObject.Find("TrackerObject");
		virutalCamera = GameObject.Find("VirtualCamera");
		virtualHand = GameObject.Find ("VirtualHand(Clone)");
		
		if (virtualHand != null) 
		{
			lineRenderer = virtualHand.AddComponent<LineRenderer> ();
			lineRenderer.SetWidth (0.02f, 0.02f);
			lineRenderer.useWorldSpace = true;
			lineRenderer.SetColors( Color.red, Color.red);
			lineRenderer.enabled = true;
		}
	}
	
	public void OnDisable()
	{
		if (lineRenderer != null) 
		{
			lineRenderer.enabled = false;
			Destroy (lineRenderer);
		}
		
		// recover color of all collidees
		prevCollidees = this.GetCollideeEnumeration ();
		IEnumerable<GameObject> actCollidees = new List<GameObject> ();
		this.RecoverCollideeColors (prevCollidees, actCollidees);
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
	
	void OnGUI()
	{
		if (Input.GetButton ("MultiSelect"))
			this.MultipleSelection = !this.MultipleSelection;
	}
	
	#region version 2.0
	private float currentTorsoHandDistance = 0;
	
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
				Ray pickRay = this.HomerRay(this.TorsoTransform, this.PhysicalHandTransform);
				this.ShowRay(pickRay);
				
				IEnumerable<RaycastHit> rayCastHits = null;
				if (!selected)
				{
					rayCastHits = this.HomerPick(pickRay, this.MultipleSelection);
					currentTorsoHandDistance = this.RaycastHitsDistance(rayCastHits);
					//if (currentTorsoHandDistance == 0)
					//	currentTorsoHandDistance = (this.PhysicalHandTransform.position - this.TorsoTransform.position).magnitude;
				}
				
				// Transform (translate and rotate) selected object depending on of virtual hand's transformation
				if (selected)
				{
					this.HomerTransformation(pickRay, currentTorsoHandDistance, this.PhysicalHandTransform, this.VirtualHandTransform);
					
					this.transformInter(this.transform.position, this.transform.rotation);
				}
				else
				{
					// move virtual hand like physical hand
					this.transform.position  = tracker.transform.position;
					this.transform.rotation = tracker.transform.rotation;
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
	#endregion
	
	private float RaycastHitsDistance(IEnumerable<RaycastHit> rayCastHits)
	{
		float distance_average = 0;
		if (rayCastHits != null && rayCastHits.Count() > 0)
			distance_average = rayCastHits.Sum(rayCastHit => rayCastHit.distance) / rayCastHits.Count();
		
		return distance_average;
	}
	
	private bool _MultipleSelection = true;
	private DateTime MultipleSelectionSetTime = DateTime.MinValue;
	private TimeSpan MultipleSelectionSetTimeout = new TimeSpan (0, 0, 0, 0, 500);
	public bool MultipleSelection
	{
		get
		{
			return this._MultipleSelection;
		}
		set
		{
			// use timeout to avoid permanent toggle
			if (DateTime.Now > this.MultipleSelectionSetTime)
			{
				this._MultipleSelection = value;
				this.MultipleSelectionSetTime = DateTime.Now + this.MultipleSelectionSetTimeout;
			}
		}
	}
	
	protected IEnumerable<RaycastHit> Pick(Ray pickRay, bool multiple, float distance, int layerMask)
	{
		IList<RaycastHit> rayCastHits = new List<RaycastHit>();
		
		RaycastHit hitInfo;
		if (multiple) 
		{
			while (Physics.Raycast (pickRay, out hitInfo))//, distance, layerMask)) 
			{
				rayCastHits.Add (hitInfo);
				hitInfo.collider.enabled = false;
			}
			rayCastHits.All (rayCastHit => rayCastHit.collider.enabled = true);
		}
		else if (Physics.Raycast (pickRay, out hitInfo))
		{
			rayCastHits.Add(hitInfo);
		}

		Debug.Log ("No of picks: " + rayCastHits.Count);
		
		return rayCastHits;
	}
	
	protected Ray HomerRay(Transform trans_torso, Transform trans_hand)
	{
		// pick colliders along pickray
		Vector3 origin = trans_hand.position;
		Vector3 direction = (trans_hand.position - trans_torso.position).normalized;
		Ray pickRay = new Ray(origin, direction);
		return pickRay;
	}
	
	protected void ShowRay(Ray ray)
	{
		lineRenderer.enabled = true;
		
		// show ray in scene
		lineRenderer.SetPosition (0, ray.origin);
		lineRenderer.SetPosition (1, ray.origin + ray.direction * this.rayLength);
	}
	
	private Dictionary<GameObject, Color> ColideesColorMap = new Dictionary<GameObject, Color> ();
	
	private IEnumerable<GameObject> prevCollidees = null;
	
	private IEnumerable<GameObject> GetCollideeEnumeration()
	{
		List<GameObject> myCollidees = new List<GameObject>();
		IEnumerable<GameObject> colls = from collidee in collidees.Cast<DictionaryEntry> () select collidee.Value as GameObject;
		if (colls.Any())
			myCollidees = colls.ToList<GameObject> ();
		
		return myCollidees;
	}

	private int MyLayerMask
	{
		get
		{
			//return 1 << LayerMask.NameToLayer("Default");
			//return 1 << LayerMask.NameToLayer("Ignore Raycast");
			return LayerMask.GetMask(new string[]{
				"Ignore Raycast"
			});
//			return LayerMask.GetMask(new string[]{
//				"Default"
//			});
		}
	}
	
	#region version 2.0
	protected IEnumerable<RaycastHit> HomerPick(Ray pickRay, bool multiple)
	{	
		//Pick(Ray pickRay, bool multiple, float distance, int layerMask)
		IEnumerable<RaycastHit> rayCastHits = Pick(pickRay, multiple, this.rayLength, this.MyLayerMask);
		
		prevCollidees = this.GetCollideeEnumeration ();
		IEnumerable<GameObject> actCollidees = from rayCastHit in rayCastHits select rayCastHit.collider.gameObject;
		
		this.RecoverCollideeColors (prevCollidees, actCollidees);
		
		collidees.Clear ();
		
		foreach (GameObject collidee in actCollidees) 
		{
			//this.collidees.Add(collidee, collidee);
			this.collidees.Add(collidee.GetInstanceID(), collidee);

			// backup color object
			this.BackupCollideeColor(collidee);
			
			collidee.renderer.material.SetColor("_Color", Color.blue);
		}
		
		return rayCastHits;
		
	}
	#endregion
	
	protected void RecoverCollideeColors(IEnumerable<GameObject> prevCollidees, IEnumerable<GameObject> actCollidees)
	{
		IEnumerable<GameObject> collideesToReset =  prevCollidees.Except (actCollidees);
		
		foreach (GameObject collidee in collideesToReset) 
		{
			if (ColideesColorMap.ContainsKey(collidee))
				collidee.renderer.material.SetColor("_Color", ColideesColorMap[collidee]);
		}
	}
	
	protected void BackupCollideeColor(GameObject collidee)
	{
		// backup color object
		Color actColor = collidee.renderer.material.GetColor("_Color");
		if (!this.ColideesColorMap.ContainsKey(collidee))
			this.ColideesColorMap.Add(collidee, actColor);
	}
	
	#region version 2.0
	protected void HomerTransformation(Ray pickRay, float distance_average, Transform trans_hand, Transform trans_hand_homer)
	{
		Vector3 pos_virtHand =  pickRay.origin + distance_average * pickRay.direction;
		trans_hand_homer.position = pos_virtHand;
		trans_hand_homer.rotation = trans_hand.rotation;
	}
	#endregion
	
	// ------------------ VRUE Tasks END ----------------------------
}