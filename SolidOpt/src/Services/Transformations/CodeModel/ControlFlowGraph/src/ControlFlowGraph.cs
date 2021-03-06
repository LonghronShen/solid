/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SolidOpt.Services.Transformations.CodeModel.ControlFlowGraph
{
  /// <summary>
  /// Implementation of a Control Flow Graph structure. The CFG represents the
  /// the method body as a graph, which consist of nodes. Every node of the CFG
  /// contains linear block of CIL instructions. A node can have successors and
  /// predecessors, which represent the explicit change (branches) of the 
  /// control flow.
  /// </summary>
  public class ControlFlowGraph<T> {
    
    #region Fields & Properties
    
    private BasicBlock<T> root;
    public BasicBlock<T> Root {
      get { return root; }
    }

    private List<BasicBlock<T>> rawBlocks = null;
    public List<BasicBlock<T>> RawBlocks {
      get { return this.rawBlocks; }
    }

    private MethodDefinition methodDefinition;
    public MethodDefinition MethodDefinition {
      get { return methodDefinition; }
    }
    
    #endregion

    public ControlFlowGraph(BasicBlock<T> root, List<BasicBlock<T>> rawBlocks, MethodDefinition methodDefinition)
    {
      this.root = root;
      this.rawBlocks = rawBlocks;
      this.methodDefinition = methodDefinition;
    }

    public override string ToString ()
    {
      StringBuilder sb = new StringBuilder();
      
      foreach (BasicBlock<T> block in RawBlocks) {
       sb.AppendLine(String.Format("block {0}:", block.Name));
       sb.AppendLine(String.Format("  kind: {0}", block.Kind.ToString().ToLower()));
       sb.AppendLine("  body:");
       foreach (T instruction in block)
         sb.AppendLine(String.Format("    {0}", instruction.ToString()));
      
       if (block.Successors != null && block.Successors.Count > 0) {
         sb.AppendLine("  successors:");
         foreach (BasicBlock<T> succ in block.Successors) {
           sb.AppendLine(String.Format("    block {0}", succ.Name));
         }
       }
      
       if (block.Predecessors != null && block.Predecessors.Count > 0) {
         sb.AppendLine("  predecessors:");
         foreach (BasicBlock<T> pred in block.Predecessors) {
           sb.AppendLine(String.Format("    block {0}", pred.Name));
         }
       }
      }
      
      return sb.ToString();
    }
  }
}
