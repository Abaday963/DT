using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Интерфейс для боеприпасов
public interface IAmmunition
{
    void Launch(Vector2 force);
    void OnImpact();
}
