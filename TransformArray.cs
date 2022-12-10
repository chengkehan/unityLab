// rotation and transform an array
// aniticlockwise 90 degree
// pixels of image stored in a flatten array, rotate pixels array to rotate image.
Vector2[] tUV2 = new Vector2[tUV.Length];
System.Array.Copy(tUV, 0, tUV2, 0, tUV.Length);
for (int pi = 0; pi < h; ++pi)
{
    for (int pj = 0; pj < w; ++pj)
    {
        Vector2 pc = tUV2[pi * w + pj];
        tUV[(w - pj - 1) * w + pi] = pc;
    }
}
System.Array.Copy(tUV, 0, tUV2, 0, tUV.Length);

// Flip horizontal
for (int pi = 0; pi < h; ++pi)
{
    for (int pj = 0; pj < w; ++pj)
    {
        Vector2 pc = tUV2[pi * w + pj];
        tUV[(w - pi - 1) * w + pj] = pc;
    }
}
