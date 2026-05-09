/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public partial class PopupManagement : managerNode
{
    public Dictionary<string, Window> windows;
    public Dictionary<string, Action<Window>> windowCloseHandlers;

    public override void setup()
    {
        base.setup();
        
    }
    public Window create(string name)
    {
        Window window = new Window();
        window.Name = name;
        windows[name] = window;
        window.CloseRequested += windowCloseHandlers;
    }
    public void closeWindow(Window window)
    {
        //windows.Remove()
    }
    public void closeWindows()
    {
        for (int i = 0; i < windows.Count(); i++)
        {
            
        }
    }
}*/