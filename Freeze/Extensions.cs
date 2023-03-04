using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Data;
using System.Text;
using System.Speech;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using Newtonsoft;
using Newtonsoft.Json;
using HarmonyLib;
using ThunderRoad;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using Methods = Extensions.Methods;
using static Extensions.Methods;
using Continuum = Extensions.Continuum;
using static Extensions.Continuum;
using Action = System.Action;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Extensions;

internal static class Methods {
	public static Vector3 PlayerPosition(this Creature creature) => creature.player.transform.position;
	public static Creature GetClosestCreature(this Creature creature) {
		return Creature.allActive.Where(otherCreature => !otherCreature.isPlayer && !otherCreature.isKilled)
		               .OrderBy(otherCreature => (creature.transform.position - otherCreature.transform.position).sqrMagnitude)
		               .FirstOrDefault();
	}
	public static Item GetClosestItem(this Creature creature) {
		return Item.allActive.Where(item => item is not null)
		           .OrderBy(item => (creature.transform.position - item.transform.position).sqrMagnitude)
		           .FirstOrDefault();
	}
	public static void SliceAllParts(this Creature creature) {
		creature.Kill();
		creature.GetNeck().TrySlice();
		creature.GetLeftArm().TrySlice();
		creature.GetRightArm().TrySlice();
		creature.GetLeftLeg().TrySlice();
		creature.GetRightLeg().TrySlice();
	}
	public static RagdollPart GetRagdollPart(this Creature creature, RagdollPart.Type ragdollPartType) => creature.ragdoll.GetPart(ragdollPartType);
	public static RagdollPart GetHead(this Creature creature) => creature.ragdoll.headPart;
	public static RagdollPart GetNeck(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.Neck);
	public static RagdollPart GetTorso(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.Torso);
	public static RagdollPart GetLeftHand(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.LeftHand);
	public static RagdollPart GetRightHand(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.RightHand);
	public static RagdollPart GetLeftArm(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.LeftArm);
	public static RagdollPart GetRightArm(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.RightArm);
	public static RagdollPart GetLeftLeg(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.LeftLeg);
	public static RagdollPart GetRightLeg(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.RightLeg);
	public static RagdollPart GetLeftFoot(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.LeftFoot);
	public static RagdollPart GetRightFoot(this Creature creature) => creature.GetRagdollPart(RagdollPart.Type.RightFoot);
	public static float GetDistanceBetweenCreatureAndPlayer(this Creature creature) =>
		(creature.transform.position - Player.currentCreature.player.transform.position).sqrMagnitude;
	public static float GetDistanceBetweenItems(this Item first, Item other) => (first.transform.position -
	                                                                             other.transform.position).sqrMagnitude;
	public static float GetDistanceBetweenHands() => (Player.currentCreature.handLeft.transform.position -
	                                                  Player.currentCreature.handRight.transform.position).sqrMagnitude;
	public static bool EmptyHanded(this RagdollHand hand) => hand.grabbedHandle is not null &&
	                                                         hand.caster.telekinesis.catchedHandle is not null &&
	                                                         !hand.caster.isFiring &&
	                                                         !hand.caster.isMerging;
	public static bool GripPressed(this RagdollHand hand) => hand.playerHand.controlHand.gripPressed;
	public static bool TriggerPressed(this RagdollHand hand) => hand.playerHand.controlHand.usePressed;
	public static bool AlternateUsePressed(this RagdollHand hand) => hand.playerHand.controlHand.alternateUsePressed;
	public static Vector3 GetHandVelocity(this RagdollHand hand) => Player.currentCreature.transform.rotation *
	                                                                hand.playerHand.controlHand.GetHandVelocity();
	public static float GetHandVelocityDirection(this RagdollHand hand, Vector3 direction) => Vector3.Dot(hand.GetHandVelocity(), direction);
	public static Transform ThumbFingerTip(this RagdollHand hand) => hand.fingerThumb.tip;
	public static Vector3 DorsalHandPosition(this RagdollHand hand, float distance = 1.5f) => -hand.palmCollider.transform.forward *
	                                                                                          distance +
	                                                                                          hand.palmCollider.transform.position;
	public static Vector3 PalmarHandPosition(this RagdollHand hand, float distance = 1.5f) => -hand.palmCollider.transform.forward *
	                                                                                          -distance +
	                                                                                          hand.palmCollider.transform.position;
	public static Transform IndexFingerTip(this RagdollHand hand) => hand.fingerIndex.tip;
	public static Vector3 AboveIndexTip(this RagdollHand hand, float distance = 2.0f) => -hand.IndexFingerTip().transform.forward *
	                                                                                     -distance +
	                                                                                     hand.IndexFingerTip().transform.position;
	public static Transform MiddleFingerTip(this RagdollHand hand) => hand.fingerMiddle.tip;
	public static Vector3 AboveMiddleTip(this RagdollHand hand, float distance = 2.0f) => -hand.MiddleFingerTip().transform.forward *
	                                                                                      -distance +
	                                                                                      hand.MiddleFingerTip().transform.position;
	public static Transform RingFingerTip(this RagdollHand hand) => hand.fingerRing.tip;
	public static Vector3 AboveRingTip(this RagdollHand hand, float distance = 2.0f) => -hand.RingFingerTip().transform.forward *
	                                                                                    -distance +
	                                                                                    hand.RingFingerTip().transform.position;
	public static Transform PinkyFingerTip(this RagdollHand hand) => hand.fingerLittle.tip;
	public static Vector3 AbovePinkyTip(this RagdollHand hand, float distance = 2.0f) => -hand.PinkyFingerTip().transform.forward *
	                                                                                     -distance +
	                                                                                     hand.PinkyFingerTip().transform.position;
	public static Vector3 HandPosition(this RagdollHand hand) => hand.transform.position;
	public static Quaternion HandRotation(this RagdollHand hand) => hand.transform.rotation;
	public static RagdollPart UpperArmPart(this RagdollHand hand) => hand.upperArmPart;
	public static RagdollPart LowerArmPart(this RagdollHand hand) => hand.lowerArmPart;
	public static void LoadBrain(this Creature creature) => creature.brain.Load(creature.brain.instance.id);
	public static void StopBrain(this Creature creature) => creature.brain.Stop();
	public static Transform MagicTransform(this RagdollHand hand) => hand.caster.magic;
	public static void EndCast(this SpellCaster spellCaster) {
		spellCaster.isFiring = false;
		spellCaster.intensity = 0.0f;
		spellCaster.Fire(false);
	}
	public static void Imbue(this Item item,
	                         string spellType,
	                         float imbuePower) => item.colliderGroups[0]
	                                                  .imbue.Transfer(Catalog.GetData<SpellCastCharge>(spellType),
	                                                                  imbuePower);
	public static bool Alive(this Creature creature) => creature is not null && !creature.isKilled;
	public static void CreateExplosion(Vector3 position,
	                                   float explosionRadius = 10.0f,
	                                   float explosionForce = 5.0f) {
		var rigidbodyHashSet = new HashSet<Rigidbody>();
		var creatureHashSet = new HashSet<Creature>();
		foreach (var collider in Physics.OverlapSphere(position, explosionRadius))
			if (collider.attachedRigidbody && !rigidbodyHashSet.Contains(collider.attachedRigidbody)) {
				var creature = collider.attachedRigidbody.GetComponentInParent<Creature>();
				if (creature.Alive() &&
				    !creature.isPlayer &&
				    !creatureHashSet.Contains(creature)) {
					creature.ragdoll.SetState(Ragdoll.State.Destabilized);
					creatureHashSet.Add(creature);
				}
				if (collider.attachedRigidbody.GetComponentInParent<Player>() is not null) explosionForce /= 2.0f;
				rigidbodyHashSet.Add(collider.attachedRigidbody);
				collider.attachedRigidbody.AddExplosionForce(explosionForce,
				                                             position,
				                                             explosionRadius,
				                                             1.0f,
				                                             ForceMode.VelocityChange);
			}
	}
	public static void ElectrocuteCreaturesInRadius(Vector3 position,
	                                                float radius = 10.0f,
	                                                float electrocutionPower = 100.0f,
	                                                float electrocutionDuration = 15.0f) {
		var rigidbodyHashSet = new HashSet<Rigidbody>();
		var creatureHashSet = new HashSet<Creature>();
		foreach (var collider in Physics.OverlapSphere(position, radius))
			if (collider.attachedRigidbody && !rigidbodyHashSet.Contains(collider.attachedRigidbody)) {
				var creature = collider.attachedRigidbody.GetComponentInParent<Creature>();
				if (creature.Alive() &&
				    !creature.isPlayer &&
				    !creatureHashSet.Contains(creature)) creatureHashSet.Add(creature);
				rigidbodyHashSet.Add(collider.attachedRigidbody);
				collider.attachedRigidbody.GetComponentInParent<Creature>()
				        .TryElectrocute(electrocutionPower,
				                        electrocutionDuration,
				                        true,
				                        false,
				                        Catalog.GetData<EffectData>("ImbueLightningRagdoll"));
			}
	}
	public static bool PrimarilyX(this Vector3 vector3) => vector3.x > vector3.y &&
	                                                       vector3.x > vector3.z;
	public static bool PrimarilyY(this Vector3 vector3) => vector3.y > vector3.x &&
	                                                       vector3.y > vector3.z;
	public static bool PrimarilyZ(this Vector3 vector3) => vector3.z > vector3.x &&
	                                                       vector3.z > vector3.y;
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>() is not null
		                                                                                        ? gameObject.GetComponent<T>()
		                                                                                        : gameObject.AddComponent<T>();
	public static void InertKill(this Creature creature) {
		creature.Kill();
		creature.ragdoll.SetState(Ragdoll.State.Inert);
	}
	public static float ChangeMass(this Item item, float newMass) => item.rb.mass = newMass;
	public static Vector3 ChangeItemScale(this Item item, float newScaleSize) =>
		item.transform.localScale = new Vector3(newScaleSize, newScaleSize, newScaleSize);
	public static void Unpenetrate(this Item item) {
		if (!item.isPenetrating) return;
		for (var i = 0; i < item.collisionHandlers.Count; i++)
			foreach (var damager in item.collisionHandlers[i].damagers)
				damager.UnPenetrateAll();
	}
	public delegate void LineRendererCallback(LineRenderer lineRenderer);
	public static LineRenderer CreateLineRenderer(this GameObject gameObj, LineRendererCallback callback = null) {
		var lineRenderer = gameObj.AddComponent<LineRenderer>();
		callback?.Invoke(lineRenderer);
		return lineRenderer;
	}
	public static void AddForceAll(this Creature creature, Vector3 force, float? forceAdded = null, ForceMode forceMode = ForceMode.VelocityChange) {
		foreach (var parts in creature.ragdoll.parts) parts.rb.AddForce(force * forceAdded ?? Vector3.zero, forceMode);
	}
}
internal class Continuum {
	private Continuum continuum;
	private Func<bool> condition;
	private Action action;
	private Type type = Type.Start;
	private enum Type {
		Start,
		WaitFor,
		Do,
		End
	}
	public static Continuum Start() => new Continuum();
	public Continuum WaitFor(Func<bool> condition) {
		continuum = new Continuum {
			condition = condition,
			type = Type.WaitFor
		};
		return continuum;
	}
	public Continuum Do(Action action) {
		continuum = new Continuum {
			action = action,
			type = Type.Do
		};
		return continuum;
	}
	public void Update() {
		switch (type) {
			case Type.Start:
				if (continuum is null) {
					type = Type.End;
					return;
				}
				type = continuum.type;
				action = continuum.action;
				condition = continuum.condition;
				continuum = continuum.continuum;
				Update();
				break;
			case Type.WaitFor:
				if (condition.Invoke()) {
					if (continuum is null) {
						type = Type.End;
						return;
					}
					type = continuum.type;
					action = continuum.action;
					condition = continuum.condition;
					continuum = continuum.continuum;
					Update();
				}
				break;
			case Type.Do:
				action.Invoke();
				if (continuum is null) {
					type = Type.Start;
					return;
				}
				type = continuum.type;
				action = continuum.action;
				condition = continuum.condition;
				continuum = continuum.continuum;
				break;
			case Type.End: return;
		}
	}
}