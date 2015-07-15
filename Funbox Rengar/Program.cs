using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
namespace Rengar
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
  private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
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
      if (Player.ChampionName != "Rengar")
        return;
      _q = new Spell(SpellSlot.Q, 220);
      _w = new Spell(SpellSlot.W, 400);
      _e = new Spell(SpellSlot.E, 1000);
      _e.SetSkillshot(0.3f, 75f, 1400f, true, SkillshotType.SkillshotLine);
      _e.MinHitChance = HitChance.Medium;
      _config = new Menu("Rengar", "Rengar", true);
      _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboSwitch", "Switch mode Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"})));
      _config.SubMenu("Combo").AddItem(new MenuItem("hydra", "hydra midleap").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("eq", "use E in Q mode if target out of range").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("autoheal", "%hp autoheal").SetValue(new Slider(33, 100, 0)));
      _config.AddToMainMenu();
      Drawing.OnDraw += Drawing_OnDraw;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  ComboModeSwitch();
  var searchtarget = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
  var closetarget = TargetSelector.GetTarget(350, TargetSelector.DamageType.Physical);
  var hp = _config.Item("autoheal").GetValue<Slider>().Value;
  var hml = _config.Item("hydra").GetValue<bool>();
  if ((Player.Health/Player.MaxHealth)*100 <= hp && Player.Mana == 5)
    _w.Cast();
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (searchtarget.IsValidTarget(500))
        {
          Items.UseItem(3144, searchtarget);
          Items.UseItem(3146, searchtarget);
          Items.UseItem(3153, searchtarget);
        }
      if (hml && searchtarget.IsValidTarget(350))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (!hml && searchtarget.IsValidTarget(200))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (Player.Mana <= 4)
        {
          if (searchtarget.IsValidTarget(1000) && (Player.HasBuff("rengarpassivebuff") || Player.HasBuff("rengarbushspeedbuff") || Player.HasBuff("rengarr")))
            _q.Cast();
          if (searchtarget.IsValidTarget(800) && (Player.HasBuff("rengarpassivebuff") || Player.HasBuff("rengarbushspeedbuff") || Player.HasBuff("rengarr")))
            Items.UseItem(3142);
          if (searchtarget.IsValidTarget(1000) && !Player.HasBuff("rengarpassivebuff") && !Player.HasBuff("rengarbushspeedbuff") && !Player.HasBuff("rengarr"))
            _e.Cast(searchtarget);
          if (closetarget.IsValidTarget(_q.Range))
            _q.Cast(closetarget);
          if (closetarget.IsValidTarget(350))
            _e.Cast(closetarget);
          if (closetarget.IsValidTarget(350))
            _w.Cast(closetarget);
        }
      if (Player.Mana == 5)
        {
          var comboMode = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
          var einq = _config.Item("eq").GetValue<bool>();
          switch (comboMode)
            {
              case 0:
                      {
                        if (searchtarget.IsValidTarget(1000) && (Player.HasBuff("rengarpassivebuff") || Player.HasBuff("rengarbushspeedbuff") || Player.HasBuff("rengarr")) && (Player.Health/Player.MaxHealth)*100 > hp)
                          _q.Cast();
                        if (searchtarget.IsValidTarget(800) && (Player.HasBuff("rengarpassivebuff") || Player.HasBuff("rengarbushspeedbuff") || Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (closetarget.IsValidTarget(_q.Range) && (Player.Health/Player.MaxHealth)*100 > hp)
                          _q.Cast(closetarget);
                        if (einq && searchtarget.Distance(Player.Position) > 220 && searchtarget.Distance(Player.Position) < 1000 && !Player.HasBuff("rengarpassivebuff") && !Player.HasBuff("rengarbushspeedbuff") && !Player.HasBuff("rengarr") && (Player.Health/Player.MaxHealth)*100 > hp)
                          _e.Cast(searchtarget);
                      }
              break;
              case 1:
                      {
                        if (searchtarget.IsValidTarget(800) && (Player.HasBuff("rengarpassivebuff") || Player.HasBuff("rengarbushspeedbuff") || Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (searchtarget.IsValidTarget(1000) && !Player.HasBuff("rengarpassivebuff") && !Player.HasBuff("rengarbushspeedbuff") && !Player.HasBuff("rengarr") && (Player.Health/Player.MaxHealth)*100 > hp)
                          _e.Cast(searchtarget);
                        if (closetarget.IsValidTarget(350) && (Player.Health/Player.MaxHealth)*100 > hp)
                          _e.Cast(closetarget);
                      }
              break;
            }
        }
    }
}
#endregion
#region empswitch
private static void ComboModeSwitch()
{
  var comboMode = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
  var lasttime = Environment.TickCount - _lastTick;
    if (!_config.SubMenu("Combo").Item("ComboSwitch").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
      {
        return;
      }
    switch (comboMode)
      {
        case 0:
          _config.SubMenu("Combo").Item("ComboMode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"}, 1));
          _lastTick = Environment.TickCount + 300;
        break;
        case 1:
          _config.SubMenu("Combo").Item("ComboMode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"}));
          _lastTick = Environment.TickCount + 300;
        break;
      }
}
#endregion
#region draw
private static void Drawing_OnDraw(EventArgs args)
{
  var wts = Drawing.WorldToScreen(Player.Position);
  var emp = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
      switch (emp)
        {
          case 0:
            Drawing.DrawText(wts[0], wts[1], Color.White, "Q");
          break;
          case 1:
            Drawing.DrawText(wts[0], wts[1], Color.White, "E");
          break;
        }
}
#endregion
}
}