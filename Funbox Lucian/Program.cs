using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
namespace Lucian
{
public class Program
{
private static Menu _config;
private static Orbwalking.Orbwalker _orbwalker;
private static Spell _q;
private static Spell _q2;
private static Spell _w;
private static Spell _e;
private static Spell _r;
private static int _lastTick;
private static string[] select = {"Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune","Quinn","Sivir","Teemo","Tristana","TwistedFate","Twitch","Urgot","Varus","Vayne"};
private static void Main(string[] args)
{
  CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
}
private static void Game_OnGameLoad(EventArgs args)
{
  menu();
}
private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
{
  Emodeaa();
  Emodeoff();
}
private static void OnDraw(EventArgs args)
{
  emodedrawtext();
  if (_config.Item("qmo").GetValue<bool>() && _config.Item("srdy").GetValue<bool>()) {qreadydraw();}
  if (_config.Item("qmo").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>()) {qnotreadydraw();}
  if (!_config.Item("qedl").GetValue<bool>() && _config.Item("srdy").GetValue<bool>() && _config.Item("qexmo").GetValue<bool>()) {qexreadydraw();}
  if (!_config.Item("qedl").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>() && _config.Item("qexmo").GetValue<bool>()) {qexnotreadydraw();}
  if (_config.Item("wmo").GetValue<bool>() && _config.Item("srdy").GetValue<bool>()) {wreadydraw();}
  if (_config.Item("wmo").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>()) {wnotreadydraw();}
  if (_config.Item("emo").GetValue<bool>() && _config.Item("srdy").GetValue<bool>()) {ereadydraw();}
  if (_config.Item("emo").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>()) {enotreadydraw();}
  if (_config.Item("eaamo").GetValue<bool>() && _config.Item("srdy").GetValue<bool>()) {eaareadydraw();}
  if (_config.Item("eaamo").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>()) {eaanotreadydraw();}
  if (_config.Item("rmo").GetValue<bool>() && _config.Item("srdy").GetValue<bool>()) {rreadydraw();}
  if (_config.Item("rmo").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>()) {rnotreadydraw();} 
  if (_config.Item("qedl").GetValue<bool>() && _config.Item("srdy").GetValue<bool>() && _config.Item("qexmo").GetValue<bool>()) {qexreadydrawlogic();}
  if (_config.Item("qedl").GetValue<bool>() && !_config.Item("srdy").GetValue<bool>() && _config.Item("qexmo").GetValue<bool>()) {qexnotreadydrawlogic();}
}
private static void Game_OnUpdate(EventArgs args)
{
  CastQkillsteal();
  CastQexharass();
  CastQharass();
  eswitch();
}





//USE Q, W
private static void CastQ()
{
  var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  _q.CastOnUnit(qtarget);
  Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
}
private static void CastW()
{
  var wtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
  _w.Cast(wtarget);
    if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
      {
        Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
      }
}





//KILLSTEAL
private static void CastQkillsteal()
{
  var tt = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > 30) && ((tt.Health/tt.MaxHealth)*100 <= 20) && (tt.Distance(ObjectManager.Player.Position) > 400))
        {
          if (_q.IsReady())
            {
              CastQ();
            }
        }
    }
}





//AFTER ATTACK
private static void Emodeaa()
{
  var ec = _config.Item("e").GetValue<bool>();
  var emod = _config.Item("emod").GetValue<StringList>().SelectedIndex;
  var tt = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
    {
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
        {
          if (ec)
            {
              if (_e.IsReady())
                {
                  if (emod == 0)
                    {
                      var pos = Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(), Prediction.GetPrediction(tt, 0.25f).UnitPosition.To2D(), _e.Range, Orbwalking.GetRealAutoAttackRange(tt));
                      if (pos.Count() > 0)
                        {
                          _e.Cast(pos.MinOrDefault(i => i.Distance(Game.CursorPos)));
                        }
                      else
                        {
                          _e.Cast(ObjectManager.Player.ServerPosition.Extend(tt.ServerPosition, -_e.Range));
                        }
                    }
                  else if (emod == 1)
                    {
                      _e.Cast(Game.CursorPos);
                    }
                  else if (emod == 2)
                    {
                      _e.Cast(tt.ServerPosition);
                    }
                }
              else if (_q.IsReady())
                {
                  Utility.DelayAction.Add(350, CastQ);
                }
              else if (_w.IsReady())
                {
                  Utility.DelayAction.Add(450, CastW);
                }
            }
        }
    }
}
private static void Emodeoff()
{
  var ec = _config.Item("e").GetValue<bool>();
    {
      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
        {
          if (!ec)
            {
              if (_q.IsReady())
                {
                  Utility.DelayAction.Add(350, CastQ);
                }
              else if (_w.IsReady())
                {
                  Utility.DelayAction.Add(450, CastW);
                }
            }
        }
    }
}





//Q harass
private static void CastQharass()
{
  var manahh = _config.Item("manah").GetValue<Slider>().Value;
  var t = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
    {
      if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh)
        {
          _q.CastOnUnit(t);
        }
    }
}
private static void CastQexharass()
{
  var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
  var manahh = _config.Item("manah").GetValue<Slider>().Value;
  var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
  if ((_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) || (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit) || (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
    {
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
}





//menu
private static void menu()
{
      if (ObjectManager.Player.ChampionName != "Lucian")
        return;
      _q = new Spell(SpellSlot.Q, 675);
      _q2 = new Spell(SpellSlot.Q, 1200);
      _w = new Spell(SpellSlot.W, 1000);
      _e = new Spell(SpellSlot.E, 475);
      _r = new Spell(SpellSlot.R, 1400);
      _q2.SetSkillshot(0.55f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
      _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
      _w.MinHitChance = HitChance.Low;
      _config = new Menu("Lucian", "Lucian", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Combo").AddItem(new MenuItem("e", "E combo").SetValue(false));
      _config.SubMenu("Combo").AddItem(new MenuItem("eswitch", "E mode switch Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
      _config.SubMenu("Combo").AddItem(new MenuItem("emod", "E mode").SetValue(new StringList(new[]{"Safe", "To mouse", "To target"})));
      _config.SubMenu("Harass").AddItem(new MenuItem("info1", "ON:"));
      foreach (var hero in HeroManager.Enemies)
        {
          _config.SubMenu("Harass").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName)));
        }
      _config.SubMenu("Harass").AddItem(new MenuItem("manah", "%mana").SetValue(new Slider(33, 100, 0)));
      _config.SubMenu("Draw").AddItem(new MenuItem("empd", "draw E mode text").SetValue(true));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("qmo", "Q").SetValue(false));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("qexmo", "Q Extended").SetValue(false));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("wmo", "W").SetValue(false));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("emo", "E").SetValue(false));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("eaamo", "E + AA").SetValue(false));
      _config.SubMenu("Draw").SubMenu("ON/OFF reload F5").AddItem(new MenuItem("rmo", "R").SetValue(false));
      if (_config.Item("qmo").GetValue<bool>() || _config.Item("qexmo").GetValue<bool>() || _config.Item("wmo").GetValue<bool>() || _config.Item("emo").GetValue<bool>() || _config.Item("eaamo").GetValue<bool>() || _config.Item("rmo").GetValue<bool>())
        {
          _config.SubMenu("Draw").AddItem(new MenuItem("srdy", "if spells ready to use").SetValue(false));
        }
      if (_config.Item("qmo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("Q").AddItem(new MenuItem("qnd", "ON/OFF").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("Q").AddItem(new MenuItem("qndt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      if (_config.Item("qexmo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("Q Extended").AddItem(new MenuItem("qed", "ON/OFF").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("Q Extended").AddItem(new MenuItem("qedl", "draw logic").SetValue(true));
          _config.SubMenu("Draw").SubMenu("Q Extended").AddItem(new MenuItem("qedt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      if (_config.Item("wmo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("W").AddItem(new MenuItem("wd", "ON/OFF").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("W").AddItem(new MenuItem("wdt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      if (_config.Item("emo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("E").AddItem(new MenuItem("ed", "ON/OFF").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("E").AddItem(new MenuItem("edt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      if (_config.Item("eaamo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("E+AA").AddItem(new MenuItem("ead", "ON/OFF").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("E+AA").AddItem(new MenuItem("eadt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      if (_config.Item("rmo").GetValue<bool>())
        {
          _config.SubMenu("Draw").SubMenu("R").AddItem(new MenuItem("rd", "ON/OFF").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
          _config.SubMenu("Draw").SubMenu("R").AddItem(new MenuItem("rdt", "thickness").SetValue(new Slider(10, 20, 0)));
        }
      _config.AddToMainMenu();
      Orbwalking.AfterAttack += Orbwalking_AfterAttack;
      Drawing.OnDraw += OnDraw;
      Game.OnUpdate += Game_OnUpdate;
}





//keyswitch
private static void eswitch()
{
  var emode = _config.Item("emod").GetValue<StringList>().SelectedIndex;
  var lasttime = Environment.TickCount - _lastTick;
  if (!_config.Item("eswitch").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
    {
      return;
    }
  switch (emode)
    {
      case 0:
        _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "To target"}, 1));
        _lastTick = Environment.TickCount + 300;
      break;
      case 1:
        _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "To target"}, 2));
        _lastTick = Environment.TickCount + 300;
      break;
      case 2:
        _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "To target"}));
        _lastTick = Environment.TickCount + 300;
      break;
    }
}





//Drawings
private static void emodedrawtext()
{
    var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
    var emp = _config.Item("emod").GetValue<StringList>().SelectedIndex;
    var empd = _config.Item("empd").GetValue<bool>();
    var eon = _config.Item("e").GetValue<bool>();
    if (empd && eon)
      {
        switch (emp)
          {
            case 0:
              Drawing.DrawText(wts[0] - 30, wts[1], Color.White, "Safe");
            break;
            case 1:
              Drawing.DrawText(wts[0] - 30, wts[1], Color.White, "To mouse");
            break;
            case 2:
              Drawing.DrawText(wts[0] - 30, wts[1], Color.White, "To target");
            break;
          }
      }
}
/*Draw ready spells*/
private static void qreadydraw()
{
    var qndt = _config.Item("qndt").GetValue<Slider>().Value;
    var qnd = _config.Item("qnd").GetValue<Circle>();
    if (qnd.Active)
      {
        if (_q.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, qnd.Color, qndt);
          }
      }
}
private static void qexreadydraw()
{
    var qedt = _config.Item("qedt").GetValue<Slider>().Value;
    var qed = _config.Item("qed").GetValue<Circle>();
    if (qed.Active)
      {
        if (_q2.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, qed.Color, qedt);
          }
      }
}
private static void qexreadydrawlogic()
{
    var qedt = _config.Item("qedt").GetValue<Slider>().Value;
    var qed = _config.Item("qed").GetValue<Circle>();
    if (qed.Active && _q.IsReady())
      {
        var manahh = _config.Item("manah").GetValue<Slider>().Value;
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
        var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
        if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.Distance(ObjectManager.Player.Position) < _q2.Range)
          {
            foreach (var minion in minions)
              {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, qed.Color, qedt);
              }
          }
      }
}
private static void wreadydraw()
{ 
    var wdt = _config.Item("wdt").GetValue<Slider>().Value;
    var wd = _config.Item("wd").GetValue<Circle>();
    if (wd.Active)
      {
        if (_w.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, wd.Color, wdt);
          }
      }
}
private static void ereadydraw()
{
    var edt = _config.Item("edt").GetValue<Slider>().Value;
    var ed = _config.Item("ed").GetValue<Circle>();
    if (ed.Active)
      {
        if (_e.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, ed.Color, edt);
          }
      }
}
private static void eaareadydraw()
{   
    var eadt = _config.Item("eadt").GetValue<Slider>().Value;
    var ead = _config.Item("ead").GetValue<Circle>();
    if (ead.Active)
      {
        if (_e.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range + Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), ead.Color, eadt);
          }
      }
}
private static void rreadydraw()
{   
    var rdt = _config.Item("rdt").GetValue<Slider>().Value;
    var rd = _config.Item("rd").GetValue<Circle>();
    if (rd.Active)
      {
        if (_r.IsReady())
          {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, rd.Color, rdt);
          }
      }
}
/*Draw notready spells*/
private static void qnotreadydraw()
{
    var qndt = _config.Item("qndt").GetValue<Slider>().Value;
    var qnd = _config.Item("qnd").GetValue<Circle>();
    if (qnd.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, qnd.Color, qndt);
      }
}
private static void qexnotreadydraw()
{
    var qedt = _config.Item("qedt").GetValue<Slider>().Value;
    var qed = _config.Item("qed").GetValue<Circle>();
    if (qed.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, qed.Color, qedt);
      }
}
private static void qexnotreadydrawlogic()
{
    var qedt = _config.Item("qedt").GetValue<Slider>().Value;
    var qed = _config.Item("qed").GetValue<Circle>();
    if (qed.Active)
      {
        var manahh = _config.Item("manah").GetValue<Slider>().Value;
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
        var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
        if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manahh && targetqe.Distance(ObjectManager.Player.Position) > _q.Range && targetqe.Distance(ObjectManager.Player.Position) < _q2.Range)
          {
            foreach (var minion in minions)
              {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, qed.Color, qedt);
              }
          }
      }
}
private static void wnotreadydraw()
{ 
    var wdt = _config.Item("wdt").GetValue<Slider>().Value;
    var wd = _config.Item("wd").GetValue<Circle>();
    if (wd.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, wd.Color, wdt);
      }
}
private static void enotreadydraw()
{ 
    var edt = _config.Item("edt").GetValue<Slider>().Value;
    var ed = _config.Item("ed").GetValue<Circle>();
    if (ed.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, ed.Color, edt);
      }
}
private static void eaanotreadydraw()
{   
    var eadt = _config.Item("eadt").GetValue<Slider>().Value;
    var ead = _config.Item("ead").GetValue<Circle>();
    if (ead.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range + Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), ead.Color, eadt);
      }
}
private static void rnotreadydraw()
{   
    var rdt = _config.Item("rdt").GetValue<Slider>().Value;
    var rd = _config.Item("rd").GetValue<Circle>();
    if (rd.Active)
      {
        Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, rd.Color, rdt);
      }
}





//end
}
}