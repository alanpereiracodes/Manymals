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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Static class to handle generic single touch input, falling back to mouse
/// input if touch is not supported.
/// </summary>
public static class TouchInput
{
    /// <summary>
    /// Checks if a single touch (if supported) or the left mouse (otherwise)
    /// was just pressed down in current frame.
    /// </summary>
    /// <returns>Returns true if the touch/mouse button was just pressed down,
    /// and false otherwise.</returns>
    public static bool IsTouchDown()
    {
        if(Input.touchSupported)
        {
            if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Began;
            }
            else
                return false;
        }
        else
            return Input.GetMouseButtonDown(0);
    }

    /// <summary>
    /// Checks if a single touch (if supported) or the left mouse (otherwise)
    /// was just released in current frame.
    /// </summary>
    /// <returns>Returns true if the touch/mouse button was just released, and false
    /// otherwise.</returns>
    public static bool IsTouchUp()
    {
        if(Input.touchSupported)
        {
            if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            else
                return false;
        }
        else
            return Input.GetMouseButtonUp(0);
    }

    /// <summary>
    /// Checks if a single touch (if supported) or the left mouse (otherwise)
    /// is held down in current frame.
    /// </summary>
    /// <returns>Returns true if the touch/mouse button is held down, and false
    /// otherwise.</returns>
    public static bool IsTouching()
    {
        if(Input.touchSupported)
        {
            if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Began
                    || touch.phase == TouchPhase.Stationary
                    || touch.phase == TouchPhase.Moved;
            }
            else
                return false;
        }
        else
            return Input.GetMouseButton(0);
    }

    /// <summary>
    /// Gets the position of a single touch (if supported) or mouse (otherwise) in
    /// screen coordinates.
    /// </summary>
    /// <returns>Vector2 with the touch/mouse position in screen coordinates. In
    /// targets supporting touch, if there is no touch active the position returned
    /// is a `Vector2(-1, -1)`. In targets not supporting touch, the mouse position
    /// is always returned disregarding if the mouse is pressed or not.</returns>
    public static Vector2 GetTouchPosition()
    {
        if(Input.touchSupported)
        {
            if(IsTouching())
                return Input.GetTouch(0).position;
            else
                return new Vector2(-1, -1);
        }
        else
            return Input.mousePosition;
    }

    /// <summary>
    /// Checks if the current touch position hits any UI object.
    /// </summary>
    /// <returns>True if the touch position is hitting an UI object, false otherwise.</returns>
    public static bool IsTouchingUI()
    {
        if(!IsTouching())
            return false;

        try
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = GetTouchPosition();
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            if(results.Count > 0)
                return true;
            else
                return false;
        }
        catch(System.Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Check if the the given collider is being touched.
    /// </summary>
    /// <param name="target">The collider to be checked for a touch.</param>
    /// <param name="distance">The distance in space unities for the raycast. The default is 1f, which
    /// should be enough for game input detection in most cases.</param>
    /// <param name="layerMask">The layer mask to use for the collision detection. The default is -1,
    /// meaning all layers.</param>
    /// <param name="useRaycastAll">Indication if a RaycastAll should be used instead of a
    /// simple raycast. The default is false. When this argument is set to true, all colliders
    /// underneath the touch position will be able to trigger the hit checking and the function
    /// will return true if *any* collider in the same owner object as the target's is hit. Otherwise,
    /// only if the exact collider given in the parameter is hit will cause this function to return true.</param>
    /// <returns>Returns true if the user is touching or clicking on the collider, and false otherwise.</returns>
    public static bool IsTouchingCollider(Collider2D target, int layerMask = -1, float distance = 1f, bool useRaycastAll = false)
    {
        if(!IsTouching())
            return false;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(GetTouchPosition());
        if(useRaycastAll)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Camera.main.transform.forward, distance, layerMask);
            foreach(RaycastHit2D hit in hits)
                if(hit.collider != null && hit.collider.name == target.name)
                    return true;
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Camera.main.transform.forward, distance, layerMask);
            if(hit.collider != null && hit.collider == target)
                return true;
        }
        return false;
    }
}
