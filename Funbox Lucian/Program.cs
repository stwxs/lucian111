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
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.AddItem(new MenuItem("e", "E combo").SetValue(true));
      _config.AddItem(new MenuItem("delay2", "aa reset delay after Q, W").SetValue(new Slider(400, 500, 350)));
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region after attack
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  var ec = _config.Item("e").GetValue<bool>();
  var meleetarget = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
  var targett = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (unit.IsMe)
    {
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
        {
          if (ec)
            {
              if (_e.IsReady())
                {
                  _e.Cast(Game.CursorPos);
                }
              else if (_q.IsReady())
                {
                  CastQ();
                }
              else if (_w.IsReady())
                {
                  CastW();
                }
            }
          else
            {
              if (_q.IsReady())
                {
                  CastQ();
                }
              else if (_w.IsReady())
                {
                  CastW();
                }
            }
        }
    }
}
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var ec = _config.Item("e").GetValue<bool>();
  var targett = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (ec && targett.Distance(ObjectManager.Player.Position) > 700)
        {
          _e.Cast(Game.CursorPos);
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var qtarget = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  _q.CastOnUnit(qtarget);
  Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
}
#endregion
#region W
private static void CastW()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var wtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  _w.Cast(wtarget);
    if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
}
}