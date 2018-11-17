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
/// Controller of the game actions.
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>
    /// Collider of the spawning area over the background image, where new animals
    /// are instantiated upon touching.
    /// Configurable in the editor.
    /// </summary>
    public BoxCollider2D spawningArea;

    /// <summary>
    /// Reference to the explosion
    /// </summary>
    public GameObject explosionPrefab;

    /// <summary>
    /// List of the animal prefabs for instantiation;
    /// Configurable in the editor.
    /// </summary>
    public List<GameObject> animalPrefabs;


    /// <summary>
    /// How much the Animal should inflate before explode
    /// </summary>
    public float inflateRatio = 1.5f;

    /// <summary>
    /// How fast the animal should inflate and reach its explosion
    /// </summary>
    public float inflateSpeed = .5f;

    /// <summary>
    /// Modifies the gravity force applied;
    /// </summary>
    public float gravityModifier = 10f;

    /// <summary>
    /// Elapsed time since the start of a touch, in seconds.
    /// </summary>
    private float touchingElapsedTime;

    /// <summary>
    /// Elapsed time since the last explosion, in seconds.
    /// </summary>
    private float inflateTime;

    /// <summary>
    /// Time to wait to execute some function after an inflate/explosion.
    /// </summary>
    private float inflateWaitTime = .3f;


    /// <summary>
    /// Position of the touch input.
    /// </summary>
    private Vector2 touchPosition;

    /// <summary>
    /// 
    /// </summary>
    private bool touchingSpawingArea;

    /// <summary>
    /// Reference to the animal being dragged.
    /// </summary>
    private AnimalController animalDragged;

    /// <summary>
    /// Minimum touching time in seconds for a drag of an animal to start.
    /// Configurable in the editor.
    /// </summary>
    public float draggingStartTime = 0.5f;

    /// <summary>
    /// For internal control of the inflating coroutine.
    /// </summary>
    private bool inflating = false;

    /// <summary>
    /// Captures the game start event to lock the screen orientation in
    /// landscape and enable the gyroscope.
    /// </summary>
    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
    }

    /// <summary>
    /// Captures the game update events to instantiate animals.
    /// </summary>
    private void Update()
    {
        if(TouchInput.IsTouchDown())
        {
            touchingElapsedTime = 0;
            touchPosition = TouchInput.GetTouchPosition();
            touchingSpawingArea = TouchInput.IsTouchingCollider(spawningArea, LayerMask.GetMask("Background"));
            animalDragged = GetTouchedAnimal();
            if(animalDragged)
            {
                animalDragged.isBeingDragged = true;
                animalDragged.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
        }
        else if(TouchInput.IsTouching())
        {
            touchingElapsedTime += Time.deltaTime;
            touchPosition = TouchInput.GetTouchPosition();
            touchingSpawingArea = TouchInput.IsTouchingCollider(spawningArea, LayerMask.GetMask("Background"));
            if(animalDragged != null && touchingElapsedTime >= draggingStartTime)
            {
                Debug.Log("Incha e Explode: " + animalDragged.name);
                if(!inflating)
                {
                    inflating = true;
                    inflateTime = 0;
                    StartCoroutine(InflateAndExplode(animalDragged.gameObject));
                }
            }

        }
        else if(TouchInput.IsTouchUp())
        {
            if(animalDragged)
            {
                animalDragged.isBeingDragged = false;
                animalDragged.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }

            if(Time.time > inflateTime && touchingSpawingArea && (!animalDragged || touchingElapsedTime < draggingStartTime))
            {
                SpawnAnimal();
            }
        }
    }

    private void FixedUpdate()
    {
        //Gravity tricks
        if(SystemInfo.supportsAccelerometer)
        {
            Vector3 tilt = Input.acceleration;
            Vector2 tilt2D = new Vector2(tilt.x, tilt.z);
            Physics2D.gravity = tilt2D.normalized * gravityModifier;
            Debug.Log(tilt);
        }

    }

    /// <summary>
    /// Gets the animal being touched.
    /// </summary>
    /// <returns>Returns the reference to the AnimalController of the animal
    /// being touched, or null if no animal has been touched.</returns>
    private AnimalController GetTouchedAnimal()
    {
        foreach(Transform child in transform)
        {
            AnimalController animal = child.GetComponent<AnimalController>();
            if(animal && TouchInput.IsTouchingCollider(animal.GetComponent<CircleCollider2D>(), LayerMask.GetMask("Animals")))
                return animal;
        }
        return null;
    }

    /// <summary>
    /// Spawns a random animal at the touched position inside the spawning area.
    /// </summary>
    private void SpawnAnimal()
    {
        int idx = Random.Range(0, animalPrefabs.Count - 1);
        GameObject animal = Instantiate(animalPrefabs[idx]);
        Vector3 pos = Camera.main.ScreenToWorldPoint(touchPosition);
        pos.z = 0;
        animal.transform.position = pos;
        animal.transform.parent = transform;
    }

    private IEnumerator InflateAndExplode(GameObject target)
    {
        //Take only X as reference because I am making assumption that all axis have the same value.
        float originalScale = target.transform.localScale.x;

        while(target.transform.localScale.x < inflateRatio && animalDragged != null && animalDragged.gameObject == target && TouchInput.IsTouching())
        {
            Debug.Log("Inflating");
            Vector3 newScale = Vector3.Lerp(target.transform.localScale, Vector3.one * (inflateRatio + .2f), inflateSpeed * Time.deltaTime);
            target.transform.localScale = newScale;
            yield return null;
        }

        if(target.transform.localScale.x >= inflateRatio && animalDragged != null && animalDragged.gameObject == target)
        {
            Debug.Log("Explode");
            Instantiate(explosionPrefab, target.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(.2f);
            Destroy(target.gameObject);
        }
        else
        {
            Debug.Log("Return to original size");
            target.transform.localScale = Vector3.one * originalScale;
        }

        inflateTime += Time.time + inflateWaitTime;
        inflating = false;
        yield break;
    }

}
