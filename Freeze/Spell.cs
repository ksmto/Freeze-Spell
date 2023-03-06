using ThunderRoad;
using UnityEngine;
using Extensions;
using System.Collections;

namespace Freeze; 
public class Spell : SpellCastCharge {
    public float minimumHandVelocity = 1.50f;
    public float creatureFreezeTime = 10.0f;
    public float creatureFreezeColorThreshold = 0.30f;
    public float itemFreezeTime = 12.5f;
    public float range = 100.0f;
    public float radius = 0.30f;
    public override void UpdateCaster() {
        base.UpdateCaster();
        if (!spellCaster.isFiring) return;
        if (Physics.CapsuleCast(spellCaster.magic.position,
                                spellCaster.magic.forward * range,
                                radius,
                                spellCaster.magic.forward, 
                                out var hit)) {
            if (hit.collider.GetComponentInParent<Creature>() is Creature { isPlayer: false } creature) {
                if (spellCaster.ragdollHand.HandVelocityDirection(Vector3.forward) <= minimumHandVelocity) return;
                GameManager.local.StartCoroutine(FreezeCreature(creature));
            }
            if (hit.collider.GetComponentInParent<Item>() is Item item) {
                if (spellCaster.ragdollHand.HandVelocityDirection(Vector3.forward) <= minimumHandVelocity) return;
                GameManager.local.StartCoroutine(FreezeItem(item));
            }
        }
    }
    IEnumerator FreezeCreature(Creature creature) {
        creature?.ragdoll?.SetState(Ragdoll.State.Frozen);
        creature?.brain.Stop();
        creature?.SetColor(Color.Lerp(creature.GetColor(Creature.ColorModifier.Skin), Color.blue, creatureFreezeColorThreshold),
                          Creature.ColorModifier.Skin,
                          true);
        yield return new WaitForSeconds(creatureFreezeTime);
        creature?.ragdoll?.SetState(creature.ragdoll.state);
        creature?.LoadBrain();
    }
    IEnumerator FreezeItem(Item item) {
        item.rb.isKinematic = true;
        item.rb.useGravity = false;
        yield return new WaitForSeconds(itemFreezeTime);
        item.rb.isKinematic = false;
        item.rb.useGravity = true;
    }
}