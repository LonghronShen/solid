/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using SolidOpt.Services.Transformations.CodeModel.ControlFlowGraph;
using SolidOpt.Services.Transformations.CodeModel.ThreeAddressCode;

namespace SolidOpt.Services.Transformations.Multimodel.CFGtoTAC
{
  public class ThreeAddressCodeBuilder
  {
    private ControlFlowGraph cfg = null;

    public ThreeAddressCodeBuilder(ControlFlowGraph cfg) {
      this.cfg = cfg;
    }

    public Triplet Create() {
      List<Triplet> triplets = new List<Triplet>();
      Stack<object> simulationStack = new Stack<object>();
      Instruction instr = cfg.Root.First;
      
      object newTemp;
      
      while (instr != null) {
        switch (instr.OpCode.Code) {
          case Code.Nop:
            // Nothing to do
            break;
//          case Code.Break:
          case Code.Ldarg_0:
            simulationStack.Push(cfg.Method.Parameters[0]);
            break;
          case Code.Ldarg_1:
            simulationStack.Push(cfg.Method.Parameters[1]);
            break;
          case Code.Ldarg_2:
            simulationStack.Push(cfg.Method.Parameters[2]);
            break;
          case Code.Ldarg_3:
            simulationStack.Push(cfg.Method.Parameters[3]);
            break;
          case Code.Ldloc_0:
            simulationStack.Push(cfg.Method.Body.Variables[0]);
            break;
          case Code.Ldloc_1:
            simulationStack.Push(cfg.Method.Body.Variables[1]);
            break;
          case Code.Ldloc_2:
            simulationStack.Push(cfg.Method.Body.Variables[2]);
            break;
          case Code.Ldloc_3:
            simulationStack.Push(cfg.Method.Body.Variables[3]);
            break;
          case Code.Stloc_0:
            triplets.Add(new Triplet(TripletOpCode.Assignment, cfg.Method.Body.Variables[0], simulationStack.Pop()));
            break;
          case Code.Stloc_1:
            triplets.Add(new Triplet(TripletOpCode.Assignment, cfg.Method.Body.Variables[1], simulationStack.Pop()));
            break;
          case Code.Stloc_2:
            triplets.Add(new Triplet(TripletOpCode.Assignment, cfg.Method.Body.Variables[2], simulationStack.Pop()));
            break;
          case Code.Stloc_3:
            triplets.Add(new Triplet(TripletOpCode.Assignment, cfg.Method.Body.Variables[3], simulationStack.Pop()));
            break;
          case Code.Ldarg_S:
              simulationStack.Push(instr.Operand);
              break;
//          case Code.Ldarga_S:
          case Code.Starg_S:
            triplets.Add(new Triplet(TripletOpCode.Assignment, instr.Operand, simulationStack.Pop()));
            break;
          case Code.Ldloc_S:
            simulationStack.Push(instr.Operand);
            break;
//          case Code.Ldloca_S:
          case Code.Stloc_S:
            triplets.Add(new Triplet(TripletOpCode.Assignment, instr.Operand, simulationStack.Pop()));
            break;
          case Code.Ldnull:
              simulationStack.Push(null);
              break;
          case Code.Ldc_I4_M1:
            simulationStack.Push(-1);
            break;
          case Code.Ldc_I4_0:
            simulationStack.Push(0);
            break;
          case Code.Ldc_I4_1:
            simulationStack.Push(1);
            break;
          case Code.Ldc_I4_2:
            simulationStack.Push(2);
            break;
          case Code.Ldc_I4_3:
            simulationStack.Push(3);
            break;
          case Code.Ldc_I4_4:
            simulationStack.Push(4);
            break;
          case Code.Ldc_I4_5:
            simulationStack.Push(5);
            break;
          case Code.Ldc_I4_6:
            simulationStack.Push(6);
            break;
          case Code.Ldc_I4_7:
            simulationStack.Push(7);
            break;
          case Code.Ldc_I4_8:
            simulationStack.Push(8);
            break;
          case Code.Ldc_I4_S:
            simulationStack.Push(instr.Operand);
            break;
          case Code.Ldc_I4:
            simulationStack.Push(instr.Operand);
            break;
          case Code.Ldc_I8:
            simulationStack.Push(instr.Operand);
            break;
          case Code.Ldc_R4:
            simulationStack.Push(instr.Operand);
            break;
          case Code.Ldc_R8:
            simulationStack.Push(instr.Operand);
            break;
          case Code.Dup:
            simulationStack.Push(simulationStack.Peek()); // copy reference or copy object?
            break;
          case Code.Pop:
            simulationStack.Pop();
            break;
//          case Code.Jmp:
//          case Code.Call:
//          case Code.Calli:
          case Code.Ret:
            if (simulationStack.Count == 0)
              triplets.Add(new Triplet(TripletOpCode.Return, simulationStack.Pop()));
            else
              triplets.Add(new Triplet(TripletOpCode.Return));
            break;
//          case Code.Br_S:
//          case Code.Brfalse_S:
//          case Code.Brtrue_S:
//          case Code.Beq_S:
//          case Code.Bge_S:
//          case Code.Bgt_S:
//          case Code.Ble_S:
//          case Code.Blt_S:
//          case Code.Bne_Un_S:
//          case Code.Bge_Un_S:
//          case Code.Bgt_Un_S:
//          case Code.Ble_Un_S:
//          case Code.Blt_Un_S:
//          case Code.Br:
//          case Code.Brfalse:
//          case Code.Brtrue:
//          case Code.Beq:
//          case Code.Bge:
//          case Code.Bgt:
//          case Code.Ble:
//          case Code.Blt:
//          case Code.Bne_Un:
//          case Code.Bge_Un:
//          case Code.Bgt_Un:
//          case Code.Ble_Un:
//          case Code.Blt_Un:
//          case Code.Switch:
//          case Code.Ldind_I1:
//          case Code.Ldind_U1:
//          case Code.Ldind_I2:
//          case Code.Ldind_U2:
//          case Code.Ldind_I4:
//          case Code.Ldind_U4:
//          case Code.Ldind_I8:
//          case Code.Ldind_I:
//          case Code.Ldind_R4:
//          case Code.Ldind_R8:
//          case Code.Ldind_Ref:
//          case Code.Stind_Ref:
//          case Code.Stind_I1:
//          case Code.Stind_I2:
//          case Code.Stind_I4:
//          case Code.Stind_I8:
//          case Code.Stind_R4:
//          case Code.Stind_R8:
          case Code.Add:
            newTemp = new object(); //TODO: Implement Temp operand/localvar generation
            triplets.Add(new Triplet(TripletOpCode.Addition, newTemp, simulationStack.Pop(), simulationStack.Pop()));
            simulationStack.Push(newTemp);
            break;
//          case Code.Sub:
//          case Code.Mul:
//          case Code.Div:
//          case Code.Div_Un:
//          case Code.Rem:
//          case Code.Rem_Un:
//          case Code.And:
//          case Code.Or:
//          case Code.Xor:
//          case Code.Shl:
//          case Code.Shr:
//          case Code.Shr_Un:
//          case Code.Neg:
//          case Code.Not:
//          case Code.Conv_I1:
//          case Code.Conv_I2:
//          case Code.Conv_I4:
//          case Code.Conv_I8:
//          case Code.Conv_R4:
//          case Code.Conv_R8:
//          case Code.Conv_U4:
//          case Code.Conv_U8:
//          case Code.Callvirt:
//          case Code.Cpobj:
//          case Code.Ldobj:
          case Code.Ldstr:
            simulationStack.Push(instr.Operand);
            break;
//          case Code.Newobj:
//          case Code.Castclass:
//          case Code.Isinst:
//          case Code.Conv_R_Un:
//          case Code.Unbox:
//          case Code.Throw:
//          case Code.Ldfld:
//          case Code.Ldflda:
//          case Code.Stfld:
//          case Code.Ldsfld:
//          case Code.Ldsflda:
//          case Code.Stsfld:
//          case Code.Stobj:
//          case Code.Conv_Ovf_I1_Un:
//          case Code.Conv_Ovf_I2_Un:
//          case Code.Conv_Ovf_I4_Un:
//          case Code.Conv_Ovf_I8_Un:
//          case Code.Conv_Ovf_U1_Un:
//          case Code.Conv_Ovf_U2_Un:
//          case Code.Conv_Ovf_U4_Un:
//          case Code.Conv_Ovf_U8_Un:
//          case Code.Conv_Ovf_I_Un:
//          case Code.Conv_Ovf_U_Un:
//          case Code.Box:
//          case Code.Newarr:
//          case Code.Ldlen:
//          case Code.Ldelema:
//          case Code.Ldelem_I1:
//          case Code.Ldelem_U1:
//          case Code.Ldelem_I2:
//          case Code.Ldelem_U2:
//          case Code.Ldelem_I4:
//          case Code.Ldelem_U4:
//          case Code.Ldelem_I8:
//          case Code.Ldelem_I:
//          case Code.Ldelem_R4:
//          case Code.Ldelem_R8:
//          case Code.Ldelem_Ref:
//          case Code.Stelem_I:
//          case Code.Stelem_I1:
//          case Code.Stelem_I2:
//          case Code.Stelem_I4:
//          case Code.Stelem_I8:
//          case Code.Stelem_R4:
//          case Code.Stelem_R8:
//          case Code.Stelem_Ref:
//          case Code.Ldelem_Any:
//          case Code.Stelem_Any:
//          case Code.Unbox_Any:
//          case Code.Conv_Ovf_I1:
//          case Code.Conv_Ovf_U1:
//          case Code.Conv_Ovf_I2:
//          case Code.Conv_Ovf_U2:
//          case Code.Conv_Ovf_I4:
//          case Code.Conv_Ovf_U4:
//          case Code.Conv_Ovf_I8:
//          case Code.Conv_Ovf_U8:
//          case Code.Refanyval:
//          case Code.Ckfinite:
//          case Code.Mkrefany:
//          case Code.Ldtoken:
//          case Code.Conv_U2:
//          case Code.Conv_U1:
//          case Code.Conv_I:
//          case Code.Conv_Ovf_I:
//          case Code.Conv_Ovf_U:
//          case Code.Add_Ovf:
//          case Code.Add_Ovf_Un:
//          case Code.Mul_Ovf:
//          case Code.Mul_Ovf_Un:
//          case Code.Sub_Ovf:
//          case Code.Sub_Ovf_Un:
//          case Code.Endfinally:
//          case Code.Leave:
//          case Code.Leave_S:
//          case Code.Stind_I:
//          case Code.Conv_U:
//          case Code.Arglist:
//          case Code.Ceq:
//          case Code.Cgt:
//          case Code.Cgt_Un:
//          case Code.Clt:
//          case Code.Clt_Un:
//          case Code.Ldftn:
//          case Code.Ldvirtftn:
          case Code.Ldarg:
            simulationStack.Push(instr.Operand);
            break;
//          case Code.Ldarga:
          case Code.Starg:
            triplets.Add(new Triplet(TripletOpCode.Assignment, instr.Operand, simulationStack.Pop()));
            break;
          case Code.Ldloc:
            simulationStack.Push(instr.Operand);
            break;
//          case Code.Ldloca:
          case Code.Stloc:
              triplets.Add(new Triplet(TripletOpCode.Assignment, instr.Operand, simulationStack.Pop()));
              break;
//          case Code.Localloc:
//          case Code.Endfilter:
//          case Code.Unaligned:
//          case Code.Volatile:
//          case Code.Tail:
//          case Code.Initobj:
//          case Code.Constrained:
//          case Code.Cpblk:
//          case Code.Initblk:
//          case Code.No:
//          case Code.Rethrow:
//          case Code.Sizeof:
//          case Code.Refanytype:
//          case Code.Readonly:

          default:
            throw new NotImplementedException(instr.OpCode.ToString());
        }
        instr = instr.Next;
      }

      //triplets.Add(CreateTriplet());
      // Here should be the implementation
      return triplets[0];
    }
  }

/*L0: flag = 0;
L1: PushParam "A"
L2: tmp0 = call s.StartsWith();
L3: ifFalse tmp0 goto L5;
L4: flag = s.Length >= 3;
L5: flag = false;
L6: ret flag;
*/
}
