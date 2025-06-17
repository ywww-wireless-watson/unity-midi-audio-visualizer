using UnityEngine;
using UnityEngine.UI;

// Controls animation and state for UI and particle GameObjects.
public class AnimObjects : MonoBehaviour
{
    public int type;
    public float value;
    void Start()
    {
        switch (type)
        {
            case 0:
                GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0);
                break;
            case 1:
                break;
        }
    }
    void Update()
    {
        switch (type)
        {
            case 0:
                GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0),0.5f*Time.deltaTime);
                break;
            case 1:
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(transform.localRotation.eulerAngles,new Vector3(0, value, 0), 5*Time.deltaTime));
                if (transform.localRotation.eulerAngles.y > 72)
                {
                    transform.localRotation = Quaternion.Euler(new Vector3(0, transform.localRotation.eulerAngles.y-72, 0));
                    value -= 72;
                }
                break;
        }
    }
    // Triggers the "on" animation or state.
    public void On()
    {
        switch (type)
        {
            case 0:
                GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b,1);
                break;
            case 1:
                value += 72;
                break;
            case 2:
                GetComponent<ParticleSystem>().Play();
                break;
        }
    }
    // Triggers the "off" animation or state.
    public void Off()
    {
        switch (type)
        {
            case 0:
                GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0);
                break;
            case 1:
                break;
        }
    }
}
