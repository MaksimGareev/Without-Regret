using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPossessable
{
    void BeginPossession(float resistance);
    void UpdatePossession(Vector3 inputDirection, float resistance);
    void EndPossession();
}
