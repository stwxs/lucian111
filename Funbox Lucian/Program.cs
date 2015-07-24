using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
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
      _e = new Spell(SpellSlot.E, 475);
      _q2.SetSkillshot(0.55f, 35, float.MaxValue, false, SkillshotType.SkillshotLine);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Combo").SubMenu("Q Settings").AddItem(new MenuItem("qcaa", "Q before attack").SetValue(false));
      _config.SubMenu("Combo").SubMenu("Q Settings").AddItem(new MenuItem("qcaaa", "Q after attack E off").SetValue(false));
      _config.SubMenu("Combo").SubMenu("E Settings").AddItem(new MenuItem("e", "E combo").SetValue(false));
      //_config.SubMenu("Killsteal").AddItem(new MenuItem("qec" , "Q Extended").SetValue(true));
      _config.SubMenu("Harass").SubMenu("Q normal Settings").AddItem(new MenuItem("qn" , "normal Q - target in autoattack range").SetValue(true));
      _config.SubMenu("Harass").SubMenu("Q normal Settings").AddItem(new MenuItem("aqn" , "Auto normal Q - target in autoattack range").SetValue(false));
      _config.SubMenu("Harass").SubMenu("Q Extended Settings").AddItem(new MenuItem("qe" , "Q Extended").SetValue(true));
      _config.SubMenu("Harass").SubMenu("Q Extended Settings").AddItem(new MenuItem("aqe" , "Auto Q Extended").SetValue(false));
      _config.SubMenu("Harass").AddItem(new MenuItem("info1", "ON:"));
      foreach (var hero in HeroManager.Enemies)
        {
          _config.SubMenu("Harass").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName)));
        }
      _config.SubMenu("Harass").AddItem(new MenuItem("manah", "%mana").SetValue(new Slider(33, 100, 0)));
      _config.SubMenu("Draw").AddItem(new MenuItem("qnd", "Q normal").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
      _config.SubMenu("Draw").AddItem(new MenuItem("qed", "Q Extended").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Drawing.OnDraw += OnDraw;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region after attack
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  var ec = _config.Item("e").GetValue<bool>();
  var qbef = _config.Item("qcaa").GetValue<bool>();
  var qaft = _config.Item("qcaaa").GetValue<bool>();
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
                  if (!qbef)
                    {
                      Utility.DelayAction.Add(100, CastQ);
                    }
                }
              else if (_w.IsReady())
                {
                  Utility.DelayAction.Add(150, CastW);
                }
            }
          else
            {
              if (_q.IsReady())
                {
                  if (qaft)
                    {
                      CastQ();
                    }
                }
              else if (_w.IsReady())
                {
                  Utility.DelayAction.Add(150, CastW);
                }
            }
        }
    }
}
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var qbef = _config.Item("qcaa").GetValue<bool>();
  var qex = _config.Item("qe").GetValue<bool>();
  var aqex = _config.Item("aqe").GetValue<bool>();
  //var qexc = _config.Item("qec").GetValue<bool>();
  var autoqnor = _config.Item("aqn").GetValue<bool>();
  var qnor = _config.Item("qn").GetValue<bool>();
  if (qbef && (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
    {
      CastQbef();
    }
  if (qex && aqex && !(_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
    {
      var manahh = _config.Item("manah").GetValue<Slider>().Value;
      var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && _q.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in minions)
            {
              if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                {
                  _q2.CastOnUnit(minion);
                }
            }
        }
    }
  /*if (qex && qexc && (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
    {
      var manahh = _config.Item("manah").GetValue<Slider>().Value;
      var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && _q.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
        {
          foreach (var minion in minions)
            {
              if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                {
                  _q2.CastOnUnit(minion);
                }
            }
        }
    }*/
  if (qnor && autoqnor && !(_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
    {
      var manahh = _config.Item("manah").GetValue<Slider>().Value;
      var t = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh)
        {
          _q.CastOnUnit(t);
        }
    }
  if (!aqex && qex)
    {
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && !(_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
        {
          var manahh = _config.Item("manah").GetValue<Slider>().Value;
          var t = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
          var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
          var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
          if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && _q.IsReady() && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.CountEnemiesInRange(_q2.Range) > 0)
            {
              foreach (var minion in minions)
                {
                  if (_q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                    {
                      _q2.CastOnUnit(minion);
                    }
                }
            }
          if (qnor)
            {
              var tq = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
              if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh)
                {
                  _q.CastOnUnit(tq);
                }
            }
        }
    }
  SwitchOptionSEX();
  SwitchOptionsnq();
  SwitchOptionseqc();
  SwitchOptionsdas();
}
#endregion
#region Q
private static void CastQ()
{
  var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  _q.CastOnUnit(qtarget);
  Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
}
#endregion
#region Qbef
private static void CastQbef()
{
  var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  _q.Cast(qtarget);
    if (_q.Cast(qtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
#region W
private static void CastW()
{
  var wtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
  _w.Cast(wtarget);
    if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
      }
}
#endregion
#region draw
private static void OnDraw(EventArgs args)
{
  var qnd = _config.Item("qnd").GetValue<Circle>();
  var qed = _config.Item("qed").GetValue<Circle>();
  if (qnd.Active)
    {
      Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, qnd.Color);
    }
  if (qed.Active)
    {
      var manahh = _config.Item("manah").GetValue<Slider>().Value;
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
      var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.Distance(ObjectManager.Player.Position) < _q2.Range)
        {
          foreach (var minion in minions)
            {
              Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, qed.Color);
            }
        }
    }
}
#endregion
#region switchoptionSEX
private static void SwitchOptionSEX()
  {
    if (!(_config.Item("qe").GetValue<bool>()))
      {
        _config.Item("aqe").SetValue(false);
      }
  }
#endregion
#region switchoptionsnq
private static void SwitchOptionsnq()
  {
    if (!(_config.Item("qn").GetValue<bool>()))
      {
        _config.Item("aqn").SetValue(false);
      }
  }
#endregion
#region switchoptionseqc
private static void SwitchOptionseqc()
  {
    if (_config.Item("e").GetValue<bool>())
      {
        _config.Item("qcaa").SetValue(false);
      }
  }
#endregion
#region switchoptionsdas
private static void SwitchOptionsdas()
  {
    if (_config.Item("qcaaa").GetValue<bool>())
      {
        _config.Item("qcaa").SetValue(false);
        _config.Item("e").SetValue(false);
      }
  }
#endregion
}
}