using Server.Engines.MLQuests;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Spellweaving
{
  public abstract class ArcanistSpell : Spell
  {
    private int m_CastTimeFocusLevel;

    public ArcanistSpell(Mobile caster, Item scroll, SpellInfo info)
      : base(caster, scroll, info)
    {
    }

    public abstract double RequiredSkill { get; }
    public abstract int RequiredMana { get; }

    public override SkillName CastSkill => SkillName.Spellweaving;
    public override SkillName DamageSkill => SkillName.Spellweaving;

    public override bool ClearHandsOnCast => false;

    public virtual int FocusLevel => m_CastTimeFocusLevel;

    public static int GetFocusLevel(Mobile from)
    {
      ArcaneFocus focus = FindArcaneFocus(from);

      return focus?.Deleted != false ? 0 : focus.StrengthBonus;
    }

    public static ArcaneFocus FindArcaneFocus(Mobile from) => from.Holding as ArcaneFocus ?? from.Backpack?.FindItemByType<ArcaneFocus>();

    public static bool CheckExpansion(Mobile from) => !(from is PlayerMobile) || from.NetState?.SupportsExpansion(Expansion.ML) == true;

    public override bool CheckCast()
    {
      if (!base.CheckCast())
        return false;

      Mobile caster = Caster;

      if (!CheckExpansion(caster))
      {
        caster.SendLocalizedMessage(
          1072176); // You must upgrade to the Mondain's Legacy Expansion Pack before using that ability
        return false;
      }

      if (caster is PlayerMobile mobile)
      {
        MLQuestContext context = MLQuestSystem.GetContext(mobile);

        if (context?.Spellweaving != true)
        {
          mobile.SendLocalizedMessage(
            1073220); // You must have completed the epic arcanist quest to use this ability.
          return false;
        }
      }

      int mana = ScaleMana(RequiredMana);

      if (caster.Mana < mana)
      {
        caster.SendLocalizedMessage(1060174,
          mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
        return false;
      }

      if (caster.Skills[CastSkill].Value < RequiredSkill)
      {
        caster.SendLocalizedMessage(1063013,
          $"{RequiredSkill:F1}\t{"#1044114"}"); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
        return false;
      }

      return true;
    }

    public override void GetCastSkills(out double min, out double max)
    {
      min = RequiredSkill - 12.5; // per 5 on Friday, 2/16/07
      max = RequiredSkill + 37.5;
    }

    public override int GetMana() => RequiredMana;

    public override void DoFizzle()
    {
      Caster.PlaySound(0x1D6);
      Caster.NextSpellTime = Core.TickCount;
    }

    public override void DoHurtFizzle()
    {
      Caster.PlaySound(0x1D6);
    }

    public override void OnDisturb(DisturbType type, bool message)
    {
      base.OnDisturb(type, message);

      if (message)
        Caster.PlaySound(0x1D6);
    }

    public override void OnBeginCast()
    {
      base.OnBeginCast();

      SendCastEffect();
      m_CastTimeFocusLevel = GetFocusLevel(Caster);
    }

    public virtual void SendCastEffect()
    {
      Caster.FixedEffect(0x37C4, 10, (int)(GetCastDelay().TotalSeconds * 28), 4, 3);
    }

    public virtual bool CheckResisted(Mobile m)
    {
      double percent =
        (50 + 2 * (GetResistSkill(m) - GetDamageSkill(Caster))) /
        100; // TODO: According to the guide this is it.. but.. is it correct per OSI?

      if (percent <= 0)
        return false;

      if (percent >= 1.0)
        return true;

      return percent >= Utility.RandomDouble();
    }
  }
}