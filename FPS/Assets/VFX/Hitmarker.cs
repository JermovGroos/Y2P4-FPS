using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    public Image hitmarker;

    public AnimationCurve opacityOverTime;
    private float _time = 50;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            HitMarker();
        }

        _time += 1 * Time.deltaTime;

        hitmarker.color = new Color(255,255,255,opacityOverTime.Evaluate(_time));

    }

    public void HitMarker()
    {
        _time = 0;
    }
}
