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

        public DKC1([NotNull] IPlayer player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(responseHandler, statusUpdateHandler) => _player = player;
        public override List<Effect> Effects => new List<Effect>(new[]
        {
            new Effect("Shake", "shake", 10),
            new Effect("Take Bananas", "takebananas", 1),
            new Effect("Send extra life", "extralife", 1)
        });

        public override List<(string, Action)> MenuActions
        {
            get
            {
                List<(string, Action)> result = new List<(string, Action)>();
                return result;
            }
        }

        public override Game Game { get; } = new Game(0, "D");

        protected override void RequestData(DataRequest request)
        {
            Respond(request, request.Key, null, false, $"Variable name \"{request.Key}\" not known.");
            return;
        }

        protected override void StartEffect(EffectRequest request)
        {
            /*switch(Connector.ReadByte?(0x7e003e))
            {
                default:
                    DelayEffect(request, TimeSpan.FromSeconds(10));
                    return;
            }*/
            switch (request.InventoryItem.BaseItem.Code)
            {
                case "extralife":
                    {
                        var (success, message) = SimpleIncrement(request, 0x7e0575, 99,  "");

                        if (success ?? false)
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
                        //byte? g = Connector.ReadByte(0x009bf3);
                        //System.Windows.Forms.MessageBox.Show("[" + g.ToString() + "]");
                        var (success, message) = StartTimed(request, 0x7e1b0d, 0x3f, TimeSpan.FromMinutes(1), "started a shake");
                        success &= Connector?.WriteByte(0x7e1b0c,0xf0) ?? false;

                        Respond(request, success ?? false);
                        return;
                    }
                case "takebananas":
                    {
                        var (success, message) = ChangeWord(request, 0x7e052b, 0, 0, 100, false, "took your bananas");
                        Connector?.WriteByte(0x7e052f, 0xff); /* Bring the counter on-screen */
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

                        if ( result )
                        {
                            Connector?.SendMessage($"{request.DisplayViewer}'s shake has ended.");
                        }
                        return result;
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
                    success &= Connector?.WriteBytes(0x7e1b0c, new byte[] { 0x00, 0x00 }) ?? false;//
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
