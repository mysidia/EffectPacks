; Notes( 65c816 opcodes reference - http://www.6502.org/tutorials/65c816opcodes.html#1 )
;
; Copyright 2018-2019 Mysidia
; The following is Licensed under the Apache License, Version 2.0 (the "License");
; you may not use this file except in compliance with the License.
; You may obtain a copy of the License at
; http://www.apache.org/licenses/LICENSE-2.0
; Unless required by applicable law or agreed to in writing, software
; distributed under the License is distributed on an "AS IS" BASIS,
; WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
; See the License for the specific language governing permissions and
; limitations under the License.
;
; // BV_COUNTER_BIT0 = 0x01
; // BV_COUNTER_BIT1 = 0x02
; // BV_COUNTER_BIT2 = 0x04
; // BV_SHIFTRIGHT   = 0x00
; // BV_SHIFTLEFT    = (1 << 4);
; // BV_BYTESWAP     = (1 << 3);
; result[0] = BV_MAGIC_1;     /* Enablement byte0 */
; result[1] = BV_MAGIC_2;     /* Enablement byte1 */
; if (abxy != PadState.NORMAL || dpad != PadState.NORMAL) {
;     /* Turn off Select button while alternate controls in effect */
;     result[10] = BV_DISABLE;                       /* Select button */
; }
; result[11] = 0;                                /* Start button */
;
; if  (abxy == PadState.INVERT) {  // Switched Button Controls
;      /* Swap A and X buttons */
;      result[4] = (BV_SHIFTRIGHT | BV_COUNTER_BIT0); /* A button */
;      result[5] = (BV_SHIFTLEFT  | BV_COUNTER_BIT0); /* X button */
;
;      /* Swap L and R buttons */
;      result[6] = (BV_SHIFTRIGHT | BV_COUNTER_BIT0); /* L button */
;      result[7] = (BV_SHIFTLEFT  | BV_COUNTER_BIT0); /* R button */

;      /* Swap B and Y buttons */
;      result[8] = (BV_SHIFTRIGHT | BV_COUNTER_BIT0); /* B button */
;      result[9] = (BV_SHIFTLEFT  | BV_COUNTER_BIT0); /* Y button */
; }
;
; if  (dpad == PadState.INVERT) {  // Simple inversion
;     /* Reverse both Dpad directions */
;     result[12] = (BV_SHIFTRIGHT | BV_COUNTER_BIT0); /* Dpad Down */
;     result[13] = (BV_SHIFTLEFT  | BV_COUNTER_BIT0); /* Dpad Up */
;     result[14] = (BV_SHIFTRIGHT | BV_COUNTER_BIT0); /* Dpad Left */
;     result[15] = (BV_SHIFTLEFT  | BV_COUNTER_BIT0); /* Dpad Right */
; }
; Connector?.WriteMemory(0x7e0000 + 0x70f0, result);

           STX $1E74      ;8E7410
           STA $0502      ;8D0205
           LDA #$3333     ;A93333
           CMP $7E70F0    ;CFF0707E
           BNE btnskip    ;D0xx
           STZ $1E70      ;9C701E
           LDX #$0000     ;A90000
           LDA #$0080     ;A98000
           STA $1E76      ;8D761E
.btn0      LDA $7E70F4, X ;BFF4707E
           STZ $1E72      ;9C721E
           BIT #$08       ;890800
           BNE nobytesw   ;D0xxxx
           INC $1E72      ;EE721E
.nobytesw  BIT #$0010     ;891000
           BEQ nsleft     ;F0xxxx
.nsright   LDA #$0010     ;A91000
           BIT #$0007     ;890700
           TAY            ;A8
           CMP #$0000     ;C90000
           BEQ done1      ;F0xxxx
           LDA $0500      ;AD0005
           AND $1E76      ;2D761E
 .right_n  LSR            ;4A
           DEC Y          ;88
           BEQ right_n    ;F0xxxx
           BRA done1      ;80xxxx
.nsleft    LDA $7E70F4, X ;BFF4707E
           BIT #$0007     ;890700
           TAY            ;A8
           CMP #$0000     ;C90000
           BEQ done1      ;F0xxxx
           LDA $0500      ;A90005
           AND $1E76      ;2D761E
 .left_n   ASL            ;0A
           DEC Y          ;88
           BEQ left_n     ;F0xxxx
.done1     DEC $1E72      ;CE721E
           #BEQ           ;F0xxxx
           XBA            ;EB
           TSB $1E70      ;0C701E
           LSR $1E76      ;4E761E
           INC X          ;E8
           TXA            ;8A
           BEQ btnfin     ;F0xxxx
           BRA btn0	    ;80xxxx
.btnfin    LDA $1E70      ;AD701E
           STA $0500      ;8D0005
           STA $0502      ;8D0205
           STA $0504      ;8D0504
           LDA $1E74      ;AD741E
.btnskip   RTL            ;6B
