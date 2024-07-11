#include <stdio.h>
#include <stddef.h>

int add(int m, int n) {
 struct add {
   char a[m][n];
   char b;
 };

 return offsetof(struct add, b);
}


int main(void) {
   printf("%d\n", add(-12345,-65432));
   return 0;
}
