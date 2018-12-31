           STX   $1E74
           LDX   #$0000
           LDA   #$0080
           STA   $70F2
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
           AND $70F2
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
 .left_n   AND $70F2
           ASL
           DEC Y
           BEQ left_n
.done1     DEC $1E72
           #BEQ 
           XBA
           TSB $1E70
           LSR $70F2
           INC X
           TXA
           BEQ btnfin
           BRA btn0	   
.btnfin    LDA $1E70
           STA $0500
           STA $0502
           STA $0504
           LDA $1E74
           RTL
