using System;
using System.Drawing;
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
  private static int _lastTick;
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
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboSwitch", "Combo switch Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[]{"EQW - aggresive", "QWE - safe"})));
      _config.SubMenu("Combo").AddItem(new MenuItem("dcom", "draw combo mode").SetValue(true));
      _config.SubMenu("Combo").SubMenu("spells usage").AddItem(new MenuItem("e", "E in EQW MODE").SetValue(true));
      _config.SubMenu("Combo").SubMenu("spells usage").AddItem(new MenuItem("e2", "E in QWE MODE").SetValue(true));
      _config.SubMenu("E in QWE MODE settings").AddItem(new MenuItem("safee", "%hp to use safe E").SetValue(new Slider(25, 100, 0)));
      _config.SubMenu("E in QWE MODE settings").AddItem(new MenuItem("meleran", "distance to closest target").SetValue(new Slider(400, 700, 100)));
      _config.AddToMainMenu();
      Drawing.OnDraw += Drawing_OnDraw;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var comod = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
  var ec = _config.SubMenu("Combo").SubMenu("spells usage").Item("e").GetValue<bool>();
  var eec = _config.SubMenu("Combo").SubMenu("spells usage").Item("e2").GetValue<bool>();
  var hp = _config.SubMenu("E in QWE MODE settings").Item("safee").GetValue<Slider>().Value;
  var ran = _config.SubMenu("E in QWE MODE settings").Item("meleran").GetValue<Slider>().Value;
  var meleetarget = TargetSelector.GetTarget(ran, TargetSelector.DamageType.Physical);
  var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (comod == 0)
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
      if (comod == 1)
        {
          if (target.IsValidTarget(700))
            {
              CastQ();
            }
          if (!_q.IsReady())
            {
              Utility.DelayAction.Add(600, CastW);
            }
          if (eec && meleetarget.Distance(ObjectManager.Player.Position) < ran && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= hp)
            _e.Cast(Game.CursorPos);
        }
    }
  Switch();
}
#endregion
#region Q
private static void CastQ()
{
  var qtarget = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (_q.IsReady())
    {
      _q.CastOnUnit(qtarget);
      Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
    }
}
#endregion
#region W
private static void CastW()
{
  var wtarget = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
  if (_w.IsReady())
    {
      _w.Cast(wtarget);
        if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
          {
            Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
          }
    }
}
#endregion
#region key
private static void Switch()
{
  var empp = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
  var lasttime = Environment.TickCount - _lastTick;
    if (!_config.SubMenu("Combo").Item("ComboSwitch").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
      {
        return;
      }
    switch (empp)
      {
        case 0:
          _config.SubMenu("Combo").Item("ComboMode").SetValue(new StringList(new[]{"EQW - aggresive", "QWE - safe"}, 1));
          _lastTick = Environment.TickCount + 300;
        break;
        case 1:
          _config.SubMenu("Combo").Item("ComboMode").SetValue(new StringList(new[]{"EQW - aggresive", "QWE - safe"}));
          _lastTick = Environment.TickCount + 300;
        break;
      }
}
#endregion
#region draw
private static void Drawing_OnDraw(EventArgs args)
{
  var dc = _config.SubMenu("Combo").Item("dcom").GetValue<bool>();
  var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
  var emp = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
  if (dc)
    {
      switch (emp)
        {
          case 0:
            Drawing.DrawText(wts[0] - 30, wts[1], Color.White, "EQW - aggresive");
          break;
          case 1:
            Drawing.DrawText(wts[0] - 30, wts[1], Color.White, "QWE - safe");
          break;
        }
    }
}
#endregion
}
}