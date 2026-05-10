using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class ColorManagement : managerNode
{
    public event EventHandler<ColorChangedEvent> colorChanged;
    public event EventHandler colorLibraryChanged;
    public Dictionary<string, Color> colors {get; set;}
    public List<string> savedColorNames {get; private set;}
    public List<string> recentColorNames {get; private set;}
    public colorPicker picker {get; private set;}
    public string activeColorName = "active";
    public string pickerColorName = "colorSlot0";
    PackedScene colorPickerScene;
    ColorPicker activePicker;

    public override void setup()
    {
        colors = new Dictionary<string, Color>();
        savedColorNames = new List<string>();
        recentColorNames = new List<string>();

        colors[""] = new Color(0, 0, 0, 0);
        colors["black"] = new Color(0, 0, 0);
        colors["grey"] = new Color(0.5f, 0.5f, 0.5f);
        addSavedColor("colorSlot0", new Color(0.02f, 0.02f, 0.02f, 1f));
        addSavedColor("colorSlot1", new Color(0.85f, 0.1f, 0.08f, 1f));
        addSavedColor("colorSlot2", new Color(0.08f, 0.28f, 0.9f, 1f));
        addSavedColor("colorSlot3", new Color(1f, 0.72f, 0.12f, 1f));
        picker = new colorPicker();
        colorPickerScene = mAccess.uiManager.scenes["colorPicker"];
        setPickerColorName(pickerColorName);
    }
    
    public void addColor(string name, Color color)
    {
        colors[name] = color;
    }
    public void updateColor(string name, Color color)
    {
        colors[name] = color;
        colorChanged?.Invoke(this, new ColorChangedEvent(name, color));
    }
    public string savePickerColor()
    {
        string name = "savedColor" + savedColorNames.Count;
        addSavedColor(name, picker.color);
        colorLibraryChanged?.Invoke(this, EventArgs.Empty);
        return name;
    }
    public void applySavedColor(string savedColorName)
    {
        Color color = getColor(savedColorName);
        picker.setColor(color);
        setStoredColor(pickerColorName, color);
    }
    void addSavedColor(string name, Color color, bool recent = false)
    {
        colors[name] = color;
        if (!savedColorNames.Contains(name))
        {
            savedColorNames.Add(name);
        }
        if (recent)
        {
            addRecentColor(name);
        }
    }
    void addRecentColor(string name)
    {
        recentColorNames.Remove(name);
        recentColorNames.Insert(0, name);
        if (recentColorNames.Count > 8)
        {
            recentColorNames.RemoveAt(recentColorNames.Count - 1);
        }
    }
    public void setPicker(float radius, float saturation, float value, float translucence)
    {
        picker.rotation = radius % 1f;
        if (picker.rotation < 0f)
        {
            picker.rotation += 1f;
        }
        picker.saturation = Mathf.Clamp(saturation, 0f, 1f);
        picker.brightness = Mathf.Clamp(value, 0f, 1f);
        picker.transluscence = Mathf.Clamp(translucence, 0f, 1f);
        setStoredColor(pickerColorName, picker.color);
    }
    public void setPickerBrightness(float brightness)
    {
        setPicker(picker.rotation, picker.saturation, brightness, picker.transluscence);
    }
    public void setPickerTransluscence(float translucence)
    {
        setPicker(picker.rotation, picker.saturation, picker.brightness, translucence);
    }
    public void setActiveColor(Color color)
    {
        colors[activeColorName] = color;
        colorChanged?.Invoke(this, new ColorChangedEvent(activeColorName, color));
    }
    public void setActiveColorName(string name)
    {
        setActiveColor(getColor(name));
    }
    public void setStoredColor(string name, Color color)
    {
        colors[name] = color;
        colors[activeColorName] = color;
        colorChanged?.Invoke(this, new ColorChangedEvent(name, color));
        colorChanged?.Invoke(this, new ColorChangedEvent(activeColorName, color));
    }
    public void setPickerColorName(string name)
    {
        pickerColorName = name;
        Color color = getColor(name);
        picker.setColor(color);
        setActiveColor(color);
    }
    public Color getActiveColor()
    {
        return colors[activeColorName];
    }
    public void openColorPicker(Control owner, string colorName, Vector2 globalPosition)
    {
        activePicker = colorPickerScene.Instantiate<ColorPicker>();
        mAccess.windowManager.openWindowAt("Color Picker", activePicker, globalPosition, "staticMenu", false);
        activePicker.openForColor(colorName);
    }
    public void commitPickerColorToRecent()
    {
        string name = "recentColor" + recentColorNames.Count + "_" + Time.GetTicksMsec();
        colors[name] = picker.color;
        addRecentColor(name);
        colorLibraryChanged?.Invoke(this, EventArgs.Empty);
    }
    public Color getColor(string color)
    {
        color = color?? "";
        if (colors.ContainsKey(color))
        {
            return colors[color];
        }
        else if (color[0] == '#')
        {
            return new Color(color);
        }
        else
        {
            GD.Print("invalid color: " + color);
            return colors[""];
        }
    }
    public class colorPicker
    {
        public float rotation = 0f;
        public float saturation = 0f;
        public float transluscence = 1f;
        public float brightness = 1f;
        public Color color
        {
            get
            {
                Color c = Color.FromHsv(rotation, saturation, brightness);
                c.A = transluscence;
                return c;
            }
        }
        public void setColor(Color color)
        {
            transluscence = color.A;
            brightness = Math.Max(Math.Max(color.R, color.G), color.B);

            float min = Math.Min(Math.Min(color.R, color.G), color.B);
            float delta = brightness - min;
            saturation = brightness == 0f ? 0f : delta / brightness;
            if (delta == 0f)
            {
                rotation = 0f;
            }
            else if (brightness == color.R)
            {
                rotation = ((color.G - color.B) / delta) / 6f;
            }
            else if (brightness == color.G)
            {
                rotation = (((color.B - color.R) / delta) + 2f) / 6f;
            }
            else
            {
                rotation = (((color.R - color.G) / delta) + 4f) / 6f;
            }

            if (rotation < 0f)
            {
                rotation += 1f;
            }
        }
    }
}

public class ColorChangedEvent : EventArgs
{
    public string name {get;}
    public Color color {get;}
    public ColorChangedEvent(string nameArg, Color colorArg)
    {
        name = nameArg;
        color = colorArg;
    }
}
