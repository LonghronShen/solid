// RUN: "@ILASM@" /DLL @TEST_CASE@
.assembly TestCase {}

.class public auto ansi beforefieldinit TestCase
       extends [mscorlib]System.Object
{
  //  public static bool Main(int x) {
  //    bool flag;
  //    if (x <= 3) {
  //      flag = x > 100;
  //      return flag;
  //    } else {
  //        if (x < 42) {
  //          flag = 1;
  //          return flag;
  //        } else {
  //            flag = x > 100;
  //            return flag;
  //        }
  //    }
  //  }
	.method public hidebysig static bool Main(int32 x) cil managed
	{
		.locals init (bool local0)
		IL_0000: ldarg.0
		IL_0001: ldc.i4.3
		IL_0002: ble.s IL_0009
		IL_0004: ldarg.0
		IL_0005: ldc.i4.s 42
		IL_0007: blt.s IL_0010
		IL_0009: ldarg.0
		IL_000a: ldc.i4.s 100
		IL_000c: cgt
		IL_000e: br.s IL_0011
		IL_0010: ldc.i4.1
		IL_0011: stloc.0
		IL_0012: br.s IL_0014
		IL_0014: ldloc.0
		IL_0015: ret
	}
}
