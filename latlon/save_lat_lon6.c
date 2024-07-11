#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/file.h>
#include <arpa/inet.h>

int main(){
        unsigned short length;
	long double latitude_float,longitude_float;
	printf("How many pairs:");
        while ( scanf("%hu", &length) < 1 ) {
		printf("Try again\nHow many pairs:");
		if (feof(stdin))
		    return -1;
	}
	unsigned char data[length*8+2]; 
	int32_t latitude_converted, longitude_converted;
	data[0] = (htons(length) >>8)&0xff;
	data[1] = (htons(length)    )&0xff;
	printf("Write length=$%.2X: [%.2x %.2x] for latlong.dat\n", length, data[0], data[1]);

	for(unsigned int i = 0; i < length; i++)
	{
		printf("Enter latitude longitude (in degrees -90 to +90): ");
		while( scanf("%Lf %Lf", &latitude_float, &longitude_float) < 2 ) {
			printf("Try again\nEnter latitude longitude (in degrees -90 to +90): ");
			if (feof(stdin))
				return -1;
		}
		latitude_converted = (int32_t)(latitude_float * 11900000.0L);  // % (90*11900000);
		longitude_converted = (int32_t)(longitude_float * 11900000.0L);// % (180*11900000);

		//if (latitude_converted < -90*11900000) {
		//	latitude_converted = -( (-latitude_converted)%(90*11900000) ) ;
		//}
		//if (longitude_converted < -180*11900000) {
		//	longitude_converted = -( (-longitude_converted)%(180*11900000) );
		//}

		latitude_converted = htonl(latitude_converted);
		longitude_converted = htonl(longitude_converted);
		data[2+8*i+0] =   ( ((u_int32_t)latitude_converted)  >>0   ) & 0xff;
                data[2+8*i+1] =   ( ((u_int32_t)latitude_converted)  >>8   ) & 0xff;
                data[2+8*i+2] =   ( ((u_int32_t)latitude_converted)  >>16  ) & 0xff;
                data[2+8*i+3] =   ( ((u_int32_t)latitude_converted)  >>24  ) & 0xff;

		data[2+8*i+4] =   ( ((u_int32_t)longitude_converted) >>0   ) & 0xff;
		data[2+8*i+5] =   ( ((u_int32_t)longitude_converted) >>8   ) & 0xff;
                data[2+8*i+6] =   ( ((u_int32_t)longitude_converted) >>16  ) & 0xff;
                data[2+8*i+7] =   ( ((u_int32_t)longitude_converted) >>24  ) & 0xff;
		                printf("write raw [%.2X %.2X %.2X %.2X %.2X %.2X %.2X %.2X]: %Lf->%d %Lf->%d\n",
						data[2+8*i+0], data[2+8*i+1], data[2+8*i+2], data[2+8*i+3],
						data[2+8*i+4], data[2+8*i+5], data[2+8*i+6], data[2+8*i+7],
                                        latitude_float, latitude_converted, longitude_float, longitude_converted);
	}

        FILE* fp = fopen("latlong.dat.new", "wb");
        int error_flagged = 0;

        if (!fp) {
                perror("fopen(latlong.dat.new,wb)");
                return -1;
        }
	while ( flock(fileno(fp), LOCK_EX) == -1 ) {
		perror("Error waiting for lock on latlong.dat.new");
		fclose(fp);
		return -1;
	}


	puts("Wrote vector to latlong.dat.new");
	/*printf("Writing bytes to latlong.dat: \n");
	for(int j=0;j<length*8+2;j++) {
		printf("%.2X ", data[j]);
	}
	puts("");*/
	
	printf("{\"pairlist\" : \"");
	for(unsigned int i = 0; i < length*8+2; i++) {
	   printf("%.2X", data[i]);
	}
	printf("\"}\n");

	if ( fwrite(data, sizeof(*data), length*8+2, fp) < length*8+2) {
		perror("fwrite");
	       	error_flagged = 1; /* write failed, but file is still open*/
	}
	if ( fflush(fp) == -1 ) {
		perror("fflush() on lastlong.dat.new");
		error_flagged = 1; /* failed to flush changes to disk */
	}
	if (error_flagged == 0){
		if (rename("latlong.dat.new", "latlong.dat") == -1) {
			perror("rename failed");
			return -1;
		}
		puts("Renamed latlong.dat.new to latlong.dat");
	} else {
		(void)unlink("latlong.dat.new");
	}
	if (fclose(fp) == -1) {
		perror("fclose");
		return -1;
	}
}

//(C) Mysidia 2024
