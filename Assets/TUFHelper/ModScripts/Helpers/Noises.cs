using System.Runtime.CompilerServices;
using UnityEngine;

namespace TUFHelper.Utils
{
    public static class Noises
    {
        // Skewing and unskewing factors for 3D Simplex noise
        private const float F3 = 1.0f / 3.0f;
        private const float G3 = 1.0f / 6.0f;

        // Permutation table (0-255 duplicated to avoid modulo operations)
        private static readonly byte[] Perm = new byte[512]
        {
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
            8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
            35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,
            134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
            55,46,245,40,244,102,143,54, 65,25,63,161,1,216,80,73,209,76,132,187,208,89,
            18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,
            250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,
            189,28,42,223,183,170,213,119,248,152, 2,44,154,163,70,221,153,101,155,167,
            43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185, 112,104,218,246,
            97,228,251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,
            107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
            
            // Duplicated permutation for lookup overflow wrapping
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
            8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
            35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,
            134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
            55,46,245,40,244,102,143,54, 65,25,63,161,1,216,80,73,209,76,132,187,208,89,
            18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,
            250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,
            189,28,42,223,183,170,213,119,248,152, 2,44,154,163,70,221,153,101,155,167,
            43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185, 112,104,218,246,
            97,228,251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,
            107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        // 12 Gradient directions for 3D Simplex
        private static readonly float[] GradX = { 1, -1, 1, -1, 1, -1, 1, -1, 0, 0, 0, 0 };
        private static readonly float[] GradY = { 1, 1, -1, -1, 0, 0, 0, 0, 1, -1, 1, -1 };
        private static readonly float[] GradZ = { 0, 0, 0, 0, 1, 1, -1, -1, 1, 1, -1, -1 };

        /// <summary>
        /// Generates 3D Simplex noise value between -1.0 and 1.0.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Simplex3D(float x, float y, float z)
        {
            // Skew input space to determine simplex cell origin
            float s = (x + y + z) * F3;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            int k = FastFloor(z + s);

            // Unskew cell origin back to (x, y, z) space
            float t = (i + j + k) * G3;
            float X0 = i - t;
            float Y0 = j - t;
            float Z0 = k - t;

            // Unskewed distance from cell origin
            float x0 = x - X0;
            float y0 = y - Y0;
            float z0 = z - Z0;

            // Determine which simplex traversal order (1 of 6 paths)
            int i1, j1, k1;
            int i2, j2, k2;

            if (x0 >= y0)
            {
                if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y
            }
            else
            {
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z
            }

            // Offsets for 4 corners of the simplex in unskewed coordinates
            float x1 = x0 - i1 + G3;
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;

            float x2 = x0 - i2 + 2.0f * G3;
            float y2 = y0 - j2 + 2.0f * G3;
            float z2 = z0 - k2 + 2.0f * G3;

            float x3 = x0 - 1.0f + 3.0f * G3;
            float y3 = y0 - 1.0f + 3.0f * G3;
            float z3 = z0 - 1.0f + 3.0f * G3;

            // Wrap coordinates to perm table indices
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;

            // Calculate contribution from corner 0
            float n0 = 0.0f;
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 > 0.0f)
            {
                t0 *= t0;
                int gi0 = Perm[ii + Perm[jj + Perm[kk]]] % 12;
                n0 = t0 * t0 * (GradX[gi0] * x0 + GradY[gi0] * y0 + GradZ[gi0] * z0);
            }

            // Calculate contribution from corner 1
            float n1 = 0.0f;
            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 > 0.0f)
            {
                t1 *= t1;
                int gi1 = Perm[ii + i1 + Perm[jj + j1 + Perm[kk + k1]]] % 12;
                n1 = t1 * t1 * (GradX[gi1] * x1 + GradY[gi1] * y1 + GradZ[gi1] * z1);
            }

            // Calculate contribution from corner 2
            float n2 = 0.0f;
            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 > 0.0f)
            {
                t2 *= t2;
                int gi2 = Perm[ii + i2 + Perm[jj + j2 + Perm[kk + k2]]] % 12;
                n2 = t2 * t2 * (GradX[gi2] * x2 + GradY[gi2] * y2 + GradZ[gi2] * z2);
            }

            // Calculate contribution from corner 3
            float n3 = 0.0f;
            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 > 0.0f)
            {
                t3 *= t3;
                int gi3 = Perm[ii + 1 + Perm[jj + 1 + Perm[kk + 1]]] % 12;
                n3 = t3 * t3 * (GradX[gi3] * x3 + GradY[gi3] * y3 + GradZ[gi3] * z3);
            }

            // Normalize result to [-1, 1] range (factor 32.0f scales max peak values)
            return 32.0f * (n0 + n1 + n2 + n3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Simplex3D(Vector3 position)
        {
            return Simplex3D(position.x, position.y, position.z);
        }
        public static float Fbm3D(Vector3 position, int octaves = 4, float lacunarity = 2.0f, float gain = 0.5f)
        {
            float total = 0.0f;
            float frequency = 1.0f;
            float amplitude = 1.0f;
            float maxValue = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                total += Simplex3D(position * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }

            return total / maxValue; // Returns normalized [-1.0, 1.0]
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastFloor(float x)
        {
            int xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }
    }
}