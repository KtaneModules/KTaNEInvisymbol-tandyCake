using UnityEngine;

public class DisplayInfo {

	public Sprite inner;
	public Sprite masks;
	public Vector3 rotation;
    public int val;

    public DisplayInfo(Sprite inner, Sprite masks, Vector3 rotation, int val)
    {
        this.inner = inner;
        this.masks = masks;
        this.rotation = rotation;
        this.val = val;
    }
}
