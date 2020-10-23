// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"

extern "C" _declspec(dllexport) float BaseFunc_RE(int i, int k, float u, float knot[]);
extern "C" _declspec(dllexport) float BaseFunc(int i, int k, float u, float knot[], float uArray[], int tArray[]);


float div1, div2, U1, U2;

int rk, ri, k_2;
inline int index(int const k, int const it);

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

float BaseFunc_RE(int i, int  k, float u, float knot[])
{
	if (k == 0)
	{
		return (u >= knot[i] && u < knot[i + 1]) ? 1.0f : 0.0f;
	}

	float div1 = knot[i + k] - knot[i];
	float div2 = knot[i + k + 1] - knot[i + 1];

	float U1 = (abs(div1) < 1e-4) ? 1.0f : (u - knot[i]) / div1;
	float U2 = (abs(div2) < 1e-4) ? 1.0f : (knot[i + k + 1] - u) / div2;

	return U1 * BaseFunc_RE(i, k - 1, u, knot) + U2 * BaseFunc_RE(i + 1, k - 1, u, knot);
}

float BaseFunc(int i, int k, float u, float knot[], float uArray[], int tArray[])
{
	k_2 = (int)pow(2, k);
	rk = 0;
	for (int it = 0; it < k_2; it += 1)
	{
		ri = tArray[index(k, it)];
		uArray[it] = (u >= knot[i + ri]
			&& u < knot[i + ri + 1]) ? 1.0f : 0.0f;
	}
	rk++;
	while (rk <= k)
	{
		for (int it = 0; it < k_2; it += 2)
		{
			ri = tArray[index(k - rk, it / 2)];
			div1 = knot[i + ri + rk] - knot[i + ri];
			div2 = knot[i + ri + rk + 1] - knot[i + ri + 1];
			U1 = (abs(div1) < 1e-3f) ? 1.0f : (u - knot[i + ri]) / div1;
			U2 = (abs(div2) < 1e-3f) ? 1.0f : (knot[i + ri + rk + 1] - u) / div2;
			uArray[it / 2] = U1 * uArray[it] + U2 * uArray[it + 1];
		}
		k_2 /= 2;
		rk++;
	}
	return uArray[0];
}

inline int index(int const k, int const it)
{
	return (int)pow(2, k) - 1 + it;
}