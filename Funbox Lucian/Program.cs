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
  private static Spell _w2;
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
      _w2 = new Spell(SpellSlot.W, 1000);
      _q2.SetSkillshot(0.55f, 75, float.MaxValue, false, SkillshotType.SkillshotLine);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _w2.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
      _w2.MinHitChance = HitChance.High;
      _e = new Spell(SpellSlot.E, 475);
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Q Extended Settings").SubMenu("select champions").AddItem(new MenuItem("nhgfr", "use Q Extended on:"));
      foreach (var hero in HeroManager.Enemies)
        {
          _config.SubMenu("Q Extended Settings").SubMenu("select champions").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName)));
        }
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("q", "Q Extended").SetValue(true));
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("mana", "%mana").SetValue(new Slider(40, 100, 0)));
      _config.SubMenu("Q Extended Settings").AddItem(new MenuItem("q2", "Hitchance").SetValue(new StringList(new[]{"VeryHigh", "Dashing", "Immobile"})));
      _config.SubMenu("Harras").AddItem(new MenuItem("mana2", "%mana").SetValue(new Slider(40, 100, 0)));
      _config.SubMenu("Harras").AddItem(new MenuItem("qh", "use Q").SetValue(true));
      _config.SubMenu("Harras").AddItem(new MenuItem("wh", "use W").SetValue(true));
      _config.SubMenu("Harras").AddItem(new MenuItem("eh", "use E").SetValue(false));
      _config.AddItem(new MenuItem("e", "E combo").SetValue(false));
      _config.AddItem(new MenuItem("delay2", "reset aa").SetValue(new Slider(350, 375, 325)));
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region after attack
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  if (unit.IsMe)
    {
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
        {
          if (_config.Item("e").GetValue<bool>())
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
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
        {
          if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > (_config.SubMenu("Harras").Item("mana2").GetValue<Slider>().Value))
            {
              if ((_config.SubMenu("Harras").Item("eh").GetValue<bool>()) && _e.IsReady())
                {
                  _e.Cast(Game.CursorPos);
                }
              else if ((_config.SubMenu("Harras").Item("wh").GetValue<bool>()) && _w2.IsReady())
                {
                  _w2.Cast(TargetSelector.GetTarget(_w2.Range, TargetSelector.DamageType.Physical));
                }
            }
        }
    }
}
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > (_config.SubMenu("Harras").Item("mana2").GetValue<Slider>().Value))
        {
          if ((_config.SubMenu("Harras").Item("qh").GetValue<bool>()) && _q.IsReady())
            {
              CastQ();
            }
        }
    }
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      if ((_config.SubMenu("Q Extended Settings").Item("q").GetValue<bool>()) && ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > (_config.SubMenu("Q Extended Settings").Item("mana").GetValue<Slider>().Value)) && _q.IsReady() && (HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").SubMenu("select champions").Item("auto" + hero.ChampionName).GetValue<bool>())).Distance(ObjectManager.Player.Position) > _q.Range && (HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").SubMenu("select champions").Item("auto" + hero.ChampionName).GetValue<bool>())).CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly))
            {
              if ((_config.SubMenu("Q Extended Settings").Item("q2").GetValue<StringList>().SelectedIndex) == 0)
                {
                  if (_q2.WillHit((HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").SubMenu("select champions").Item("auto" + hero.ChampionName).GetValue<bool>())), ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
              if ((_config.SubMenu("Q Extended Settings").Item("q2").GetValue<StringList>().SelectedIndex) == 1)
                {
                  if (_q2.WillHit((HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").SubMenu("select champions").Item("auto" + hero.ChampionName).GetValue<bool>())), ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.Dashing))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
              if ((_config.SubMenu("Q Extended Settings").Item("q2").GetValue<StringList>().SelectedIndex) == 2)
                {
                  if (_q2.WillHit((HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.SubMenu("Q Extended Settings").SubMenu("select champions").Item("auto" + hero.ChampionName).GetValue<bool>())), ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.Immobile))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
            }
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  _q.CastOnUnit(TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical));
  Utility.DelayAction.Add((_config.Item("delay2").GetValue<Slider>().Value), Orbwalking.ResetAutoAttackTimer);
}
#endregion
#region W
private static void CastW()
{
  _w.Cast(TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical));
    if (_w.Cast(TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical)) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add((_config.Item("delay2").GetValue<Slider>().Value), Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
}
}