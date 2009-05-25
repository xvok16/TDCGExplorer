/* program.c */
/* vim: set shiftwidth=4 cindent : */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "tdcg.h"
#include "tsofile.h"

int main(int argc, char *argv[])
{
    FILE *file;

    if (argc != 2)
    {
	printf("Usage: matline <tso file>\n");
	return 1;
    }
    char* filename = argv[1];
    puts(filename);

    file = fopen(filename, "rb");
    if (!file)
    {
	return 1;
    }
    int magic;
    magic = read_int(file);
    printf("magic %08x\n", magic);
    if (magic != (int)(('T'<<8*0)+('S'<<8*1)+('O'<<8*2)+('1'<<8*3)))
    {
	fclose(file);
	return 1;
    }

    Tsofile *tso = create_tso();
    tso_read(tso, file);

    int i, j;
    for (i=0; i<tso->nmaterials; i++)
    for (j=0; j<tso->materials[i]->nlines; j++)
    {
	Param *param = create_param();
	param_read(param, tso->materials[i]->lines[j]);
	param_dump(param);
	free_param(param);
    }
    free_tso(tso);
    tso = NULL;

    fclose(file);
    return 0;
}
