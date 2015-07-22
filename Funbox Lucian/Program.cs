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
  private static Spell _q2;
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
      _q = new Spell(SpellSlot.Q, 675);
      _q2 = new Spell(SpellSlot.Q, 1200);
      _w = new Spell(SpellSlot.W, 675);
      _q2.SetSkillshot(0.55f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("q", "Q Extended").SetValue(true));
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("mana", "%mana").SetValue(new Slider(40, 100, 0)));
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("q2", "Hitchance").SetValue(new StringList(new[]{"VeryHigh", "Dashing", "Immobile"})));
      _config.AddItem(new MenuItem("e", "E combo").SetValue(false));
      _config.AddItem(new MenuItem("delay2", "aa reset delay after Q, W").SetValue(new Slider(375, 400, 350)));
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region after attack
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  var ec = _config.Item("e").GetValue<bool>();
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
                  Utility.DelayAction.Add(200, CastW);
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
                  Utility.DelayAction.Add(200, CastW);
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
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      var mna = _config.SubMenu("Q Extended Settings").Item("mana").GetValue<Slider>().Value;
      var ex = _config.SubMenu("Q Extended Settings").Item("q").GetValue<bool>();
      var ex2 = _config.SubMenu("Q Extended Settings").Item("q2").GetValue<StringList>().SelectedIndex;
      var targetqe = TargetSelector.GetTarget(_q2.Range, TargetSelector.DamageType.Physical);
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
      if (ex && (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > mna && _q.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in minions)
            {
              if (ex2 == 0)
                {
                  if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
              if (ex2 == 1)
                {
                  if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.Dashing))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
              if (ex2 == 2)
                {
                  if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.Immobile))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
            }
        }
      if (_q.IsReady())
        {
          CastQ();
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  _q.CastOnUnit(qtarget);
  Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
}
#endregion
#region W
private static void CastW()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var wtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
  _w.Cast(wtarget);
    if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
}
}