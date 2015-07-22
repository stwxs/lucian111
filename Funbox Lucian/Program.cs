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
  private static string[] select = {"Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune","Quinn","Sivir","Teemo","Tristana","TwistedFate","Twitch","Urgot","Varus","Vayne"};
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
      _q2 = new Spell(SpellSlot.Q, 1100);
      _q2.SetSkillshot(0.25f, 40, 3000, false, SkillshotType.SkillshotLine);
      _w = new Spell(SpellSlot.W, 1000);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("q", "Q Extended").SetValue(true));
      foreach (var enemy in HeroManager.Enemies)
      {
        _config.SubMenu("Q Extended Settings").AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName).SetValue(false));
        for (int i = 0; i < select.Length; i++)
          {
            select[i] = enemy.ChampionName;
            _config.SubMenu("Q Extended Settings").Item(select[i]).SetValue(true);
          }
      }
      _config.SubMenu("E Settings").AddItem(new MenuItem("e", "E combo").SetValue(true));
      _config.SubMenu("E Settings").AddItem(new MenuItem("e2", "E if enemy out of attack range").SetValue(false));
      _config.AddItem(new MenuItem("delay2", "aa reset delay after Q, W").SetValue(new Slider(450, 500, 400)));
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region after attack
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  var ec = _config.SubMenu("E Settings").Item("e").GetValue<bool>();
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
                  Utility.DelayAction.Add(250, CastW);
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
                  Utility.DelayAction.Add(250, CastW);
                }
            }
        }
    }
}
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var ec = _config.SubMenu("E Settings").Item("e").GetValue<bool>();
  var ecc = _config.SubMenu("E Settings").Item("e2").GetValue<bool>();
  var targett = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      var ex = _config.SubMenu("Q Extended Settings").Item("q").GetValue<bool>();
      var targetqe = TargetSelector.GetTarget(_q2.Range, TargetSelector.DamageType.Physical);
      var collisions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q2.Range, MinionTypes.All, MinionTeam.NotAlly);
      if (ex && _q2.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in collisions)
            {
              var p = new Geometry.Polygon.Rectangle(ObjectManager.Player.ServerPosition, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), _q2.Width);
              if (p.IsInside(targetqe))
                {
                  _q2.CastOnUnit(minion);
                }
            }
        }
      if (_q.IsReady())
        {
          CastQ();
        }
    }
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (ecc && ec && targett.Distance(ObjectManager.Player.Position) > 700)
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