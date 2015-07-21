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
      _config.AddItem(new MenuItem("e2", "E safe mode").SetValue(true));
      _config.AddItem(new MenuItem("diste", "E safe mode - distance to closest enemy").SetValue(new Slider(400, 700, 0)));
      _config.AddItem(new MenuItem("hptoe", "E safe mode - %hp").SetValue(new Slider(25, 100, 0)));
      _config.AddItem(new MenuItem("delay", "Delay before spell").SetValue(new Slider(430, 1000, 0)));
      _config.AddItem(new MenuItem("delay2", "Delay after spell").SetValue(new Slider(350, 1000, 0)));
      _config.AddToMainMenu();
      Game.OnUpdate += Game_OnUpdate;
    }
#endregion
#region OnGameUpdate
private static void Game_OnUpdate(EventArgs args)
{
  var ec = _config.Item("e").GetValue<bool>();
  var ecs = _config.Item("e2").GetValue<bool>();
  var dis = _config.Item("diste").GetValue<Slider>().Value;
  var hp = _config.Item("hptoe").GetValue<Slider>().Value;
  var del = _config.Item("delay").GetValue<Slider>().Value;
  var meleetarget = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
  var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
      if (_e.IsReady())
        {
          CastQ();
          if (!_q.IsReady())
            {
              Utility.DelayAction.Add(del, CastW);
            }
        }
      else
        {
          Utility.DelayAction.Add(del, CastQ);
          if (!_q.IsReady())
            {
              Utility.DelayAction.Add(del*2, CastW);
            }
        }
      if (ec)
        {
          if (!ecs)
            {
              if (target.IsValidTarget(900))
                {
                  _e.Cast(Game.CursorPos);
                }
            }
          else
            {
              if (meleetarget.Distance(ObjectManager.Player.Position) < dis && (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= hp)
                _e.Cast(Game.CursorPos);
            }
        }
    }
}
#endregion
#region Q
private static void CastQ()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var qtarget = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
  if (_q.IsReady())
    {
      _q.CastOnUnit(qtarget);
      Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
    }
}
#endregion
#region W
private static void CastW()
{
  var dell = _config.Item("delay2").GetValue<Slider>().Value;
  var wtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
  if (_w.IsReady())
    {
      _w.Cast(wtarget);
        if (_w.Cast(wtarget) == Spell.CastStates.SuccessfullyCasted)
          {
            Utility.DelayAction.Add(dell, Orbwalking.ResetAutoAttackTimer);
          }
    }
}
#endregion
}
}