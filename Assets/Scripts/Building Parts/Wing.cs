using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing
{
    RectInt bounds;
    Story[] stories;
    Roof roof;

    public RectInt Bounds { get => bounds; }
    public Story[] Stories { get => stories; }
    public Roof GetRoof { get => roof; }

    public Wing(RectInt bounds)
    {
        this.bounds = bounds;
    }

    public Wing(RectInt bounds, Story[] stories, Roof roof)
    {
        this.bounds = bounds;
        this.stories = stories;
        this.roof = roof;
    }

    public override string ToString()
    {
        string wing = "Wing(" + bounds.ToString() + "):\n";
        foreach (Story s in stories)
        {
            wing += "\t" + s.ToString() + "\n";
        }
        wing += "\t" + roof.ToString() + "\n";
        return wing;
    }
}
