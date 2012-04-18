///*
// * $Id$
// * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
// * For further details see the nearest License.txt
// */

using SolidOpt.Services;
using SolidOpt.Services.Transformations.Multimodel;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Mono.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class MainWindow: Gtk.Window
{
  private PluginServiceContainer plugins = new PluginServiceContainer();
  private List<String> fileNames = new List<String>();
  private AssemblyDefinition curAssembly = null;
  private ModuleDefinition curModule = null;
  private TypeDefinition curType = null;

	public MainWindow(): base(Gtk.WindowType.Toplevel)
	{
    // That's a hack because of the designer. If one needs to attach an event the designer attaches
    // it in the end of the file after the call to Initialize. Works for the most of the events
    // but not for events like Realize which happen in the initialization. This function is used
    // to attach the event handlers before the initialization part.
    PreBuild();
		Build();
	}

	protected void OnDeleteEvent(object sender, Gtk.DeleteEventArgs a)
	{
    SaveEnvironment();
		Gtk.Application.Quit();
		a.RetVal = true;
	}

	protected void OnOpenActionActivated(object sender, System.EventArgs e)
  {
    var fc = new Gtk.FileChooserDialog("Choose the file to open",
                                        this, Gtk.FileChooserAction.Open,
                                        "Cancel", Gtk.ResponseType.Cancel,
                                        "Open", Gtk.ResponseType.Accept);
    try {
      fc.SelectMultiple = true;
      fc.SetCurrentFolder(Environment.CurrentDirectory);
      if (fc.Run() == (int)Gtk.ResponseType.Accept) {
        List<string> filesToLoad = new List<string>();
        filesToLoad.AddRange(fc.Filenames);
        for (uint i = 0; i < fc.Filenames.Length; i++)
          if (!fileNames.Contains(fc.Filenames[i]))
            fileNames.Add(fc.Filenames[i]);
          else {
            var msg = new Gtk.MessageDialog(null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                                            Gtk.ButtonsType.Ok,
                                            String.Format("Filename {0} already loaded.",
                                                          fc.Filenames[i]));
            filesToLoad.Remove(fc.Filenames[i]);
            msg.Run();
            msg.Destroy();
        }
        LoadFilesInTreeView(filesToLoad.ToArray());
      }
    } finally {
      fc.Destroy();
    }
  }

	protected void OnExitActionActivated(object sender, System.EventArgs e)
	{
    SaveEnvironment();
		Gtk.Application.Quit();
	}

  protected void OnRealized(object sender, System.EventArgs e)
  {
    LoadEnvironment();
    LoadRegisteredPlugins();
  }

  private void LoadEnvironment() {
    string curEnv = System.IO.Path.Combine(Environment.CurrentDirectory, "Current.env");
    if (System.IO.File.Exists(curEnv)) {
      fileNames.AddRange(System.IO.File.ReadAllLines(curEnv));
      LoadFilesInTreeView(fileNames.ToArray());
    }
  }

  private void SaveEnvironment() {
    string curEnv = System.IO.Path.Combine(Environment.CurrentDirectory, "Current.env");
    System.IO.File.WriteAllLines(curEnv, fileNames.ToArray());

    string pluginsEnv = System.IO.Path.Combine(Environment.CurrentDirectory, "Plugins.env");

    foreach(PluginInfo pInfo in plugins.plugins) {
      System.IO.File.AppendText(pluginsEnv).WriteLine(pInfo.codeBase);
    }
  }

  private void LoadFilesInTreeView(string [] files) {
    Gtk.TreeViewColumn col = new Gtk.TreeViewColumn();

    Gtk.CellRendererText colTitleCell = new Gtk.CellRendererText();
    col.PackStart(colTitleCell, true);

    col.AddAttribute(colTitleCell, "text", 0);

    if (assemblyView.GetColumn(0) != null)
      assemblyView.Columns[0] = col;
    else
      assemblyView.AppendColumn(col);

    Gtk.TreeStore store = assemblyView.Model as Gtk.TreeStore;
    if (store == null)
     store = new Gtk.TreeStore(typeof(string));

    foreach (string file in files) {
      store.AppendValues(System.IO.Path.GetFileName(file));
    }

    assemblyView.Model = store;
    assemblyView.ShowAll();
  }

  private void LoadRegisteredPlugins() {
    string registeredPlugins =
        System.IO.Path.Combine(Environment.CurrentDirectory, "Plugins.env");
    if (System.IO.File.Exists(registeredPlugins)) {
      foreach (string s in System.IO.File.ReadAllLines(registeredPlugins))
        if (System.IO.File.Exists(s))
          plugins.AddPlugin(s);
    }

    plugins.LoadPlugins();
    //IDecompile<MethodDefinition, ControlFlowGraph> ILtoCfgTransformer =
    //   plugins.GetService<IDecompile<MethodDefinition, ControlFlowGraph>>();


  }

  protected virtual void PreBuild() {
    this.Realized += new global::System.EventHandler (this.OnRealized);
  }

  protected void OnAssemblyViewRowActivated (object o, Gtk.RowActivatedArgs args)
  {
    Gtk.TreeIter iter;
    assemblyView.Model.GetIter(out iter, args.Path);
    string s = (string) assemblyView.Model.GetValue(iter, 0);

    switch(args.Path.Depth) {
    case 1:
        foreach (string f in fileNames) {
          if (System.IO.Path.GetFileName(f) == s) {
            curAssembly = AssemblyDefinition.ReadAssembly(f);
            Debug.Assert(curAssembly != null, "Assembly cannot be null.");
            AttachSubTree(assemblyView.Model, iter, curAssembly.Modules.ToArray());
          }
        }
        break;
    case 2:
        foreach (ModuleDefinition mDef in curAssembly.Modules) {
          if (mDef.Name == s) {
            curModule = mDef;
            AttachSubTree(assemblyView.Model, iter, mDef.Types.ToArray());
          }
        }
        break;

    case 3:
        Debug.Assert(curModule != null, "CurModule is null!?");
        foreach (TypeDefinition tDef in curModule.Types) {
          if (tDef.FullName == s) {
            curType = tDef;
            //AttachSubTree(assemblyView.Model, iter, tDef.Fields.ToArray());
            AttachSubTree(assemblyView.Model, iter, tDef.Methods.ToArray());
            //AttachSubTree(assemblyView.Model, iter, tDef.Events.ToArray());
          }
        }
        break;
      case 4:
        Debug.Assert(curType != null, "CurType is null!?");
        foreach (MethodDefinition mDef in curType.Methods)
          if (mDef.ToString() == s)
            DumpMember(mDef);
        break;
    }
    assemblyView.ShowAll();
    ShowAll();
  }

  protected void AttachSubTree(Gtk.TreeModel model, Gtk.TreeIter parent, object[] elements)
  {
    Gtk.TreeStore store = model as Gtk.TreeStore;
    Debug.Assert(store != null, "TreeModel shouldn't be flat");

    // remove the values if they were added before.
    Gtk.TreePath path = store.GetPath(parent);
    path.Down();
    Gtk.TreeIter iter;
    while (store.GetIter(out iter, path))
      store.Remove(ref iter);

    // Add the elements to the tree view.
    for (uint i = 0; i < elements.Length; ++i) {
      store.AppendValues(parent, elements[i].ToString());
    }
  }

  protected void DumpMember(IMemberDefinition member)
  {
    disassemblyText.Buffer.Clear();

    Gtk.TextIter textIter = disassemblyText.Buffer.EndIter;
    MethodDefinition method = member as MethodDefinition;

    if (method != null) {
      SolidReflector.ILWriter writer = new SolidReflector.ILWriter(disassemblyText.Buffer);
      writer.Indent();
      writer.WriteMethodAttribute(".method");

      if (method.IsPublic)
        writer.WriteMethodAttribute("public");
      if (method.IsPrivate)
        writer.WriteMethodAttribute("private");
      if (method.IsHideBySig)
        writer.WriteMethodAttribute("hidebysig");
      if (method.IsStatic)
        writer.WriteMethodAttribute("static");
      else
        writer.WriteMethodAttribute("instance");

      writer.WriteType(method.ReturnType.Name);
      writer.WriteName(method.Name);
      writer.Write(" (");
      if (method.Parameters.Count > 0) {
        writer.WriteType(method.Parameters[0].ParameterType.ToString());
        writer.WriteName(method.Parameters[0].Name.ToString());
      }
      writer.Write(") ");
      if (method.IsIL)
        writer.WriteImplementationAttribute("cil");
      else if (method.IsNative)
        writer.WriteImplementationAttribute("native");

      if (method.IsManaged)
        writer.WriteImplementationAttribute("managed");
      else if (method.IsUnmanaged)
        writer.WriteImplementationAttribute("unmanaged");

      writer.NewLine();
      writer.Write("{");
      writer.NewLine();

      if (method.Body.Variables.Count > 0) {
        writer.WriteKeyword(".locals init");
        writer.Write("(");
        for (int i = 0; i < method.Body.Variables.Count; i++) {
          writer.WriteType(method.Body.Variables[i].VariableType.Name.ToString());
          writer.WriteName(method.Body.Variables[i].ToString());
          if (i + 1 != method.Body.Variables.Count)
              writer.Write(", ");
        }
        writer.Write(")");
        writer.NewLine();
      }

      foreach (Instruction inst in method.Body.Instructions) {
        //if (method.Body.HasExceptionHandlers) {
        foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
          if (handler.FilterStart == inst) {
            writer.Indent();
            writer.WriteExceptionDirective(".filter {");
          }

          if (handler.FilterEnd == inst) {
            writer.Outdent();
            writer.WriteExceptionDirective("}");
          }

          if (handler.TryStart == inst) {
            writer.WriteExceptionDirective(".try {");
            writer.Indent();
          }

          if (handler.TryEnd == inst) {
            writer.Outdent();
            writer.WriteExceptionDirective("}");
          }

          if (handler.HandlerStart == inst) {
            writer.WriteExceptionDirective(handler.HandlerType.ToString() + " {");
            writer.Indent();
          }

          if (handler.HandlerEnd == inst) {
            writer.Outdent();
            writer.WriteExceptionDirective("}");
          }
        }
        writer.WriteInstruction(inst.ToString());

      }

      writer.Outdent();
      writer.Write("}");

      return;
    }

    EventDefinition evt = member as EventDefinition;
    if (evt != null) {
      foreach (MethodDefinition mDef in evt.OtherMethods) {
         disassemblyText.Buffer.Insert(ref textIter, mDef.ToString() + "\n");
      }
      return;
    }

    FieldDefinition field = member as FieldDefinition;
    if (field != null) {
      disassemblyText.Buffer.Insert(ref textIter, field.ToString() + "\n");
      return;
    }

  }

  private void AppendLabel (System.Text.StringBuilder builder, Instruction instruction) {
    builder.Append ("IL_");
    builder.Append (instruction.Offset.ToString ("x4"));
  }

  protected void OnCombobox6Changed (object sender, System.EventArgs e)
  {
    Gtk.TreeIter iter;
    combobox6.GetActiveIter(out iter);
    string val = (string)combobox6.Model.GetValue(iter, 0);
    if (val == "IL") {

    }
    else if (val == "CFG") {

    }

  }
}