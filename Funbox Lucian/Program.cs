using System;
using System.Linq;
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
      _q = new Spell(SpellSlot.Q, 675);
      _q2 = new Spell(SpellSlot.Q, 1200);
      _w = new Spell(SpellSlot.W, 1000);
      _q2.SetSkillshot(0.55f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      foreach (var hero in HeroManager.Enemies)
        {
          _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName)));
        }
      _config.AddItem(new MenuItem("e", "E combo").SetValue(false));
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
                  Utility.DelayAction.Add(250, CastQ);
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
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").Item("auto" + hero.ChampionName).GetValue<bool>());
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > 40 && _q.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in minions)
            {
              if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                {
                  _q2.CastOnUnit(minion);
                }
            }
        }
      if (_q.IsReady())
        {
          if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > 40)
            {
              CastQ();
            }
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  _q.CastOnUnit(qtarget);
  Utility.DelayAction.Add(400, Orbwalking.ResetAutoAttackTimer);
}
#endregion
#region W
private static void CastW()
{
  var wtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
  _w.Cast(wtarget);
    if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(400, Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
}
}