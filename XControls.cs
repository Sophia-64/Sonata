/*-----------------------------------------------------------|
 |XControls.cs
 |Author: Sophia Caldwell
 |License: 2025 CC-BY-NC-SA
 |A series of classes designed to simplify basic state machines
 |by providing a series of lookup tables operated with trivial
 |syntax. Designed to be dropped into any Unity project. Can be 
 |easily modified to work on a non-Unity CS project also.
 |
 |See README.md. 
 ------------------------------------------------------------|*/



using System.Collections.Generic;   //Lists, Dictionaries
using UnityEngine               ;   //Used for 'Component' keys

[System.Serializable]
public class BControl
{
    bool avoidTrue;

    public Dictionary<int, bool> intControls = new Dictionary<int, bool>();

    bool useStringControls = false;

    public Dictionary<string, bool> stringControls = new Dictionary<string, bool>();

    bool useComponentControls = false;

    public Dictionary<Component, bool> componentControls = new Dictionary<Component, bool>();

    bool useNullableControls   = false;

    public Dictionary<object, bool> objectControls = new Dictionary<object, bool>();

    public delegate void dOnBoolAssigned();

    /// <summary>
    /// Occurs whenever a bool in BControl is assigned, even if it is to the same value.
    /// </summary>
    public event dOnBoolAssigned OnBoolAssigned;

    public bool this[int key]
    {
        get
        {
            if (intControls.ContainsKey(key)) return intControls[key];
            return avoidTrue;
        }

        set
        {
            if (intControls.ContainsKey(key)) intControls[key] = value;
            else intControls.Add(key, value);
            if (OnBoolAssigned != null) OnBoolAssigned();
        }
    }

    public bool this[string key]
    {
        get
        {
            if (!useStringControls || !stringControls.ContainsKey(key)) return avoidTrue;
            return stringControls[key];
        }

        set
        {
            if (!useStringControls)
            {
                stringControls = new Dictionary<string, bool>();
                stringControls.Add(key, value);
                useStringControls = true;
            }
            else
            {
                if (stringControls.ContainsKey(key)) stringControls[key] = value;
                else stringControls.Add(key, value);
            }

            if (OnBoolAssigned != null) OnBoolAssigned();
        }
    }

    public bool this[Component key]
    {
        get
        {
            if (!useComponentControls || !componentControls.ContainsKey(key)) return avoidTrue;
            return componentControls[key];
        }

        set
        {
            if (!useComponentControls)
            {
                componentControls = new Dictionary<Component, bool>();
                componentControls.Add(key, value);
                useComponentControls = true;
            }
            else
            {
                if (componentControls.ContainsKey(key)) componentControls[key] = value;
                else componentControls.Add(key, value);
            }

            if (OnBoolAssigned != null) OnBoolAssigned();
        }
    }

    public bool this[object key]
	{
        get
		{
            if (!useNullableControls || !objectControls.ContainsKey(key)) return avoidTrue;
            return objectControls[key];
		}

        set
		{
            if (!useNullableControls)
			{
                objectControls = new Dictionary<object, bool>();
                objectControls.Add(key, value);
                useNullableControls = true;
			}
            else
			{
                if (objectControls.ContainsKey(key)) objectControls[key] = value;
                else objectControls.Add(key, value);
			}
		}
	}

    // Creates a BControl with a root bool of 0.
    public static implicit operator BControl(bool b)
    {
        BControl bc = new BControl();
        bc.intControls.Add(0, b);
        bc.avoidTrue = b;
        return bc;
    }

    //Explanation for the main line in each loop: (kvp.Value ^ bc.xControls) return !bc.avoidTrue.
    //If avoidTrue is set, we will return false if any value is false
    //Then (bool XOR true) is true if the bool is false, and we'll return false.
    //If avoidTrue is false, we will return true if any value is true.
    //We have (bool XOR false), which is true if any value is true, then we'll return true. 
    public static implicit operator bool(BControl bc)
    {
        foreach (var kvp in bc.intControls)
        {
            if (kvp.Value ^ bc.avoidTrue) return !bc.avoidTrue;
        }

        if (bc.useStringControls)
        {
            foreach (var kvp in bc.stringControls)
            {
                if (kvp.Value ^ bc.avoidTrue) return !bc.avoidTrue;
            }
        }

        if (bc.useComponentControls)
        {
            foreach (var kvp in bc.componentControls)
            {
                if (kvp.Value ^ bc.avoidTrue) return !bc.avoidTrue;
            }
        }

        return bc.avoidTrue;
    }


    public override string ToString ()
	{
		return "BControl -> " + ((bool) this);
	}

	public string ToLongString ()
	{
		string s = "[BControl -> Avoid " + avoidTrue + "]: " + ((bool) this).ToString() + " => Contents follow. \n";
		
		foreach (var kvp in intControls) s += kvp.Key + "> " + kvp.Value + "\n";
		if (useStringControls) foreach (var kvp in stringControls) s += kvp.Key + "> " + kvp.Value + "\n";
		if (useComponentControls) foreach (var kvp in componentControls) s += kvp.Key.name + "> " + kvp.Value + "\n";
		return s;
	}
}

[System.Serializable]
public class IControl
{
	public Dictionary<string, int> Controls = new Dictionary<string, int>();

	public IControl (IControlMode mode)
	{
		this.mode = mode;
	}

	//When cast to an int, does the largest int return, or the smallest?
	public enum IControlMode
	{
		Minimum,
		Maximum
	}

	public IControlMode mode = IControlMode.Maximum;

	public int this [string key]
	{
		get	
		{
			int i;
			if (Controls.TryGetValue(key, out i)) return i;
			else 
			{
				int val = 0;
				if (mode == IControlMode.Maximum) val = int.MinValue;
				else if (mode == IControlMode.Minimum) val = int.MaxValue;

				Controls.Add(key, val);
				return val;
			}
		}

		set
		{
			int i;
			if (Controls.ContainsKey(key)) Controls[key] = value; 
			else Controls.Add(key, value);
		}
	}

	public static implicit operator IControl (IControlMode mode)
	{
		return new IControl(mode);
	}

	public static implicit operator int (IControl ic)
	{
		int i = -1; 

		if (ic.mode == IControlMode.Maximum)
		{
			int max = int.MinValue;

			foreach (var kvp in ic.Controls)
			{
				if (kvp.Value > max) max = kvp.Value;
			}
			
			i = max;
		}

		else if (ic.mode == IControlMode.Minimum)
		{
			int min = int.MaxValue;

			foreach (var kvp in ic.Controls)
			{
				if (kvp.Value < min) min = kvp.Value;
			}
				
			i = min;
		}
		
		return i;
	}
}


/// <summary>
/// Arbitrary X Control returning the T with the highest order that lacks a condition.
/// Example: XControl\<GameObject\> ObjectMap = false; 
/// somewhere() { ObjectMap[this] = (true, 10, this.gameObject); }
/// elsewhere() { ObjectMap["hi"] = (true, 1, steve); }
/// GameObject func() {  if (ObjectMap.Item1) return ObjectMap.Item2; }
/// func will return the first object because it is priority 10. 
/// </summary>
public class XControl<T>
{
    public Dictionary<string, (bool, int, T)> xmap_str = new Dictionary<string, (bool, int, T)>();
    public Dictionary<int, (bool, int, T)> xmap_int = new Dictionary<int, (bool, int, T)>();
    public Dictionary<Component, (bool, int, T)> xmap_cmp = new Dictionary<Component, (bool, int, T)>();
    public Dictionary<object, (bool, int, T)> xmap_null = new Dictionary<object, (bool, int, T)>();

    bool avoidTrue;

    public static implicit operator XControl<T>(bool magnetValue)
    {
        XControl<T> ret = new XControl<T>();
        ret.avoidTrue = magnetValue;
        return ret;
    }

    public static implicit operator (bool, T)(XControl<T> ctrl)
    {
        bool res = ctrl.avoidTrue;
        int max = int.MinValue;
        T val = default(T);

        foreach (var kvp in ctrl.xmap_null)
        {
            bool active = kvp.Value.Item1;
            int priority = kvp.Value.Item2;
            T result = kvp.Value.Item3;

            if (active ^ ctrl.avoidTrue && priority > max)
            {
                res = !ctrl.avoidTrue;
                max = priority;
                val = result;
            }
        }

        foreach (var kvp in ctrl.xmap_cmp)
        {
            bool active = kvp.Value.Item1;
            int priority = kvp.Value.Item2;
            T result = kvp.Value.Item3;

            if (active ^ ctrl.avoidTrue && priority > max)
            {
                res = !ctrl.avoidTrue;
                max = priority;
                val = result;
            }
        }


        foreach (var kvp in ctrl.xmap_int)
        {
            bool active = kvp.Value.Item1;
            int priority = kvp.Value.Item2;
            T result = kvp.Value.Item3;

            if (active ^ ctrl.avoidTrue && priority > max)
            {
                res = !ctrl.avoidTrue;
                max = priority;
                val = result;
            }
        }

        foreach (var kvp in ctrl.xmap_str)
        {
            bool active = kvp.Value.Item1;
            int priority = kvp.Value.Item2;
            T result = kvp.Value.Item3;

            if (active ^ ctrl.avoidTrue && priority > max)
            {
                res = !ctrl.avoidTrue;
                max = priority;
                val = result;
            }
        }

        return (res, val);
    }

    public (bool, int, T) this[int key]
    {
        get
        {
            if (xmap_int.ContainsKey(key)) return xmap_int[key];
            return (avoidTrue, int.MinValue, default(T));
        }

        set
        {
            if (xmap_int.ContainsKey(key)) xmap_int[key] = value;
            else xmap_int.Add(key, value);
        }
    }

    public (bool, int, T) this[string key]
    {
        get
        {
            if (xmap_str.ContainsKey(key)) return xmap_str[key];
            return (avoidTrue, int.MinValue, default(T));
        }

        set
        {
            if (xmap_str.ContainsKey(key)) xmap_str[key] = value;
            else xmap_str.Add(key, value);
        }
    }

    public (bool, int, T) this[object key]
    {
        get
        {
            if (xmap_null.ContainsKey(key)) return xmap_null[key];
            return (avoidTrue, int.MinValue, default(T));
        }

        set
        {
            if (xmap_null.ContainsKey(key)) xmap_null[key] = value;
            else xmap_null.Add(key, value);
        }
    }

    public (bool, int, T) this[Component key]
    {
        get
        {
            if (xmap_cmp.ContainsKey(key)) return xmap_cmp[key];
            return (avoidTrue, int.MinValue, default(T));
        }

        set
        {
            if (xmap_cmp.ContainsKey(key)) xmap_cmp[key] = value;
            else xmap_cmp.Add(key, value);
        }
    }
}

[System.Serializable]
public class MHXDatum<T, R>
{
    public T ID;
    public R Value;
}

[System.Serializable]
public class MHXFlagColor : MHXDatum<MHXTileFlags, Color> { }

[System.Serializable]
public class MHXDT<T, R>
{
    public List<MHXDatum<T,R>> Data;
    
}
