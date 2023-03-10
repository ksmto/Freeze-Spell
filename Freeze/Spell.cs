using Extensions;
using ThunderRoad;
using UnityEngine;
using System.Collections;

public class Spell : SpellCastCharge {
    public float handVelocity = 1.50f;
    public float creatureFreezeTime = 10.0f;
    public float creatureFreezeColorThreshold = 0.30f;
    public float itemFreezeTime = 12.5f;
    public override void UpdateCaster() {
        base.UpdateCaster();
        if (!spellCaster.isFiring) return;
        if (!Physics.CapsuleCast(spellCaster.magic.position,
                                 spellCaster.magic.forward * 2.0f,
                                 0.75f,
                                 spellCaster.magic.forward,
                                 out var hit)) return;
        if (hit.collider.GetComponentInParent<Creature>() is Creature { isPlayer: false } creature) {
            if (spellCaster.ragdollHand.HandMovementDirection(Vector3.forward) < handVelocity) return;
            GameManager.local.StartCoroutine(FreezeCreature(creature));
        }
        if (hit.collider.GetComponentInParent<Item>() is not Item item) return;
        if (spellCaster.ragdollHand.HandMovementDirection(Vector3.forward) < handVelocity) return;
        GameManager.local.StartCoroutine(FreezeItem(item));
    }
    private IEnumerator FreezeCreature(Creature creature) {
        creature?.ragdoll?.SetState(Ragdoll.State.Frozen);
        creature?.brain?.Stop();
        creature?.SetColor(Color.Lerp(creature.GetColor(Creature.ColorModifier.Skin), Color.blue, creatureFreezeColorThreshold),
                          Creature.ColorModifier.Skin,
                          true);
        yield return new WaitForSeconds(creatureFreezeTime);
        creature?.ragdoll?.SetState(Ragdoll.State.Destabilized);
        creature?.LoadBrain();
    }
    private IEnumerator FreezeItem(Item item) {
        if (item is null) yield break;
        item.rb.isKinematic = true;
        item.rb.useGravity = false;
        yield return new WaitForSeconds(itemFreezeTime);
        item.rb.isKinematic = false;
        item.rb.useGravity = true;
    }
}