/*
 * Copyright (c) 2018, Luiz Carlos Vieira (http://www.luiz.vieira.nom.br)
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller of the animal prefabs.
/// </summary>
public class AnimalController : MonoBehaviour
{
    /// <summary>
    /// Indicates if the animal is being dragged by the player through the spring
    /// hook in the game controller. If it is, it will not destroy itself when it
    /// gets bellow/underneath the scene's platform.
    /// </summary>
    public bool isBeingDragged = false;

    /// <summary>
    /// Square of the distance, in world unities, from the center of the world, 
    /// that will be used to trigger the destruction of animals that get too far away
    /// from the scene. The square of the distance is used to make calculations faster.
    /// </summary>
    public float squaredDistanceToDestroy = 2500f;

    /// <summary>
    /// Captures the frame update event to check for the vertical limit of the animal
    /// if not being dragged by the player, and destroy the animal instance when it gets
    /// out of screen (in order to guarentee a smooth memory management).
    /// </summary>
    private void Update()
    {
        if(!isBeingDragged && transform.position.sqrMagnitude >= squaredDistanceToDestroy)
            Destroy(this.gameObject);
	}
}
