using System;
using LeagueSharp;
using LeagueSharp.Common;
namespace Lucian
{
public class Program
{
#region declarations
  private static Menu _config;
  private static Orbwalking.Orbwalker _orbwalker;
  private static Spell _q;
  private static Spell _w;
  private static Spell _e;
#endregion
#region Main
  private static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
#endregion
#region OnGameLoad
  private static void Game_OnGameLoad(EventArgs args)
    {
      if (ObjectManager.Player.ChampionName != "Lucian")
        return;
      _q = new Spell(SpellSlot.Q, 700);
      _w = new Spell(SpellSlot.W, 1000);
      _w.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _e = new Spell(SpellSlot.E);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.AddItem(new MenuItem("res", "RESET AA").SetValue(true));
      _config.AddToMainMenu();
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var reset = _config.Item("res").GetValue<bool>();
  var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (!_e.IsReady())
        {
          Utility.DelayAction.Add(600, CastQ);
          Utility.DelayAction.Add(2000, CastW);
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var reset = _config.Item("res").GetValue<bool>();
  var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (_q.IsReady())
    {
      _q.CastOnUnit(target);
        if (reset)
          {
            Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
          }
    }
}
#endregion
#region W
private static void CastW()
{
  var reset = _config.Item("res").GetValue<bool>();
  var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (_w.IsReady())
    {
      _w.Cast(target);
        if (reset && _w.Cast(target) == Spell.CastStates.SuccessfullyCasted)
          {
            Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
          }
    }
}
#endregion
}
}