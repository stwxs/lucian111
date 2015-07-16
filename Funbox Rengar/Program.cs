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
      if (ObjectManager.Player.ChampionName != "Rengar")
        return;
      _q = new Spell(SpellSlot.Q, 230);
      _w = new Spell(SpellSlot.W, 400);
      _e = new Spell(SpellSlot.E, 1000);
      _e.SetSkillshot(0.3f, 75f, 1400f, true, SkillshotType.SkillshotLine);
      _e.MinHitChance = HitChance.Medium;
      _config = new Menu("Rengar", "Rengar", true);
      _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      xSLxOrbwalker.AddToMenu(_config.SubMenu("Orbwalking"));
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      _config.AddSubMenu(targetSelectorMenu);
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboSwitch", "Switch mode Key").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
      _config.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"})));
      _config.SubMenu("Combo").AddItem(new MenuItem("hydra", "hydra midleap").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("melorb", "ONESHOT").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("eq", "use E in Q mode if target out of range").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("twotar", "2 targets selectors - search and close target").SetValue(true));
      _config.SubMenu("Combo").AddItem(new MenuItem("autoheal", "%hp autoheal").SetValue(new Slider(33, 100, 0)));
      _config.AddToMainMenu();
      Drawing.OnDraw += Drawing_OnDraw;
      Obj_AI_Base.OnProcessSpellCast += oncast;
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region resetQ
private static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
{
  var spell = args.SData;
  if (!sender.IsMe)
  return;
  if (spell.Name.ToLower().Contains("rengarq"))
    {
      Orbwalking.ResetAutoAttackTimer();
    }
}
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  ComboModeSwitch();
  var hp = _config.Item("autoheal").GetValue<Slider>().Value;
  var hml = _config.Item("hydra").GetValue<bool>();
  var mel = _config.Item("melorb").GetValue<bool>();
  var twot = _config.Item("twotar").GetValue<bool>();

  if (!twot)
  {
  var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
  if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= hp && ObjectManager.Player.Mana == 5)
    _w.Cast();
  if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
    {
      if (target.IsValidTarget(500))
        {
          Items.UseItem(3144, target);
          Items.UseItem(3146, target);
          Items.UseItem(3153, target);
        }
      if (hml && target.IsValidTarget(350))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (!hml && target.IsValidTarget(200))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (ObjectManager.Player.Mana <= 4)
        {
          if (target.IsValidTarget(1000) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
            _q.Cast();
          if (target.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
            Items.UseItem(3142);
          if (target.IsValidTarget(1000) && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr"))
            _e.Cast(target);
          if (mel && target.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
            {
              ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
          if (target.IsValidTarget(_q.Range))
            _q.Cast(target);
          if (target.IsValidTarget(350))
            {
              _e.Cast(target);
              _w.Cast(target);
                if (mel && target.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                  {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                  }
            }
        }
      if (ObjectManager.Player.Mana == 5)
        {
          var comboMode = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
          var einq = _config.Item("eq").GetValue<bool>();
          switch (comboMode)
            {
              case 0:
                      {
                        if (target.IsValidTarget(1000) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _q.Cast();
                        if (target.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (target.IsValidTarget(_q.Range) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _q.Cast(target);
                        if (einq && target.Distance(ObjectManager.Player.Position) > 250 && target.Distance(ObjectManager.Player.Position) < 1000 && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr") && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(target);
                        if (mel && target.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                          {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                          }
                      }
              break;
              case 1:
                      {
                        if (target.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (target.IsValidTarget(1000) && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr") && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(target);
                        if (target.IsValidTarget(350) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(target);
                        if (mel && target.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                          {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                          }
                      }
              break;
            }
        }
    }
    }
if (twot)
  {
  var searchtarget = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
  var closetarget = TargetSelector.GetTarget(350, TargetSelector.DamageType.Physical);
  if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= hp && ObjectManager.Player.Mana == 5)
    _w.Cast();
  if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
    {
      if (searchtarget.IsValidTarget(500))
        {
          Items.UseItem(3144, searchtarget);
          Items.UseItem(3146, searchtarget);
          Items.UseItem(3153, searchtarget);
        }
      if (hml && closetarget.IsValidTarget(350))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (!hml && closetarget.IsValidTarget(200))
        {
          Items.UseItem(3074);
          Items.UseItem(3077);
          Items.UseItem(3143);
        }
      if (ObjectManager.Player.Mana <= 4)
        {
          if (searchtarget.IsValidTarget(1000) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
            _q.Cast();
          if (searchtarget.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
            Items.UseItem(3142);
          if (searchtarget.IsValidTarget(1000) && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr"))
            _e.Cast(searchtarget);
          if (mel && closetarget.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
            {
              ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, closetarget);
            }
          if (closetarget.IsValidTarget(_q.Range))
            _q.Cast(closetarget);
          if (closetarget.IsValidTarget(350))
            {
              _e.Cast(closetarget);
              _w.Cast(closetarget);
                if (mel && closetarget.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                  {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, closetarget);
                  }
            }
        }
      if (ObjectManager.Player.Mana == 5)
        {
          var comboMode = _config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
          var einq = _config.Item("eq").GetValue<bool>();
          switch (comboMode)
            {
              case 0:
                      {
                        if (searchtarget.IsValidTarget(1000) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _q.Cast();
                        if (searchtarget.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (closetarget.IsValidTarget(_q.Range) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _q.Cast(closetarget);
                        if (einq && searchtarget.Distance(ObjectManager.Player.Position) > 250 && searchtarget.Distance(ObjectManager.Player.Position) < 1000 && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr") && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(searchtarget);
                        if (mel && closetarget.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                          {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, closetarget);
                          }
                      }
              break;
              case 1:
                      {
                        if (searchtarget.IsValidTarget(800) && (ObjectManager.Player.HasBuff("rengarpassivebuff") || ObjectManager.Player.HasBuff("rengarbushspeedbuff") || ObjectManager.Player.HasBuff("rengarr")))
                          Items.UseItem(3142);
                        if (searchtarget.IsValidTarget(1000) && !ObjectManager.Player.HasBuff("rengarpassivebuff") && !ObjectManager.Player.HasBuff("rengarbushspeedbuff") && !ObjectManager.Player.HasBuff("rengarr") && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(searchtarget);
                        if (closetarget.IsValidTarget(350) && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > hp)
                          _e.Cast(closetarget);
                        if (mel && closetarget.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                          {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, closetarget);
                          }
                      }
              break;
            }
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
  var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
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