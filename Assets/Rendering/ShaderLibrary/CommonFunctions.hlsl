

#define CROO -0.5
#define CR01 1.5
#define CR02 -1.5
#define CR03 0.5
#define CR10 1.0
#define CR11 -2.5
#define CR12 2.0
#define CR13 -0.5
#define CR20 -0.5
#define CR21 0.0
#define CR22 0.5
#define CR23 0.0
#define CR30 0.0
#define CR31 1.0
#define CR32 0.0
#define CR33 0.0


void spline_float(float x, float k1, float k2, float k3, float k4, out float y)
{
    float cO, cl, c2, c3;
    /* Evaluate the span cubic at x using Horner’s rule. */
    c3 = CROO*k1 + CR01*k2 + CR02*k3 + CR03*k4;
    c2 = CR10*k1 + CR11*k2 + CR12*k3 + CR13*k4;
    cl = CR20*k1 + CR21*k2 + CR22*k3 + CR23*k4;
    cO = CR30*k1 + CR31*k2 + CR32*k3 + CR33*k4;
    y = ((c3*x + c2)*x + cl)*x + cO;
} 

void smallxxhash_float(float seed, float data, out float y)
{
    uint prima = 2654435761;
    uint primb = 2246822519;
    uint primc = 3266489917;
    uint primd = 668265263;
    uint prime = 374761393;

    uint accumulator = (uint)seed + prime; //seed
    uint udata = accumulator + (uint)data * primc;
    uint rotatedData = (udata << 17) |(udata >> 32 - 17);
    uint avalanche = rotatedData * primd;
    avalanche ^= avalanche >> 15;
    avalanche *= primb;
    avalanche ^= avalanche >> 13;
    avalanche *= primc;
    avalanche ^= avalanche >> 16;
    y = (float)avalanche;

}
#define M1 1597334677U     //1719413*929
#define M2 3812015801U     //140473*2467*11

void hashTong_float(float2 i, out float r)
{
    uint2 q = (uint2)i;
	q *= uint2(M1, M2);
    uint n = q.x ^ q.y;
    n = n * (n ^ (n >> 15));
    r = float(n) * (1.0/float(0xffffffffU));
}

void hashTong_half(float2 i, out float r)
{
    uint2 q = (uint2)i;
	q *= uint2(M1, M2);
    uint n = q.x ^ q.y;
    n = n * (n ^ (n >> 15));
    r = float(n) * (1.0/float(0xffffffffU));
}

void hash_float( float2 i, out float r)
{
    uint2 q = (uint2)i;
    q *= uint2(M1, M2); 
    
    uint n = (q.x ^ q.y) * M1;
    
    r = float(n) * (1.0/float(0xffffffffU));
}