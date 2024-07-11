#include <stdio.h>
#include <stddef.h>

int add(int m, int n) {
 struct add {
   char a[n][m];
   char b;
 };

 return offsetof(struct add, a[n]) - offsetof(struct add,a[m]) * offsetof(struct add,a[0])  ;
}


int main(void) {
   printf("%d\n", add(10,2));
   return 0;
}
