using CrowdControl.Common;
using CrowdControl.Games;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EffectPacks
{
    class DKC1: SNESEffectPack
    {
        [NotNull] private readonly IPlayer _player;

        // This requires updating

        public override Game Game { get; }
            = new Game(0, "Game name Goes Here");


        public DKC1([NotNull] IPlayer player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(responseHandler, statusUpdateHandler) => _player = player;
        public override List<Effect> Effects => new List<Effect>(new[]
        {
            new Effect("Shake", "shake", 10),
            new Effect("Take Bananas", "takebananas", 1),
            new Effect("Send extra life", "extralife", 1),
            new Effect("Darkness", "darkness", 1)
        });

        public override List<(string, Action)> MenuActions
        {
            get
            {
                List<(string, Action)> result = new List<(string, Action)>();
                return result;
            }
        }

        protected override void RequestData(DataRequest request)
        {
            Respond(request, request.Key, null, false, $"Variable name \"{request.Key}\" not known.");
            return;
        }

        protected override void StartEffect(EffectRequest request)
        {
            byte? checkByte1 = Connector.ReadByte(0x7e004b);
            byte? playState = Connector.ReadByte(0x7e0579);
            bool gameIsPaused = ((playState ?? 0x0) & 0x40) != 0;  

            if ( checkByte1 == null || playState == null || 
                 checkByte1.Equals(0x0) || gameIsPaused )
            {

                    DelayEffect(request, TimeSpan.FromSeconds(10));
                    return;
            }

            switch (request.InventoryItem.BaseItem.Code)
            {
                case "extralife":
                    {
                        // Grant an extra life;
                        // Player lives are 2-bytes starting at $0578
                        // There is another copy of the variable at $0575, so we need to 
                        // increment both locations.
                        var (success, message) = SimpleIncrement(request, 0x7e0575, 99,  "");

                        if (success != false)
                        {
                            (success, message) = SimpleIncrement(request, 0x7e0578, 99, "sent you an extra life");

                            Respond(request, success, message);
                        } else
                        {
                            Respond(request, false, "could not send extra life");
                        }
                        return;
                    }
                case "shake":
                    {
                        // Let's make the ground start shaking
                        //byte? g = Connector.ReadByte(0x009bf3);
                        //System.Windows.Forms.MessageBox.Show("[" + g.ToString() + "]");
                        var (success, message) = StartTimed(request, 0x7e1b0d, 0x3f, TimeSpan.FromMinutes(1), "started a shake");
                        success &= Connector?.WriteByte(0x7e1b0c,0xf0) ?? false;

                        Respond(request, success ?? false, message);
                        return;
                    }
                case "darkness":
                    {
                        var (success, message) = StartTimed(request, 0x7e051b, 0xff, TimeSpan.FromSeconds(28), "cast a darkness spell");
                        Respond(request, success ?? false, message);
                        return;
                    }
                case "takebananas":
                    {
                        // Steal all their bananas
                        var (success, message) = ChangeWord(request, 0x7e052b, 0, 0, 100, false, "took your bananas");

                        // Try to bring the banana counter on screen, so the player will see
                        Connector?.WriteByte(0x7e0530, 0x01);
                        Respond(request, success, message);
                        return;
                    }
            }
        }

        protected override bool StopEffect(EffectRequest request)
        {
            switch (request.InventoryItem.BaseItem.Code)
            {
                case "shake":
                    {
                        bool result = Connector?.WriteBytes(0x7e1b0c, new byte[] { 0x00, 0x00 }) ?? false;//
                        //Connector?.WriteByte(0x809bf3, 0xb) ?? false;

                        if (result)
                        {
                            Connector?.SendMessage($"{request.DisplayViewer}'s shake has ended.");
                        }
                        return result;
                    }

                case "darkness":
                    {
                        bool result = Connector?.WriteBytes(0x7e051b, 0x7f) ?? false;//
                        if (result)
                        {
                            Connector?.SendMessage($"{request.DisplayViewer}'s darkness spell has ended.");
                        }
                        return DialogResult;
                    }
            }

            return false;
        }

        public override bool StopAllEffects()
        {
            using (Connector?.OpenBatchWriteContext())
            {
                base.StopAllEffects();
                bool success = true;

                try
                {
                    //success &= Connector?.WriteByte(0x809bf3, 0x0b) ?? false;

                    success &= Connector?.WriteBytes(0x7e1b0c, new byte[] { 0x00, 0x00 }) ?? false;
                    success &= Connector?.WriteByte(0x7e051b, 0x7f) ?? false;//
                }
                catch
                {
                    success = false;
                }
                return success;
            }
        }
    }
}
