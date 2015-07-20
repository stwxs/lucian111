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
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.AddItem(new MenuItem("q", "Q").SetValue(true));
      _config.AddItem(new MenuItem("w", "W").SetValue(true));
      _config.AddItem(new MenuItem("e", "E").SetValue(true));
      _config.AddToMainMenu();
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var ec = _config.Item("e").GetValue<bool>();
  var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (ec && target.IsValidTarget(900))
        _e.Cast(Game.CursorPos);
      if (!_e.IsReady())
        {
          Utility.DelayAction.Add(600, CastQ);
          if (!_q.IsReady())
            {
              Utility.DelayAction.Add(1500, CastW);
            }
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var qc = _config.Item("q").GetValue<bool>();
  var qtarget = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (qc && _q.IsReady())
    {
      _q.CastOnUnit(qtarget);
      Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
    }
}
#endregion
#region W
private static void CastW()
{
  var wc = _config.Item("w").GetValue<bool>();
  var wtarget = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
  if (wc && _w.IsReady())
    {
      _w.Cast(wtarget);
        if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
          {
            Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
          }
    }
}
#endregion
}
}