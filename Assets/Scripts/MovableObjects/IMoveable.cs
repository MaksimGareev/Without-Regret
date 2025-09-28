using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    void Grab(Transform grabPoint);
    void Release();
}
