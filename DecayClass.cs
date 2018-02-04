using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fougerite;
using UnityEngine;
using System.IO;

namespace Decay
{
    public class DecayClass : Fougerite.Module
    {
        public override string Name { get { return "Decay"; } }
        public override string Author { get { return "Salva/Juli"; } }
        public override string Description { get { return "Decay"; } }
        public override Version Version { get { return new Version("1.0"); } }

        public string red = "[color #B40404]";
        public string blue = "[color #81F7F3]";
        public string green = "[color #82FA58]";
        public string yellow = "[color #F4FA58]";
        public string orange = "[color #FF8000]";
        public string pink = "[color #FA58F4]";
        public string white = "[color #FFFFFF]";

        public IniParser Settings;

        public bool DecayShelter = true;
        public bool DecayCampFire = true;
        public bool DeleteOnlyCampFiresOnGround = true;
        public bool DecayBarricade = true;
        

        public int TimeDecayShelter = 60;
        public int TimeDecayCampFire = 60;
        public int TimeDecayBarricade = 60;

        public override void Initialize()
        {
            Hooks.OnCommand += OnCommand;
            Hooks.OnEntityDeployedWithPlacer += OnEntityDeployed;
            ReloadConfig();
        }
        public override void DeInitialize()
        {
            Hooks.OnCommand -= OnCommand;
            Hooks.OnEntityDeployedWithPlacer -= OnEntityDeployed;
        }
        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (!player.Admin) { return; }
            if (cmd == "decay")
            {
                if (args.Length == 0)
                {
                    player.MessageFrom(Name, "/decay " + blue + " List of commands");
                    player.MessageFrom(Name, "/decay reload " + blue + " Reload and apply the Settings");
                }
                else
                {
                    if (args[0] == "reload")
                    {
                        ReloadConfig();
                        player.MessageFrom(Name, "Settings has been Reloaded :)");
                    }
                }
            }
        }
        private void ReloadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));

                Settings.AddSetting("Shelter", "DecayShelter", "true");
                Settings.AddSetting("CampFire", "DecayCampFire", "true");
                Settings.AddSetting("CampFire", "DeleteOnlyCampFiresOnGround", "true");
                Settings.AddSetting("Barricade", "DecayBarricade", "true");

                Settings.AddSetting("Shelter", "MinutesForDecayShelter", "60");
                Settings.AddSetting("CampFire", "MinutesForDecayCampFire", "60");
                Settings.AddSetting("Barricade", "MinutesForDecayBarricade", "60");
                Settings.Save();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                try
                {
                    DecayShelter = Settings.GetBoolSetting("Shelter", "DecayShelter");
                    DecayCampFire = Settings.GetBoolSetting("CampFire", "DecayCampFire");
                    DeleteOnlyCampFiresOnGround = Settings.GetBoolSetting("CampFire", "DeleteOnlyCampFiresOnGround");
                    DecayBarricade = Settings.GetBoolSetting("Barricade", "DecayBarricade");
                    

                    TimeDecayShelter = int.Parse(Settings.GetSetting("Shelter", "MinutesForDecayShelter"));
                    TimeDecayCampFire = int.Parse(Settings.GetSetting("CampFire", "MinutesForDecayCampFire"));
                    TimeDecayBarricade = int.Parse(Settings.GetSetting("Barricade", "MinutesForDecayBarricade"));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Decay Plugin: detected a problem in the configuration, please delete the Settings.ini file and restart the server ** " + ex.Message);
                }
                return;
            }
        }
        public void OnEntityDeployed(Fougerite.Player pl, Fougerite.Entity e, Fougerite.Player actualplacer)
        {
            if (e.Name == "Wood_Shelter" || e.Name == "Campfire" || e.Name == "Wood Barricade")
            {
                var dict = new Dictionary<string, object>();
                dict["entity"] = e;
                dict["player"] = actualplacer;
                if (e.Name == "Wood_Shelter" && DecayShelter)
                {
                    actualplacer.MessageFrom(Name, "This " + orange + e.Name + white + " will be removed automatically in " + TimeDecayShelter + " minutes");
                    Timer1(TimeDecayShelter * 60000, dict).Start();
                }
                else if (e.Name == "Campfire" && DecayCampFire)
                {
                    if (DeleteOnlyCampFiresOnGround)
                    {
                        var distance = World.GetWorld().GetGroundDist(e.Location);
                        var dis = Convert.ToDouble(distance);
                        if (dis < 0.20)
                        {
                            actualplacer.MessageFrom(Name, "This " + orange + e.Name + white + " will be removed automatically in " + TimeDecayCampFire + " minutes");
                            Timer1(TimeDecayCampFire * 60000, dict).Start();
                        }
                    }
                    else
                    {
                        actualplacer.MessageFrom(Name, "This " + orange + e.Name + white + " will be removed automatically in " + TimeDecayCampFire + " minutes");
                        Timer1(TimeDecayCampFire * 60000, dict).Start();
                    }

                }
                else if (e.Name == "Wood Barricade" && DecayBarricade)
                {
                    actualplacer.MessageFrom(Name, "This " + orange + e.Name + white + " will be removed automatically in " + TimeDecayBarricade + " minutes");
                    Timer1(TimeDecayBarricade * 60000, dict).Start();
                }
            }
        }
        public TimedEvent Timer1(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += CallBack;
            return timedEvent;
        }
        public void CallBack(TimedEvent e)
        {
            var dict = e.Args;
            e.Kill();
            Fougerite.Player pl = (Fougerite.Player)dict["player"];
            Fougerite.Entity ent = (Fougerite.Entity)dict["entity"];
            ent.Destroy();
            pl.Message("Your " + green + ent.Name + white + " has been removed for exceeding the maximum time allowed");
        }
    }
}
