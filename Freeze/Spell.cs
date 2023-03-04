using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using Extensions;
using UnityEngine;

namespace Freeze;
public class Spell : SpellCastProjectile {
	public float freezeTime = 7.5f;
	public float freezeColorAmount = 0.25f;
	public override void Throw(Vector3 velocity) {
		base.Throw(velocity);
		bool throwAllowed;
		throwAllowed = guidedProjectile is not null;
	}
	protected override void OnProjectileCollision(ItemMagicProjectile projectile, CollisionInstance collisionInstance) {
		base.OnProjectileCollision(projectile, collisionInstance);
		if (collisionInstance.damageStruct.hitRagdollPart?.ragdoll.creature is Creature { isPlayer: false } creature)
			GameManager.local.StartCoroutine(FreezeCreature(creature));
		if (collisionInstance.damageStruct.hitItem is Item item) GameManager.local.StartCoroutine(FreezeItem(item));
	}
	public override bool OnImbueCollisionStart(CollisionInstance collisionInstance) {
		if (collisionInstance.damageStruct.hitRagdollPart?.ragdoll.creature is Creature { isPlayer: false } creature)
			GameManager.local.StartCoroutine(FreezeCreature(creature));
		if (collisionInstance.damageStruct.hitItem is Item item) GameManager.local.StartCoroutine(FreezeItem(item));
		return base.OnImbueCollisionStart(collisionInstance);
	}
	private IEnumerator FreezeCreature(Creature creature) {
		creature.StopBrain();
		creature.ragdoll.SetState(Ragdoll.State.Frozen);
		creature.SetColor(Color.Lerp(creature.GetColor(Creature.ColorModifier.Skin), 
		                             Color.blue,
		                             freezeColorAmount),
		                  Creature.ColorModifier.Skin,
		                  true);
		yield return new WaitForSeconds(freezeTime);
		creature.Kill();
	}
	private IEnumerator FreezeItem(Item item) {
		item.rb.isKinematic = true;
		item.rb.useGravity = false;
		yield return new WaitForSeconds(freezeTime);
		item.rb.isKinematic = false;
		item.rb.useGravity = true;
	}
}