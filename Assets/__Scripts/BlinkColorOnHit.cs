using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]                                                     // a
public class BlinkColorOnHit : MonoBehaviour
{
    private static float blinkDuration = 0.1f; // # seconds to show damage      // b
    private static Color blinkColor = Color.red;

    [Header("Dynamic")]
    public bool showingColor = false;
    public float blinkCompleteTime; // Time to stop showing the color

    private Material[] materials;   // All the Materials of this & its children
    private Color[] originalColors;
    private BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponentInParent<BoundsCheck>();                       // c
                                                                              // Get materials and colors for this GameObject and its children
        materials = Utils.GetAllMaterials(gameObject);                      // d
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    // void Start() {…}  // Please delete the unused Start() method

    void Update()
    {
        if (showingColor && Time.time > blinkCompleteTime) RevertColors(); // e
    }

    void OnCollisionEnter(Collision coll)
    {
        // Check for collisions with ProjectileHero
        ProjectileHero p = coll.gameObject.GetComponent<ProjectileHero>() ?? coll.gameObject.GetComponent<ProjectileHero>();
        if (p != null)
        {                                                  // f
            if (bndCheck != null && !bndCheck.isOnScreen)
            {               // g
                return; // Don’t show damage if this is off screen
            }
            SetColors();
        }
    }

    /// <summary
    /// Sets the Albedo color (i.e., the main color) of all materials in the
    ///  materials array to blinkColor, sets showingColor to true, and sets the
    ///  time that the colors should be reverted.
    /// </summary
    public void SetColors()
    {
        foreach (Material m in materials)
        {
            m.color = blinkColor;
        }
        showingColor = true;
        blinkCompleteTime = Time.time + blinkDuration;
    }

    /// <summary
    /// Reverts all materials in the materials array back to their original color
    ///  and sets showingColor to false.
    /// </summary
    public void RevertColors()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingColor = false;
    }
}