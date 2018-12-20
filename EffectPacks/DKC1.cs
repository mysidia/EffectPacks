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
    class DKC1 : SNESEffectPack
    {
        [NotNull] private readonly IPlayer _player;

        // This requires updating

        public override Game Game { get; }
            = new Game(0, "Game name Goes Here");

        private enum PadState
        {
            NORMAL,
            INVERT,
            ROTATE,
        };
        private PadState mydpadstate = PadState.NORMAL;
        private PadState myabxystate = PadState.NORMAL;

        byte[] GetControlPattern(PadState abxy, PadState dpad)
        {
            byte[] result = new byte[16];


            // Connector?.WriteBytes(0x7e70f0, new byte[] { 0x33, 0x33, 
            // 0x00, 0x00,
            // 0x00, 0x00, 
            // 0xa0, 0x2a, <- RS1
            // 0x50, 0xd5, <- LS1
            // 0x00,  0x00, <- LS2
            // 0x00, 0x00, <- RS2
            // 0x0f, 0xc0 <- CP } );

            /* ---ROM PATCH 1----
             * 0080:a28e original instruction is: txa
             * 0080:a28f original instruction is: sta 0502  
             *     change ROM c0:a28e [22 80 7f 81]    ( jsl .jpmodify 817f80  )
             *   .jpmodify at ROM location c1:7f80  (aka PC=0x817f80)
             *      TXA  ;8A
             *      STA $0502 ;8D0205
             *      LDA #$3333  ;A93333
             *      CMP $7E70F0 ;CFF0707E
             *      BNE $7FDC   ;D04F
             *      stz $1E70   ;9C701E
             *      lda $7E70F4 ;AFF4707E
             *      trb $0500   ;1C0005
             *      lda $7E70F6 ;AFF6707E
             *      and $0500   ;2D0005
             *      lsr         ;4A
             *      tsb $1E70   ;0C701E
             *      lda $7E70F8 ;AFF8707E
             *      and $0500   ;2D0005
             *      asl         ;0A
             *      tsb $1E70   ;0C701E
             *      lda $7E70FE ;AFFE701E
             *      and $0500    ;2D0005
             *      tsb $1E70   ;0c701e
             *      lda $7E70FA ;AFFA707E
             *      and $0500   ;2D0005
             *      lsr         ;4A
             *      lsr         ;4A
             *      tsb $1E70   ;00701E
             *      lda $7E70FC ;AFFC707E
             *      and $0500   ;2D0005
             *      asl         ;0A
             *      asl         ;0A
             *      tsb $1E70   ;0c701e
             *      lda $1E70   ;aD701e
             *      sta $0500   ;8D0005
             *      sta $0502   ;8d0205
             *      sta $0504   ;8d0405
             *      rtl         ;6B
             *      rtl         ;6B
             */

            if (dpad != PadState.NORMAL || abxy != PadState.NORMAL)
            {
                result[0] = 0x33; /* Tell the  ROM patch to turn on */
                result[1] = 0x33;
            }

            switch (dpad)
            {
                default: break;
                case PadState.INVERT:  result[7] |=  0x0A; result[9] |=  0x05; break;
                case PadState.ROTATE:  result[11] |= 0x0C; result[13] |= 0x03; break;
            }

            switch(abxy)
            {
                default: break;
                case PadState.INVERT:
                case PadState.ROTATE:
                    result[6]  |= 0x80;
                    result[7]  |= 0x80;
                    result[8]  |= 0x40;
                    result[9]  |= 0x40;
                    break;
            }

            result[14] = (byte)~(result[4] | result[6] | result[8] | result[10] | result[12]);
            result[15] = (byte)~(result[5] | result[7] | result[9] | result[11] | result[13]);
            Connector.SendMessage("result = " +  string.Join(" ", result.Select( hh => hh.ToString("X2"))  ) );
            return result;
        }


        public DKC1([NotNull] IPlayer player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(responseHandler, statusUpdateHandler) => _player = player;
        public override List<Effect> Effects => new List<Effect>(new[]
        {
            new Effect("Shake (1 minute)", "shake", 10),
            new Effect("Take Bananas", "takebananas", 1),
            new Effect("Send extra life", "extralife", 1),
            new Effect("Darkness (26 seconds)", "darkness", 1),
            new Effect("Lift", "lift", 1),
            new Effect("Reverse D-Pad (1 minute)", "invertdpad", 1),
            new Effect("Rotate D-Pad (1 minute)", "rotatedpad", 1),
            new Effect("Swap buttons (1 minute)", "swapbuttons", 1),
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

            if (checkByte1 == null || playState == null ||
                 checkByte1.Equals(0x0) || gameIsPaused)
            {

                DelayEffect(request, TimeSpan.FromSeconds(10));
                return;
            }

            if ( mydpadstate != PadState.NORMAL )
            {
                if (request.InventoryItem.BaseItem.Code.Equals("invertdpad") ||
                    request.InventoryItem.BaseItem.Code.Equals("reversedpad"))
                {
                    DelayEffect(request, TimeSpan.FromSeconds(5));
                    return;
                }
            }

            if (request.InventoryItem.BaseItem.Code.Equals("darkness"))
            {
                /* Make sure its not already dark */
                if ((Connector?.ReadByte(0x7e051a) ?? 1) != 0x0f)
                {
                    DelayEffect(request, TimeSpan.FromSeconds(60));
                    return;
                }
            }

            //
            // Delay effects that don't work in a swimming level
            //
            if (request.InventoryItem.BaseItem.Code.Equals("shake") ||
                request.InventoryItem.BaseItem.Code.Equals("lift"))
            {
                byte? levelByte = Connector?.ReadByte(0x7e003e);

                if (levelByte != null)
                {
                    int? entryType = Connector?.ReadWord((uint)0x3ffd60 + (uint)((levelByte ?? 0) << 1) );

                    if (entryType == null || entryType == 0x8915)
                    {
                        DelayEffect(request, TimeSpan.FromSeconds(60));
                        return;
                    }
                } else
                {
                    DelayEffect(request, TimeSpan.FromSeconds(60));
                    return;
                }
            }


            switch (request.InventoryItem.BaseItem.Code)
            {
                case "extralife":
                    {
                        // Grant an extra life;
                        // Player lives are 2-bytes starting at $0575
                        // There is another copy of the variable at $0578
                        var (success, message) = SimpleIncrement(request, 0x7e0575, 99, "sent you an extra life");

                        if (success != false)
                        {
                            byte[] mbuffer = new byte[2];

                            if (Connector?.ReadBytes(0x7e0575, mbuffer) ?? false)
                            {
                                Connector.WriteBytes(0x7e0577, mbuffer);
                            } /* else    - It seems this copy is non-essential.
                            {
                                success = false;
                                Respond(request, false, "could not send extra life");
                            }*/
                            Respond(request, success, message);
                        }
                        else
                        {
                            Respond(request, false, "could not send extra life");
                        }
                        return;
                    }
                case "lift":
                    {
                        if (Connector?.WriteBytes(0x7e0bc3, new byte[] { 0x0f, 0x04, 0x0f, 0x04 }) ?? false)
                        {
                            Respond(request, true, "gave you a lift");
                        }
                        else
                        {
                            Respond(request, true, "failed to give you a lift");
                        }
                        return;
                    }
                case "shake":
                    {
                        // Let's make the ground start shaking
                        var (success, message) = StartTimed(request, 0x7e1b0d, 0x3f, TimeSpan.FromMinutes(1), "started a shake");
                        success &= Connector?.WriteByte(0x7e1b0c, 0xf0) ?? false;

                        Respond(request, success ?? false, message);
                        return;
                    }
                case "darkness":
                    {
                        var (success, message) = StartTimed(request, 0x7e051b, 0xff, TimeSpan.FromSeconds(26), "cast a darkness spell");
                        Respond(request, success ?? false, message);
                        return;
                    }
                case "takebananas":
                    {
                        // Steal their bananas
                        // $052B is the decimal ones-digit,  $052C is the tens digit
                        var (success, message) = ChangeWord(request, 0x7e052b, 0, 0, 1, true, "took your bananas");

                        // Try to bring the banana counter on screen, so the player will see
                        Connector?.WriteByte(0x7e0530, 0x01);
                        Respond(request, success, message);
                        return;
                    }
                case "invertdpad":
                    {
                        if ( Connector?.WriteBytes(0x7e70f0,
                             GetControlPattern(myabxystate, PadState.INVERT)) ?? false )
                        {
                            mydpadstate = PadState.INVERT;
                            var (success, message) = StartTimed(request, 0x7e70f0, 0x33, TimeSpan.FromMinutes(1), "reversed your dpad");
                            Respond(request, success ?? false, message);
                        }
                        Respond(request, false, "could not invert dpad");
                        return;
                    }
                case "rotatedpad":
                    {
                        if (Connector?.WriteBytes(0x7e70f0,
                             GetControlPattern(myabxystate, PadState.ROTATE)) ?? false)
                        {
                            mydpadstate = PadState.ROTATE;
                            var (success, message) = StartTimed(request, 0x7e70f0, 0x33, TimeSpan.FromMinutes(1), "rotated your dpad");
                            Respond(request, success ?? false, message);
                        }
                        Respond(request, false, "could not rotate dpad");
                        return;
                    }
                case "swapbuttons":
                    {
                        if (Connector?.WriteBytes(0x7e70f0,
                             GetControlPattern(PadState.INVERT, mydpadstate)) ?? false)
                        {
                            myabxystate = PadState.INVERT;
                            var (success, message) = StartTimed(request, 0x7e70f0, 0x33, TimeSpan.FromMinutes(1), "swapped the buttons around");
                            Respond(request, success ?? false, message);
                        }
                        Respond(request, false, "could not swap the buttons around");
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
                        bool result = Connector?.WriteByte(0x7e051b, 0x7f) ?? false;//
                        if (result)
                        {
                            Connector?.SendMessage($"{request.DisplayViewer}'s darkness spell has ended.");
                        }
                        return result;
                    }

                case "invertdpad":
                case "rotatedpad":
                    {
                        if (Connector?.WriteBytes(0x7e70f0,
                             GetControlPattern(myabxystate, PadState.NORMAL)) ?? false)
                        {
                            mydpadstate = PadState.NORMAL;
                            Connector?.SendMessage($"{request.DisplayViewer}'s dpad affect has ended.");
                            return true;
                        }
                        return false;
                    }
                case "swapbuttons":
                    {
                        if (Connector?.WriteBytes(0x7e70f0,
                             GetControlPattern(PadState.NORMAL, mydpadstate)) ?? false)
                        {
                            myabxystate = PadState.NORMAL;
                            Connector?.SendMessage($"{request.DisplayViewer}'s button swap has ended.");
                            return true;
                        }
                        return false;

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
                    success &= Connector?.WriteByte(0x7e70f0, 0x00) ?? false;
                    mydpadstate = PadState.NORMAL;
                    myabxystate = PadState.NORMAL;
                }
                catch
                {
                    success = false;
                }
                return success;
            }
        }

/*        private enum LevelType
        {
            Outdoor = 1,
            Indoor = 2,   // Indoor, certain affects might be useless
            Water = 3,    // Water,  certain affects don't work
	    Bonus = 4,   
            Unknown = 5
        };

        LevelType GetLevelType(int levelid)
        {
            byte[] levelTypeMap = new byte[]
            {
                LevelType.Outdoor, LevelType.Outdoor,  LevelType.Bonus,   LevelType.Indoor,  // 00 - 03 
                LevelType.Bonus,   LevelType.Bonus,    LevelType.Bonus,   LevelType.Indoor, // 04 - 07
                LevelType.Outdoor, LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor, // 08 - 0b
                LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Outdoor, // 0c - 0f
                LevelType.Indoor,  LevelType.Bonus,    LevelType.Indoor, LevelType.Indoor,  // 10 - 13
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Outdoor, LevelType.Outdoor, // 14 - 17
                LevelType.Indoor,  LevelType.Outdoor,  LevelType.Bonus,   LevelType.Bonus,   // 18 - 1B
                LevelType.Bonus,   LevelType.Outdoor,  LevelType.Bonus,   LevelType.Bonus,   // 1C - 1F
                LevelType.Bonus,   LevelType.Indoor,   LevelType.Water,   LevelType.Indoor,  // 20 - 23
                LevelType.Outdoor, LevelType.Outdoor,  LevelType.Indoor,  LevelType.Indoor,  // 24 - 27
                LevelType.Outdoor, LevelType.Indoor,   LevelType.Water,   LevelType.Indoor,  // 28 - 2B
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 2C - 2F
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Bonus,   LevelType.Bonus,   // 30 - 33
                LevelType.Indoor,  LevelType.Outdoor,  LevelType.Indoor,  LevelType.Indoor,  // 34 - 37
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 38 - 3B
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Water,   LevelType.Water,   // 3C - 3F
                LevelType.Indoor, LevelType.Indoor,   LevelType.Outdoor, LevelType.Outdoor, // 40 - 43
                LevelType.Indoor,  LevelType.Indoor,   LevelType.Bonus,   LevelType.Indoor,  // 44 - 47
                LevelType.Bonus,   LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 48 - 4B
                LevelType.Bonus,   LevelType.Bonus,    LevelType.Bonus,   LevelType.Bonus,   // 4C - 4F
                LevelType.Bonus,   LevelType.Indoor,   LevelType.Bonus,   LevelType.Bonus,   // 50 - 53
		LevelType.Outdoor, LevelType.Bonus,    LevelType.Bonus,   LevelType.Bonus,   // 54 - 57
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Indoor,  LevelType.Indoor,  // 58 - 5B
		LevelType.Outdoor, LevelType.Indoor,   LevelType.Unknown, LevelType.Outdoor, // 5C - 5F
		LevelType.Bonus,   LevelType.Bonus,    LevelType.Water,   LevelType.Bonus, // 60 - 63
		LevelType.Indoor,  LevelType.Outdoor,  LevelType.Bonus,   LevelType.Bonus, // 64 - 67
		LevelType.Outdoor //boss 
		, LevelType.Bonus,    LevelType.Bonus,   LevelType.Bonus, // 68 - 6B		
		LevelType.Bonus,   LevelType.Water,    LevelType.Outdoor, LevelType.Outdoor, // 6C - 6F
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Outdoor, // 70 - 73
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Outdoor, // 74 - 77
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor, LevelType.Indoor, // 78 - 7B
		LevelType.Indoor, LevelType.Indoor,  LevelType.Indoor, LevelType.Indoor, // 7C - 7F
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 80 - 83
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 84 - 87
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 88 - 8B
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 8C - 8F
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 90 - 93
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Bonus,   // 94 - 97
		LevelType.Bonus,   LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 98 - 9B
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // 9C - 9F
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // A0 - A3
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Bonus,   LevelType.Outdoor,  // A4 - A7
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Outdoor,  // A8 - AB
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Outdoor,  // AC - AF
                LevelType.Outdoor, LevelType.Outdoor,  LevelType.Outdoor, LevelType.Bonus,    // B0 - B3
		LevelType.Bonus,   LevelType.Bonus,    LevelType.Outdoor, LevelType.Bonus,    // B4 - B7
		LevelType.Bonus,   LevelType.Bonus,    LevelType.Outdoor, LevelType.Outdoor, // B8 - BB
		LevelType.Outdoor, LevelType.Indoor,   LevelType.Bonus,   LevelType.Water,   // BC - BF
		LevelType.Water,   LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // C0 - C3
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // C4 - C7
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Indoor,  LevelType.Indoor,  // C8 - CB
		LevelType.Indoor,  LevelType.Indoor,   LevelType.Outdoor,  LevelType.Outdoor,  // CC - CF		
		LevelType.Outdoor, LevelType.Outdoor,  LevelType.Bonus,    LevelType.Bonus,    // D0 - D3
		LevelType.Bonus,   LevelType.Bonus,    LevelType.Outdoor,  LevelType.Bonus,    // D4 - D7
		LevelType.Outdoor, LevelType.Indoor,   LevelType.Indoor,   LevelType.Outdoor,  // D8 - DB
		LevelType.Bonus,   LevelType.Outdoor,  LevelType.Water,    LevelType.Water,    // DC - DF
		LevelType.Boss,    LevelType.Boss,     LevelType.Boss,     LevelType.Boss, // E0 - E3
		LevelType.Boss,    LevelType.Unknown,  LevelType.Unknown,  LevelType.Unknown, // E4 - E7
		
            };
            
            try {
                 return levelTypeMap[levelid];
            } catch {
                 return LevelType.Unknown;
            }
        }*/

    }
}
/*
		
		
*/
