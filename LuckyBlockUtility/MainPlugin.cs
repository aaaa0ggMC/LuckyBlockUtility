using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace LuckyBlockUtility
{
    [ApiVersion(2, 1)] public class MainPlugin : TerrariaPlugin {

        class PlayerDeath{
            public string name;
            public int dth;
            public PlayerDeath() {
                name = "";
                dth = 0;
            }
            public PlayerDeath(string name,int dth)
            {
                this.name = name;
                this.dth = dth;
            }
        }

        const int maxItem = 2500;
        Vector2 spawnPosition = new Vector2(-1,-1);
        List<PlayerDeath> deaths;
        float outLen = 500;

        public MainPlugin(Main game) : base(game) {
            r = new Random();
            spawnPosition = new Vector2(-1, -1);
            fight = false;
            deaths = new List<PlayerDeath>();
        } 
        public override string Name => "LuckyBlock"; 
        public override Version Version => new Version(0, 1); 
        public override string Author => "aaaa0ggmc"; 
        public override string Description => "LuckyBlock";

        void PlayerSpawn(object sender, TShockAPI.GetDataHandlers.SpawnEventArgs args)
        {
            string name = args.Player.Name;
            bool finded = false;
            foreach(PlayerDeath pd in deaths)
            {
                if(pd.name.CompareTo(name) == 0)
                {
                    pd.dth++;
                    finded = true;
                }
            }
            if (!finded)
            {
                deaths.Add(new PlayerDeath(name,1));
            }
        }
        public override void Initialize() {
            ServerApi.Hooks.ServerJoin.Register(this, OnEnterServer);
            GetDataHandlers.PlayerInfo += PlayerInfo;
            GetDataHandlers.TileEdit += TileEdit;
            GetDataHandlers.PlayerUpdate += PlayerUpdate;
            GetDataHandlers.PlayerSpawn += PlayerSpawn;

            //Adding a command is as simple as adding a new ``Command`` object to the ``ChatCommands`` list.
            //The ``Commands` object is available after including TShock in the file (`using TShockAPI;`)
            //第一个是权限，第二个是子程序，第三个是名字

            Commands.ChatCommands.Add(new Command("InitPlayer", InitPlayerCM, "itp")
            {
                HelpText = "with a player",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("GetSpawn", SetCPos, "gs")
            {
                HelpText = "with a player",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("OutLen", OutLen, "ol")
            {
                HelpText = "enter /OutLen or /ol and number to set maxium distance",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("StartFighting", StartFighting, "sf")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("EndFighting", EndFighting, "ef")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("StartGame", GameStart, "sg")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("EndGame", GameEnd, "eg")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("InitLucky", InitLucky, "il")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
            Commands.ChatCommands.Add(new Command("CentralHelp", CenteralHelp, "ch")
            {
                HelpText = "",
                Permissions = { Permissions.su }
            });
        }

        private void CenteralHelp(CommandArgs args)
        {
            args.Player.SendMessage(
            "支持的命令(所有命令需要超管及以上组才可调用)：\n"+
            "      全称          缩略                 描述                                            参数\n"+
            "   InitPlayer      itp          初始化一个玩家，给予50件随机物品                         玩家名字\n"+
            "   GetSpawn         gs 初始化中心位置（参数为初始化对象），用于判断玩家距离远近          玩家名字\n"+
            "   OutLen           ol 设置距离中心点位置最大距离，超出距离会扣血                        浮点值距离\n"+
            "   StartFighting    sf             开始幸运方块大作战\n"+
            "   EndFighting      ef             结束幸运方块大作战\n"+
            "   StartGame        sg             开始最初的游戏\n"+
            "   EndGame          eg             结束最终游戏\n"+
            "   InitLucky        il   初始化所有在线玩家，相当于InitPlayer XXX..(循环多次).\n"+
            "   CentralHelp      ch           插件中心化帮助\n"+
            "大致流程(指令):\n"+
            "   首先，一个人找到一个中心点（假设那个人名字为\"a\"），并执行/gs a初始坐标（否则StartXXX不允许调用)\n"+
            "   之后，调用/sg开始游戏（或者设置OutLen,默认500）,之后在一点时间后统一初始化玩家（给予50件随机物品）"+
            "   (PS:玩家若在初始化后加入，不要紧，输入/itp [玩家名字]单独初始化)\n"+
            "   在你想要开启幸运方块的时候输入/sf,不需要时输入/ef\n"+
            "   结束游戏使用/eg"
           , Color.Yellow);
        }

        private void InitLucky(CommandArgs args)
        {
            foreach (TSPlayer pl in TShock.Players)
            {
                if (pl != null)
                {
                    Console.WriteLine("Give " + pl.Name);
                    for (int i = 0; i < 50; ++i)
                    {
                        pl.GiveItem(r.Next(0, MainPlugin.maxItem), 1);
                    }
                }
            }
        }

        private void GameEnd(CommandArgs args)
        {
            TSPlayer.All.SendMessage("游戏结束！", Color.Yellow);
            TSPlayer.All.SendMessage("现在统计游戏时死亡计数：",Color.Blue);
            foreach (PlayerDeath pd in deaths)
            {
                TSPlayer.All.SendMessage(pd.name + ":" + pd.dth,Color.Yellow);
            }
            gameStarting = false;
        }

        private void GameStart(CommandArgs args)
        {
            if (spawnPosition.X == -1)
            {
                Console.Error.WriteLine("SpawnPositionHaven'tSetYet");
            }
            else
            {
                TSPlayer.All.SendMessage("游戏开始！", Color.Yellow);
                deaths.Clear();
                gameStarting = true;
            }
        }

        private void EndFighting(CommandArgs args)
        {
            fight = false;
            TSPlayer.All.SendMessage("幸运大赛结束！", Color.Yellow);
        }

        private void StartFighting(CommandArgs args)
        {
            if (spawnPosition.X == -1)
            {
                Console.Error.WriteLine("SpawnPositionHaven'tSetYet!");
            }
            else
            {
                fight = true;
                TSPlayer.All.SendMessage("幸运大赛开始！", Color.Yellow);
            }
        }

        private void OutLen(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                Console.Error.WriteLine("Less than 1 arguments!");
                return;
            }
            try
            {
                float v = float.Parse(args.Parameters[0]);
                outLen = v;
            }catch(Exception)
            {
                outLen = 500;
            }      
       }

        private void SetCPos(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                Console.Error.WriteLine("Less than 1 arguments!");
                return;
            }
            var found = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (found.Count != 0){
                TSPlayer pl = found[0];
                spawnPosition = new Vector2(pl.X, pl.Y);
            }
        }

        private void InitPlayerCM(CommandArgs args)
        {
            if(args.Parameters.Count < 1)
            {
                Console.Error.WriteLine("Less than 1 arguments!");
                return;
            }
            foreach (TSPlayer p in TSPlayer.FindByNameOrID(args.Parameters[0]))
            {
                if (p != null)
                {
                    Console.WriteLine("Give " + p.Name);
                    for (int i = 0; i < 50; ++i)
                    {
                        p.GiveItem(r.Next(0, MainPlugin.maxItem), 1);
                    }
                }
            }
        }

        Random r;
        bool fight = false;
        bool gameStarting = false;

        private void OnEnterServer(JoinEventArgs args)
        {
            TSPlayer p = TShockAPI.TShock.Players[args.Who];
            p.SendMessage("欢迎来到命令方块的世界！！！",Microsoft.Xna.Framework.Color.Yellow);
        }

        void TileEdit(object sender, TShockAPI.GetDataHandlers.TileEditEventArgs args)
        {

            if (fight)
            {
                if (args.Action.CompareTo(GetDataHandlers.EditAction.KillTile) == 0){ 
                    args.Player.GiveItem(r.Next(0, MainPlugin.maxItem), 1);
                }
            }
        }

        void PlayerInfo(object sender, TShockAPI.GetDataHandlers.PlayerInfoEventArgs args)
        {
            Console.WriteLine(args.Name + " just called a PlayerInfo event.");
            Console.WriteLine(args.Name + " has " + args.Hair + " hair!");
        }

        void PlayerUpdate(object sender, TShockAPI.GetDataHandlers.PlayerUpdateEventArgs args)
        {
            if (gameStarting)
            {
                Vector2 vec = new Vector2(args.Player.X - spawnPosition.X, args.Player.Y - spawnPosition.Y);
                float len = vec.Length();
                if(len >= outLen)
                {
                    args.Player.DamagePlayer(10);
                }
            }
        }

        protected override void Dispose(bool disposing) { 
            if (disposing) {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnEnterServer);
            } 
            base.Dispose(disposing);
        } 
    }
}
