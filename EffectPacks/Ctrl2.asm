; 65c816 opcodes reference - http://www.6502.org/tutorials/65c816opcodes.html#1
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

           STX   $1E74
           STA $0502 ;8D0205
           LDA #$3333  ;A93333
           CMP $7E70F0 ;CFF0707E
           BNE btnskip   ;D0xx
           STZ  $1E70
           LDX   #$0000
           LDA   #$0080
           STA   $1E76
.btn0      LDA $70F4, X
           STZ   $1E72
           BIT   #$08
           BNE   nobytesw
           INC   $1E72
.nobytesw  BIT #$0010
           BEQ nsleft
.nsright   LDA #$0010
           BIT #$0007
           TAY           
           CMP #$0000
           BEQ done1
           LDA $0500
           AND $1E76
 .right_n  LSR
           DEC Y
           BEQ right_n
           BRA done1
.nsleft    LDA $70F4, X
           BIT #$0007
           TAY
           CMP #$0000
           BEQ done1
           LDA $0500
 .left_n   AND $1E76
           ASL
           DEC Y
           BEQ left_n
.done1     DEC $1E72
           #BEQ 
           XBA
           TSB $1E70
           LSR $1E76
           INC X
           TXA
           BEQ btnfin
           BRA btn0	   
.btnfin    LDA $1E70
           STA $0500
           STA $0502
           STA $0504
           LDA $1E74
.btnskip   RTL
