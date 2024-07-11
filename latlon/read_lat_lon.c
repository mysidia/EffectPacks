// Copyright C 2024 Mysidia  All Rights Reserved

#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/file.h>
#include <arpa/inet.h>

int main(){
        FILE* fp = fopen("latlong.dat", "rb");
        unsigned short length;
	unsigned char header[2];
	int16_t latitude_raw, longitude_raw;

	if (!fp) {
                perror("fopen(latlong.dat,rb)");
                return -1;
        }
        while ( flock(fileno(fp), LOCK_EX) == -1 ) {
                perror("Error waiting for lock before reading latlong.dat");
                fclose(fp);
                return -1;
        }

        if ( fread(header, sizeof(*header), 2, fp) <  2) {
                perror("read");
		fclose(fp);
		return -1;
        }
	length = ntohs(header[0]<<8 | header[1]);
	printf("length = $%.2X\n", length);
	if (length <= 0) {
		printf("Empty list\n");
		fclose(fp);
		return 0;
	}

	unsigned char data[length*4];
	data[0] = header[0];
	data[1] = header[1];

	if ( fread(data+2, sizeof(*data), length*4, fp) < length*4 ) {
		perror("fread");
		fclose(fp);
		return -1;
	}
	fclose(fp); /* finished reading file */
	
	for(unsigned int i = 0; i < length ; i++) {
		latitude_raw =   ntohs(((u_int16_t)data[2+4*i+1]  << 8) & 0xff00
			         | ((u_int16_t)data[2+4*i+0]  <<0) & 0x00ff);
		longitude_raw =  ntohs((data[2+4*i+3]  << 8) & 0xff00
			         | ((u_int16_t)data[2+4*i+2]  <<0) & 0x00ff);
		printf("pair#%d :  lat=%Lf  lon=%Lf\n", i, (latitude_raw)/(long double)364.0,(longitude_raw)/(long double)364.0);
	}
}




