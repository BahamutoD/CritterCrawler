using System;
using UnityEngine;

namespace CritterCrawler
{
	public class CritterCrawler : PartModule
	{
		
		private AnimationState[] deployStates;
		private AnimationState[] walkStates;
		//private AnimationState[] retractStates;
		//Animation retractAnim;
		
		public bool walkEnabled = false;
		private bool initRetract = false;
		[KSPField(isPersistant = true)]
		public bool deployed = false;
		
		[KSPField(isPersistant = false)]
		public float maxSpeed = 10;
		
		[KSPField(isPersistant = false, guiActive = true, guiName = "Sticky Feet")]
		public bool stickyFeet = true;

		bool hasHiddenButtons = false;

		/*
		[KSPField(isPersistant = false, guiActive = true, guiName = "WalkNormTime")]  //debug
		public float walkNormTime = 0;
		*/
		
		[KSPAction("Toggle Legs")]
		public void AGToggleLegs(KSPActionParam param)
		{
			if(deployed)
			{
				retract();	
			}
			else
			{
				deploy ();
			}
		}
		 
		
		[KSPAction("Brakes", KSPActionGroup.Brakes)]
		public void AGBrakes(KSPActionParam param)
		{
			foreach(ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
			{
				mw.BrakesAction(param);	
			}
		}
		
		public override void OnStart (PartModule.StartState state)
		{
			deployStates = Utils.SetUpAnimation("deploy", this.part);
			walkStates = Utils.SetUpAnimation ("walk", this.part);
			
			if(!deployed)
			{
				foreach(AnimationState anim in walkStates)
				{
					anim.enabled = false;	
					anim.wrapMode = WrapMode.Loop;
				}
				
				//retract suspension
				foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
				{
					wc.suspensionDistance = 0;
				}
				
				
			}
			else
			{
				foreach(AnimationState anim in deployStates)
				{
					anim.enabled = false;	
				}
				foreach(AnimationState anim in walkStates)
				{
					anim.normalizedTime = 0;
					enabled = true;
					anim.wrapMode = WrapMode.Loop;
				}
				walkEnabled = true;
				
				//extend suspension
				foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
				{
					wc.suspensionDistance = 0.22f;
				}
			}
			
			if(HighLogic.LoadedSceneIsEditor)  //hide gui button spam
			{
				foreach(ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
				{
					mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
					mw.Events["LockSteering"].guiActiveEditor = false;
					mw.Events["DisableMotor"].guiActiveEditor = false;
					mw.Events["EnableMotor"].guiActiveEditor = false;
					mw.Events["InvertSteering"].guiActiveEditor = false;
					mw.Events["DisableMotor"].guiActiveEditor = false;
					mw.Fields["brakeTorque"].guiActiveEditor = false;
					mw.Actions["InvertSteeringAction"].active = false;
					mw.Actions["LockSteeringAction"].active = false;
					mw.Actions["UnlockSteeringAction"].active = false;
					mw.Actions["ToggleSteeringAction"].active = false;
					mw.Actions["ToggleMotorAction"].active = false;
					mw.Actions["BrakesAction"].active = false;
				}
			}

			

		}
		
		[KSPEvent(guiActive = true, guiName = "Toggle Sticky Feet", active = true)]
		public void toggleStickyFeet()
		{
			stickyFeet = !stickyFeet;	
		}
		
		[KSPEvent(guiActive = true, guiName = "Deploy", active = true)]
		public void deploy()
		{
			Events["deploy"].active = false;
			Events["retract"].active = true;
			deployed = true;
			foreach(AnimationState anim in deployStates)
			{
				if(anim.normalizedTime > 0 && anim.normalizedTime < 1)
				{
					anim.speed = 1;	
				}
				else
				{
					anim.normalizedTime = 0;
					anim.enabled = true;
					anim.speed = 1;	
				}
			}
			foreach(AnimationState anim in walkStates)
			{
				anim.wrapMode = WrapMode.Loop;
			}
			
			
		}
		
		
		[KSPEvent(guiActive = true, guiName = "Retract", active = false)]
		public void retract()
		{
			Events["deploy"].active = true;
			Events["retract"].active = false;
			deployed = false;
			foreach(AnimationState animD in deployStates)
			{
				if(animD.normalizedTime > 0 && animD.normalizedTime < 1)
				{
					animD.speed = -1;	
					initRetract = false;
				}
				else
				{
					foreach(AnimationState anim in walkStates)
					{
						//anim.wrapMode = WrapMode.Default;
						if(anim.normalizedTime>=0.5)
						{
							anim.speed = 2;
							walkEnabled = false;
							initRetract = true;
						}else
						{
							anim.speed = -2;
							walkEnabled = false;
							initRetract = true;
						}
					}
				}
			}
			
			
			
		}
		
		public override void OnUpdate()	
		{
			if(!hasHiddenButtons)
			{
				hasHiddenButtons = true;
				foreach(ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
				{
					mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
					mw.Events["LockSteering"].guiActive = false;
					mw.Events["LockSteering"].guiActiveEditor = false;
					mw.Events["DisableMotor"].guiActive = false;
					mw.Events["EnableMotor"].guiActive = false;
					mw.Fields["wheelStatus"].guiActive = false;
					mw.Fields["resourceFlowGui"].guiActive = false;
					mw.Events["InvertSteering"].guiActive = false;
					mw.Events["DisableMotor"].guiActive = false;
					mw.damageable = false;	
					mw.Fields["brakeTorque"].guiActive = false;
				}
			}
			/*/debug
			foreach(AnimationState anim in walkStates)
			{
				walkNormTime = anim.normalizedTime;
			}
			*/
			
			
			
			if(deployed)
			{
				//extend suspension
				foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
				{
					wc.suspensionDistance = Mathf.Lerp(wc.suspensionDistance, 0.22f, 0.1f);
				}
			}
			else
			{
				//retract suspension
				foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
				{
					wc.suspensionDistance = Mathf.Lerp (wc.suspensionDistance, 0, 0.1f);
				}
			}
			
			
			foreach(AnimationState anim in deployStates)
			{
				if(anim.speed == 1 && anim.normalizedTime > 1)
				{
					anim.enabled = false;
					anim.normalizedTime = 1;
					walkEnabled = true;
				}
				if(anim.speed == -1 && anim.normalizedTime<0)
				{
					anim.enabled = false;
					anim.normalizedTime = 0;
					walkEnabled = false;
				}
			}
			
			
			
			if(walkEnabled)
			{	
				
				//downforce
				if(stickyFeet)
				{
					UnityEngine.RaycastHit hitInfo = new UnityEngine.RaycastHit();
					var mask = 1 << 15;
					if(Physics.Raycast(this.part.transform.position, -this.part.transform.up, out hitInfo, 1.4f, mask))
					{
						//this.part.rigidbody.AddForce(-15*this.part.transform.up);
						this.vessel.rigidbody.AddForceAtPosition(-15*this.part.transform.up,this.vessel.CoM);
						//this.part.rigidbody.AddRelativeTorque(-20 * this.vessel.angularMomentum);
					}
					
					if(vessel.Landed)
					{
						if(vessel.srf_velocity.magnitude>maxSpeed)
						{
							rigidbody.AddForce(-2*rigidbody.velocity);
						}
						if(vessel.ctrlState.wheelThrottle == 0)
						{
							rigidbody.AddForce(-2*rigidbody.velocity);
						}
					}
				}
				
				if(vessel.Landed)
				{
					Vector3 lookVector = Quaternion.Inverse(this.part.transform.rotation) * this.vessel.GetSrfVelocity();

					foreach(AnimationState anim in walkStates)
					{
						anim.enabled = true;
						anim.speed = (float) -lookVector.z;
						if(anim.normalizedTime>1)
						{
							anim.normalizedTime = 0;
						}
						if(anim.normalizedTime<0)
						{
							anim.normalizedTime = 1;	
						}
						
					}
					float steerAngle;
					steerAngle = 20/Mathf.Clamp (Mathf.Abs (lookVector.x)/2,1,10);
					
					float wheelSteer = steerAngle*this.vessel.ctrlState.wheelSteer;
					
					//front wheels
					this.part.FindModelTransform("wheelTrans").Find ("suspensionNeutralPoint/steering").localRotation = Quaternion.Euler(0, 0, wheelSteer);
					this.part.FindModelTransform("wheelTrans_001").Find ("suspensionNeutralPoint2/steering").localRotation = Quaternion.Euler(0, 0, wheelSteer);
					
					//rear wheels
					this.part.FindModelTransform ("wheelTrans_004").Find ("suspensionNeutralPoint5/steering2").localRotation = Quaternion.Euler(0, 0, -wheelSteer);
					this.part.FindModelTransform ("wheelTrans_005").Find ("suspensionNeutralPoint6/steering2").localRotation = Quaternion.Euler(0, 0, -wheelSteer);
					
					
				}
			}
			
			if(!walkEnabled && initRetract)
			{
				foreach(AnimationState anim in walkStates)
				{
					if(anim.normalizedTime >= 1 || anim.normalizedTime <=0)
					{
						anim.enabled = false;
						anim.normalizedTime = 0;
						foreach(AnimationState ranim in deployStates)
						{
							ranim.normalizedTime = 1;
							ranim.enabled = true;
							ranim.speed = -1;
						}
						initRetract = false;
					}
				}
				
			}
		}

	}
}

