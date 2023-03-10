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
using Action = System.Action;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Extensions {
	internal static class Methods {
		public static Vector3 PlayerPosition(this Creature creature) => creature.player.transform.position;
		public static Creature GetClosestCreature() => Creature.allActive.Where(creature => creature.Alive() && !creature.isPlayer)
		                                                       .OrderBy(creature =>
			                                                                (Player.currentCreature.PlayerPosition() - creature.transform.position)
			                                                                .sqrMagnitude)
		                                                       .FirstOrDefault();
		public static Item GetClosestItem() => Item.allActive.Where(item => item is not null)
		                                           .OrderBy(item => (Player.currentCreature.PlayerPosition() - item.transform.position).sqrMagnitude)
		                                           .FirstOrDefault();
		public static void SliceAllParts(this Creature creature) {
			foreach (var allParts in creature?.ragdoll?.parts) allParts?.TrySlice();
		}
		public static RagdollPart GetRagdollPart(this Creature creature, RagdollPart.Type ragdollPartType) =>
			creature?.ragdoll?.GetPart(ragdollPartType);
		public static RagdollPart Head(this Creature creature) => creature?.ragdoll?.headPart;
		public static RagdollPart Neck(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.Neck);
		public static RagdollPart Torso(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.Torso);
		public static RagdollPart LeftHand(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.LeftHand);
		public static RagdollPart RightHand(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.RightHand);
		public static RagdollPart LeftArm(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.LeftArm);
		public static RagdollPart RightArm(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.RightArm);
		public static RagdollPart LeftLeg(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.LeftLeg);
		public static RagdollPart RightLeg(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.RightLeg);
		public static RagdollPart LeftFoot(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.LeftFoot);
		public static RagdollPart RightFoot(this Creature creature) => creature?.GetRagdollPart(RagdollPart.Type.RightFoot);
		public static float DistanceBetweenCreatureAndPlayer(this Creature creature) =>
			(creature.transform.position - Player.currentCreature.player.transform.position).sqrMagnitude;
		public static float DistanceBetweenItems(this Item first, Item other) => (first.transform.position - other.transform.position).sqrMagnitude;
		public static float DistanceBetweenHands() =>
			Vector3.Distance(Player.currentCreature.handLeft.transform.position, Player.currentCreature.handRight.transform.position);
		public static bool EmptyHanded(this RagdollHand hand) => hand?.grabbedHandle is not null &&
		                                                         hand?.caster?.telekinesis?.catchedHandle is not null &&
		                                                         !hand.caster.isFiring &&
		                                                         !hand.caster.isMerging;
		public static bool GripPressed(this RagdollHand hand) => hand.playerHand.controlHand.gripPressed;
		public static bool TriggerPressed(this RagdollHand hand) => hand.playerHand.controlHand.usePressed;
		public static bool AlternateUsePressed(this RagdollHand hand) => hand.playerHand.controlHand.alternateUsePressed;
		public static bool TriggerPressed(this Item item) => item.mainHandler.playerHand.controlHand.usePressed;
		public static bool AlternateUsePressed(this Item item) => item.mainHandler.playerHand.controlHand.alternateUsePressed;
		public static Vector3 HandVelocity(this RagdollHand hand) => Player.currentCreature.transform.rotation *
		                                                             hand.playerHand.controlHand.GetHandVelocity();
		public static float HandMovementDirection(this RagdollHand hand, Vector3 direction) => Vector3.Dot(hand.HandVelocity(), direction);
		public static Transform ThumbFingerTip(this RagdollHand hand) => hand.fingerThumb.tip;
		public static Vector3 BackHandPosition(this RagdollHand hand, float distance = 1.5f) =>
			hand.palmCollider.transform.position + -hand.palmCollider.transform.forward * -distance;
		public static Vector3 FrontHandPosition(this RagdollHand hand, float distance = 1.5f) =>
			hand.palmCollider.transform.position + -hand.palmCollider.transform.forward * distance;
		public static Transform IndexFingerTip(this RagdollHand hand) => hand.fingerIndex.tip;
		public static Vector3 AboveIndexTip(this RagdollHand hand, float distance = 2.0f) =>
			hand.IndexFingerTip().position + -hand.IndexFingerTip().transform.forward * -distance;
		public static Transform MiddleFingerTip(this RagdollHand hand) => hand.fingerMiddle.tip;
		public static Vector3 AboveMiddleTip(this RagdollHand hand, float distance = 2.0f) =>
			hand.MiddleFingerTip().transform.position + -hand.MiddleFingerTip().transform.forward * -distance;
		public static Transform RingFingerTip(this RagdollHand hand) => hand.fingerRing.tip;
		public static Vector3 AboveRingTip(this RagdollHand hand, float distance = 2.0f) =>
			hand.RingFingerTip().transform.position + -hand.RingFingerTip().transform.forward * -distance;
		public static Transform PinkyFingerTip(this RagdollHand hand) => hand.fingerLittle.tip;
		public static Vector3 AbovePinkyTip(this RagdollHand hand, float distance = 2.0f) =>
			hand.PinkyFingerTip().transform.position + -hand.PinkyFingerTip().transform.forward * -distance;
		public static void LoadBrain(this Creature creature) => creature?.brain?.Load(creature.brain?.instance?.id);
		public static void StopBrain(this Creature creature) => creature?.brain?.Stop();
		public static void EndCast(this SpellCaster spellCaster) {
			spellCaster.isFiring = false;
			spellCaster.intensity = 0.0f;
			spellCaster?.Fire(false);
		}
		public static void Imbue(this Item item, string spellType, float imbuePower) =>
			item?.colliderGroups[0]?.imbue?.Transfer(Catalog.GetData<SpellCastCharge>(spellType), imbuePower);
		public static bool Alive(this Creature creature) => !creature?.isKilled ?? false;
		public static void CreateExplosion(Vector3 position,
		                                   float explosionRadius = 10.0f,
		                                   float explosionForce = 25.0f) {
			var rigidbodyHashSet = new HashSet<Rigidbody>();
			var creatureHashSet = new HashSet<Creature>();
			foreach (var collider in Physics.OverlapSphere(position, explosionRadius))
				if (collider.attachedRigidbody &&
				    !rigidbodyHashSet.Contains(collider.attachedRigidbody)) {
					var creature = collider.attachedRigidbody?.GetComponentInParent<Creature>();
					if (creature.Alive() &&
					    !creature.isPlayer &&
					    !creatureHashSet.Contains(creature)) {
						creature.ragdoll?.SetState(Ragdoll.State.Destabilized);
						creatureHashSet.Add(creature);
					}
					if (collider.attachedRigidbody?.GetComponentInParent<Player>() is not null) explosionForce /= 2.0f;
					rigidbodyHashSet.Add(collider.attachedRigidbody);
					collider.attachedRigidbody?.AddExplosionForce(explosionForce,
					                                              position,
					                                              explosionRadius,
					                                              1.0f,
					                                              ForceMode.VelocityChange);
				}
		}
		public static bool PrimarilyX(this Vector3 vector3) => vector3.x > vector3.y && vector3.x > vector3.z;
		public static bool PrimarilyY(this Vector3 vector3) => vector3.y > vector3.x && vector3.y > vector3.z;
		public static bool PrimarilyZ(this Vector3 vector3) => vector3.z > vector3.x && vector3.z > vector3.y;
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject?.GetComponent<T>()
			                                                                                        ? gameObject?.AddComponent<T>()
			                                                                                        : gameObject?.GetComponent<T>();
		public static T GetAndFindComponet<T>(this GameObject gameObject,
		                                      string find) where T : Component => gameObject?.transform.Find(find)?.GetComponent<T>();
		public static T GetAndFindComponet<T>(this Transform transform,
		                                      string find) where T : Component => transform?.gameObject.transform.Find(find)?.GetComponent<T>();
		public static T GetAndFindComponet<T>(this Rigidbody rigidbody,
		                                      string find) where T : Component => rigidbody?.gameObject.transform.Find(find)?.GetComponent<T>();
		public static T GetAndFindComponet<T>(this Item item,
		                                      string find) where T : Component => item?.gameObject.transform.Find(find)?.GetComponent<T>();
		public static void InertKill(this Creature creature) {
			creature?.Kill();
			creature?.ragdoll?.SetState(Ragdoll.State.Inert);
		}
		public static Vector3 ChangeScale(this Item item, float newScaleSize) =>
			item.transform.localScale = new Vector3(newScaleSize, newScaleSize, newScaleSize);
		public static void Unpenetrate(this Item item) {
			foreach (var collisionHandlers in item?.collisionHandlers)
				foreach (var damager in collisionHandlers?.damagers)
					damager?.UnPenetrateAll();
			
		}
		public static void AddForce(this Creature creature,
		                                 Vector3 force,
		                                 float? forceAdded = null,
		                                 ForceMode forceMode = ForceMode.Impulse) {
			foreach (var parts in creature?.ragdoll?.parts) parts?.rb?.AddForce(force * forceAdded ?? Vector3.zero, forceMode);
		}
		public static void Destroy(this GameObject gameObject) => Destroy(gameObject);
		public static Color Darken(this Color color, float darkenSpeed) => Color.Lerp(color, Color.clear, darkenSpeed);
		public static void AddForceTowards(this Rigidbody rigidbody,
		                                   Transform target,
		                                   float? forceAdded = null,
		                                   ForceMode forceMode = ForceMode.VelocityChange) {
			rigidbody?.AddForce((rigidbody.position - target.position).normalized * forceAdded ?? Vector3.zero, forceMode);
		}
		public static float Sqrt(this float number) => Mathf.Sqrt(number);
		public static float Abs(this float number) => Mathf.Abs(number);
		public static float RandomNumber(float minimum = Mathf.NegativeInfinity, float maximum = Mathf.Infinity) => Random.Range(minimum, maximum);
		public static void Clone(this Item item,
		                         Vector3 position,
		                         Quaternion rotation,
		                         Vector3? forceDirection = null,
		                         float? forceAdded = null,
		                         ForceMode forceMode = ForceMode.Impulse) {
			item.data.SpawnAsync(newItem => {
				newItem.transform.position = position;
				newItem.transform.rotation = rotation;
				newItem.rb.AddForce(forceDirection ?? Vector3.up * forceAdded ?? Vector3.zero, forceMode);
			});
		}
		public delegate void LineCallback(LineRenderer lineRenderer);
		public static LineRenderer CreateLine(this GameObject gameObj, [CanBeNull] LineCallback callback = null) {
			var lineRenderer = gameObj?.AddComponent<LineRenderer>();
			callback?.Invoke(lineRenderer);
			return lineRenderer;
		}
	}
}
public class Continuum {
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