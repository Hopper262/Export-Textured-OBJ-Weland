using Hopper;
using Weland;
using Gtk;
using System;
using System.IO;
using System.Collections.Generic;

public class Plugin {

    public static bool Compatible() { 
	return true;
    }

    public static string Name() {
	return "Export Textured OBJ...";
    }


  public static void GtkRun(Editor editor) {
    Window[] all_top = Window.ListToplevels();
    Window frontmost = all_top[1];
    if (frontmost == null) {
      frontmost = all_top[0];
    }

    FileChooserDialog d = new FileChooserDialog("Export OBJ as", frontmost, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
    d.SetCurrentFolder(Weland.Weland.Settings.GetSetting("LastSave/Folder", Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
    d.CurrentName = editor.Level.Name + ".obj";
    d.DoOverwriteConfirmation = true;
    try {
  if (d.Run() == (int) ResponseType.Accept) {
      HopOBJExporter exporter = new HopOBJExporter(editor.Level);
      exporter.Export(d.Filename);
      
      Weland.Weland.Settings.PutSetting("LastSave/Folder", Path.GetDirectoryName(d.Filename));
  }
    } catch (Exception e) {
  MessageDialog m = new MessageDialog(frontmost, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "An error occured while exporting.");
  m.Title = "Export error";
  m.SecondaryText = string.Concat(e.Message, e.StackTrace);
  m.Run();
  m.Destroy();
    }
    d.Destroy();
	}
	
}
